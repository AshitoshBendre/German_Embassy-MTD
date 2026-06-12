using System;
using System.Collections.Generic;
using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// INTERFACES — implement these on your own MonoBehaviours to plug in behaviour
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Attach to any GameObject that should act as a "screen" managed by UIManager.
/// Implement Show/Hide with whatever animation library you like (LeanTween, DOTween, etc.)
/// </summary>
public interface IUIScreen
{
    GameObject gameObject { get; }

    /// <summary>Called when this screen should become visible.</summary>
    /// <param name="instant">Skip animation if true.</param>
    void Show(bool instant = false);

    /// <summary>Called when this screen should become hidden.</summary>
    /// <param name="onComplete">Invoke when the hide animation finishes.</param>
    /// <param name="instant">Skip animation if true.</param>
    void Hide(Action onComplete = null, bool instant = false);

    /// <summary>How long the show transition takes (seconds). Return 0 for instant.</summary>
    float GetTransitionDuration(bool instant = false);

    /// <summary>Enable or disable interaction (e.g. CanvasGroup.interactable).</summary>
    void SetInteractable(bool interactable);
}

/// <summary>
/// Optional. Implement on a video controller so UIManager can intercept GoBack
/// when a video is open.
/// </summary>
public interface IVideoController
{
    bool IsVideoOpen { get; }
    void CloseVideo();
}


// ─────────────────────────────────────────────────────────────────────────────
// UIMANAGER
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Generic screen-stack UI manager. Works with any class that implements IUIScreen.
///
/// HOW TO ADD A SCREEN:
///  1. Create a GameObject under the screensRoot Transform (e.g. a Canvas child).
///  2. Attach either SimpleUIScreen (no animation) or your own MonoBehaviour
///     that implements IUIScreen (with DOTween, LeanTween, etc.).
///  3. That's it — UIManager finds all IUIScreen components under screensRoot on Awake.
///  4. Assign startingScreenObject in the Inspector.
///
/// EXAMPLE — show a screen from anywhere:
///     UIManager.Instance.ShowScreen(myScreenRef);
///
/// EXAMPLE — go back:
///     UIManager.Instance.GoBack();
/// </summary>
public class UIManager : MonoBehaviour
{
    //public static UIManager Instance { get; private set; }

    [Header("Screens")]
    [Tooltip("Root Transform whose children are scanned for IUIScreen components on Awake. " +
             "Typically the parent Canvas or a dedicated 'Screens' GameObject.")]
    [SerializeField] private Transform screensRoot;

    [Tooltip("The screen shown on start. Must be a child of screensRoot.")]
    [SerializeField] private GameObject startingScreenObject;
    [SerializeField] private GameObject buttonObject;
    [Header("Optional Video Controller")]
    [Tooltip("Drag a GameObject with an IVideoController component here (optional).")]
    [SerializeField] private GameObject videoControllerObject;

    // ── Runtime state ──────────────────────────────────────────────────────

    private readonly List<IUIScreen> registeredScreens = new List<IUIScreen>();
    private IUIScreen currentScreen;
    private IVideoController videoController;
    private bool isTransitioning;

    public bool IsTransitioning => isTransitioning;

    /// <summary>Fired whenever GoBack() completes successfully.</summary>
    public static event Action OnGoBack;

    private struct ScreenHistoryState
    {
        public IUIScreen Screen;
        public bool InstantReturn;
    }

    private readonly Stack<ScreenHistoryState> screenHistory = new Stack<ScreenHistoryState>();

    // ── Unity lifecycle ────────────────────────────────────────────────────

    private void Awake()
    {
        /*if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }*/

        ScanScreensRoot();

        // Resolve optional video controller
        if (videoControllerObject != null)
        {
            videoController = videoControllerObject.GetComponent<IVideoController>();
            if (videoController == null)
                Debug.LogWarning("[UIManager] videoControllerObject has no IVideoController component.");
        }
    }

    private void Start()
    {
        if (startingScreenObject == null)
        {
            Debug.LogError("[UIManager] startingScreenObject is not assigned!");
            return;
        }

        IUIScreen startScreen = startingScreenObject.GetComponent<IUIScreen>();
        if (startScreen == null)
        {
            Debug.LogError("[UIManager] startingScreenObject has no IUIScreen component!");
            return;
        }

        // Hide every screen except the starting one
        foreach (IUIScreen screen in registeredScreens)
        {
            if (screen != startScreen)
                screen.Hide(instant: true);
        }

        currentScreen = startScreen;
        currentScreen.Show(instant: true);
    }

    /// <summary>
    /// Scans screensRoot (including all nested children) for IUIScreen components
    /// and populates registeredScreens. Called once in Awake.
    /// </summary>
    private void ScanScreensRoot()
    {
        if (screensRoot == null)
        {
            Debug.LogWarning("[UIManager] screensRoot is not assigned — no screens auto-registered. " +
                             "Assign it in the Inspector or call RegisterScreen() manually.");
            return;
        }

        // GetComponentsInChildren includes inactive GameObjects via the true flag,
        // so screens that start hidden are still discovered.
        IUIScreen[] found = screensRoot.GetComponentsInChildren<IUIScreen>(includeInactive: true);
        registeredScreens.AddRange(found);

        Debug.Log($"[UIManager] Found {registeredScreens.Count} screen(s) under '{screensRoot.name}'.");
    }

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>
    /// Register a screen at runtime (useful for dynamically spawned UI
    /// that wasn't under screensRoot at Awake time).
    /// </summary>
    public void RegisterScreen(IUIScreen screen)
    {
        if (screen == null) return;
        if (!registeredScreens.Contains(screen))
            registeredScreens.Add(screen);
    }

    /// <summary>
    /// Show a target screen, with optional history/transition control.
    /// </summary>
    /// <param name="targetScreen">The IUIScreen to navigate to.</param>
    /// <param name="rememberHistory">Push current screen onto the back-stack.</param>
    /// <param name="hideInstant">Hide current screen without animation.</param>
    /// <param name="showInstant">Show target screen without animation.</param>
    /// <param name="instantReturnOnBack">When going back to this screen, skip animation.</param>
    public void ShowScreen(
        IUIScreen targetScreen,
        bool rememberHistory = true,
        bool hideInstant = false,
        bool showInstant = false,
        bool instantReturnOnBack = false)
    {
        if (targetScreen == null)
        {
            Debug.LogWarning("[UIManager] targetScreen is null.");
            return;
        }

        if (isTransitioning)
        {
            Debug.LogWarning("[UIManager] Transition already in progress.");
            return;
        }

        isTransitioning = true;

        if (currentScreen != null && currentScreen != targetScreen)
        {
            if (rememberHistory)
            {
                screenHistory.Push(new ScreenHistoryState
                {
                    Screen = currentScreen,
                    InstantReturn = instantReturnOnBack
                });
            }

            IUIScreen screenToHide = currentScreen;
            screenToHide.Hide(() =>
            {
                currentScreen = targetScreen;
                targetScreen.Show(showInstant);
                FinishTransitionAfter(targetScreen.GetTransitionDuration(showInstant));
            }, hideInstant);
        }
        else
        {
            currentScreen = targetScreen;
            targetScreen.Show(showInstant);
            FinishTransitionAfter(targetScreen.GetTransitionDuration(showInstant));
        }
    }

    /// <summary>
    /// Convenience overload: pass a GameObject directly instead of IUIScreen.
    /// The GameObject must have an IUIScreen component.
    /// </summary>
    public void ShowScreen(
        GameObject targetObject,
        bool rememberHistory = true,
        bool hideInstant = false,
        bool showInstant = false,
        bool instantReturnOnBack = false)
    {
        if (targetObject == null)
        {
            Debug.LogWarning("[UIManager] targetObject is null.");
            return;
        }

        IUIScreen screen = targetObject.GetComponent<IUIScreen>();
        if (screen == null)
        {
            Debug.LogError($"[UIManager] {targetObject.name} has no IUIScreen component.");
            return;
        }

        ShowScreen(screen, rememberHistory, hideInstant, showInstant, instantReturnOnBack);
    }

    public void GoBack()
    {
        if (isTransitioning)
            return;

        // If a video is currently open, close it instead of navigating back.
        if (videoController != null && videoController.IsVideoOpen)
        {
            videoController.CloseVideo();
            return;
        }

        if (screenHistory.Count == 0)
        {
            Debug.Log("[UIManager] No previous screen in history.");
            return;
        }

        // Allow current screen to react before navigating away.
        if (currentScreen is SimpleUIScreen simpleScreen)
        {
            simpleScreen.OnBackNavigation();
        }

        ScreenHistoryState previousState = screenHistory.Pop();

        if (previousState.Screen == null)
        {
            Debug.LogWarning("[UIManager] Previous screen in history was destroyed!");
            return;
        }

        Debug.Log(
            $"[UIManager] Going back to: {previousState.Screen.gameObject.name}");

        currentScreen?.SetInteractable(false);

        ShowScreen(
            previousState.Screen,
            rememberHistory: false,
            hideInstant: false,
            showInstant: previousState.InstantReturn,
            instantReturnOnBack: false
        );

        OnGoBack?.Invoke();

        if (screenHistory.Count == 0)
        {
            if (buttonObject != null)
                buttonObject.SetActive(false);
        }
    }


    /*/// <summary>
    /// Navigate back to the previous screen. If a video is open, closes it first.
    /// </summary>
    public void GoBack()
    {
        if (isTransitioning) return;

        // Intercept video if present
        if (videoController != null && videoController.IsVideoOpen)
        {
            videoController.CloseVideo();
            return;
        }

        if (screenHistory.Count == 0)
        {
            Debug.Log("[UIManager] No previous screen in history.");
            return;
        }

        ScreenHistoryState previousState = screenHistory.Pop();

        if (previousState.Screen == null)
        {
            Debug.LogWarning("[UIManager] Previous screen in history was destroyed!");
            return;
        }

        Debug.Log($"[UIManager] Going back to: {previousState.Screen.gameObject.name}");

        currentScreen?.SetInteractable(false);

        ShowScreen(
            previousState.Screen,
            rememberHistory: false,
            hideInstant: false,
            showInstant: previousState.InstantReturn,
            instantReturnOnBack: false
        );

        OnGoBack?.Invoke();

        if(screenHistory.Count == 0)
        {
            if(buttonObject!= null) 
                buttonObject.SetActive(false);
        }
    }
*/
    /// <summary>
    /// Clears the entire navigation history.
    /// </summary>
    public void ClearHistory() => screenHistory.Clear();

    /// <summary>
    /// How many screens are on the back-stack.
    /// </summary>
    public int HistoryDepth => screenHistory.Count;

    // ── Private helpers ────────────────────────────────────────────────────

    private void FinishTransitionAfter(float delay)
    {
        if (delay <= 0f)
        {
            isTransitioning = false;
            return;
        }

        // Uses a plain coroutine so there's no LeanTween dependency here.
        // Swap for LeanTween.delayedCall / DOVirtual.DelayedCall if preferred.
        StartCoroutine(UnlockAfterDelay(delay));
    }

    private System.Collections.IEnumerator UnlockAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        isTransitioning = false;
    }
}