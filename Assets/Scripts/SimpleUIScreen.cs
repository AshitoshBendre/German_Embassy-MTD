using System;
using UnityEngine;
// ─────────────────────────────────────────────────────────────────────────────
// CONCRETE FALLBACK — a no-frills IUIScreen that just toggles SetActive
// Use this when you don't need fancy transitions at all.
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Drop this component on any GameObject to make it a "dumb" screen:
/// no animation, just SetActive(true/false).
/// </summary>
public class SimpleUIScreen : MonoBehaviour, IUIScreen
{
    public void Show(bool instant = false) => gameObject.SetActive(true);

    public void Hide(Action onComplete = null, bool instant = false)
    {
        gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    public float GetTransitionDuration(bool instant = false) => 0f;

    public void SetInteractable(bool interactable) { /* no-op for simple screens */ }
}
