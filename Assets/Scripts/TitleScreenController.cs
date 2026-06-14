using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenController : MonoBehaviour
{
    public static TitleScreenController Instance { get; private set; }

    [Header("Panel States")]
    [SerializeField] private bool panelAState;
    [SerializeField] private bool panelBState;
    [SerializeField] private bool panelCState;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private List<CanvasGroup> targetObjects = new();

    private bool hasHidden;
    private Coroutine fadeRoutine;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    

    private void Start()
    {
        UpdateVisibility();
    }

    public bool PanelAState
    {
        get => panelAState;
        set
        {
            if (panelAState == value) return;

            panelAState = value;
            UpdateVisibility();
        }
    }

    public bool PanelBState
    {
        get => panelBState;
        set
        {
            if (panelBState == value) return;

            panelBState = value;
            UpdateVisibility();
        }
    }

    public bool PanelCState
    {
        get => panelCState;
        set
        {
            if (panelCState == value) return;

            panelCState = value;
            UpdateVisibility();
        }
    }

    /// <summary>
    /// Hide targets if ANY panel state is true.
    /// Show targets only when ALL panel states are false.
    /// </summary>
    private void UpdateVisibility()
    {
        bool shouldHide = panelAState || panelBState || panelCState;

        // Skip if already in desired state
        if (shouldHide == hasHidden)
            return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeCanvasGroups(shouldHide));
    }

    private IEnumerator FadeCanvasGroups(bool hide)
    {
        hasHidden = hide;

        if (!hide)
        {
            SetInteraction(true);
        }

        float startAlpha = targetObjects.Count > 0 && targetObjects[0] != null
            ? targetObjects[0].alpha
            : (hide ? 1f : 0f);

        float targetAlpha = hide ? 0f : 1f;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            foreach (var canvasGroup in targetObjects)
            {
                if (canvasGroup == null) continue;
                canvasGroup.alpha = alpha;
            }

            yield return null;
        }

        foreach (var canvasGroup in targetObjects)
        {
            if (canvasGroup == null) continue;
            canvasGroup.alpha = targetAlpha;
        }

        if (hide)
        {
            SetInteraction(false);
        }

        fadeRoutine = null;
    }

    private void SetInteraction(bool enabled)
    {
        foreach (var canvasGroup in targetObjects)
        {
            if (canvasGroup == null) continue;

            canvasGroup.interactable = enabled;
            canvasGroup.blocksRaycasts = enabled;
        }
    }

    #region Button Methods
    public void SetPanelState(TitleScreenPanel panel, bool value)
    {
        switch (panel)
        {
            case TitleScreenPanel.PanelA:
                PanelAState = value;
                break;

            case TitleScreenPanel.PanelB:
                PanelBState = value;
                break;

            case TitleScreenPanel.PanelC:
                PanelCState = value;
                break;
        }
    }

    public void SetPanelA(bool value)
    {
        PanelAState = value;
    }

    public void SetPanelB(bool value)
    {
        PanelBState = value;
    }

    public void SetPanelC(bool value)
    {
        PanelCState = value;
    }

    #endregion

#if UNITY_EDITOR
    // Lets you test by changing values in the inspector during play    mode.
    private void OnValidate()
    {
        if (!Application.isPlaying)
            return;

        UpdateVisibility();
    }
#endif
}
[System.Serializable]
public enum TitleScreenPanel
{
    PanelA,
    PanelB,
    PanelC
}