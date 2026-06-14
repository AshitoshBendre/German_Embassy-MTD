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

public class UIManager : MonoBehaviour
{
    [Header("Title Screen Integration")]
    [Tooltip("Which panel does this specific UIManager control?")]
    [SerializeField] private TitleScreenPanel titleScreenPanel;

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

    private void ScanScreensRoot()
    {
        if (screensRoot == null)
        {
            Debug.LogWarning("[UIManager] screensRoot is not assigned — no screens auto-registered. " +
                             "Assign it in the Inspector or call RegisterScreen() manually.");
            return;
        }

        IUIScreen[] found = screensRoot.GetComponentsInChildren<IUIScreen>(includeInactive: true);
        registeredScreens.AddRange(found);

        Debug.Log($"[UIManager] Found {registeredScreens.Count} screen(s) under '{screensRoot.name}'.");
    }

    // ── Public API ─────────────────────────────────────────────────────────

    public void RegisterScreen(IUIScreen screen)
    {
        if (screen == null) return;
        if (!registeredScreens.Contains(screen))
            registeredScreens.Add(screen);
    }

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
            // We are leaving the homepage for the first time
            if (screenHistory.Count == 0)
            {
                if (TitleScreenController.Instance != null)
                {
                    TitleScreenController.Instance.SetPanelState(titleScreenPanel, true);
                }
            }

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
            Debug.Log("[UIManager] Already at homepage.");

            // Failsafe: Ensure the title screen knows we are at the homepage
            if (TitleScreenController.Instance != null)
            {
                TitleScreenController.Instance.SetPanelState(titleScreenPanel, false);
            }
            return;
        }



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

        // If popping the history brought us back to the root/homepage
        if (screenHistory.Count == 0)
        {
            if (buttonObject != null)
                buttonObject.SetActive(false);

            if (TitleScreenController.Instance != null)
            {
                TitleScreenController.Instance.SetPanelState(titleScreenPanel, false);
            }
        }
    }

    public void ClearHistory() => screenHistory.Clear();

    public int HistoryDepth => screenHistory.Count;

    // ── Private helpers ────────────────────────────────────────────────────

    private void FinishTransitionAfter(float delay)
    {
        if (delay <= 0f)
        {
            isTransitioning = false;
            return;
        }

        StartCoroutine(UnlockAfterDelay(delay));
    }

    private System.Collections.IEnumerator UnlockAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        isTransitioning = false;
    }

    /// <summary>
    /// Jumps directly back to the starting screen, bypassing the back-stack,
    /// clearing history, and resetting the title screen state.
    /// </summary>
    /// <param name="instant">If true, skips the screen transition animations.</param>
    public void GoToHomeScreen(bool instant = false)
    {
        if (isTransitioning)
            return;

        IUIScreen homeScreen = startingScreenObject.GetComponent<IUIScreen>();
        if (homeScreen == null)
        {
            Debug.LogWarning("[UIManager] Cannot go to home screen: startingScreenObject is missing or lacks IUIScreen.");
            return;
        }

        // If we are already on the home screen and history is clear, do nothing
        if (currentScreen == homeScreen && screenHistory.Count == 0)
            return;

        // If a video is currently open, ensure it gets closed
        if (videoController != null && videoController.IsVideoOpen)
        {
            videoController.CloseVideo();
        }

        // Allow the current screen to process its back-navigation cleanup (like hiding objects)
        if (currentScreen is SimpleUIScreen simpleScreen)
        {
            simpleScreen.OnBackNavigation();
        }

        // 1. Wipe the history stack clean
        screenHistory.Clear();
        currentScreen?.SetInteractable(false);

        // 2. Transition back to the starting screen (do not add to history)
        Debug.Log("[UIManager] Jumping directly to Home Screen.");
        ShowScreen(
            homeScreen,
            rememberHistory: false,
            hideInstant: instant,
            showInstant: instant,
            instantReturnOnBack: false
        );

        // 3. Reset UI States (Back button and Title Screen)
        if (buttonObject != null)
            buttonObject.SetActive(false);

        if (TitleScreenController.Instance != null)
        {
            TitleScreenController.Instance.SetPanelState(titleScreenPanel, false);
        }
    }
}