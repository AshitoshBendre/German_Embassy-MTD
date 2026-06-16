using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(ScreenSaverMonitor))]
public class ScreenSaveController : MonoBehaviour
{
    private ScreenSaverMonitor monitor;
    private CanvasGroup canvasGroup;
    [Header("Fade Settings")]
    public GameObject panel;
    public float fadeDuration = 2f;

    private void Start()
    {
        /*monitor = GetComponent<ScreenSaverMonitor>();
        if(monitor == null)
        {
            monitor = gameObject.AddComponent<ScreenSaverMonitor>();
            monitor.idleTimeout = 20f;
        }

        monitor.OnScreensaverActivated += ShowScreenSaver;
        monitor.OnScreensaverDeactivated += HideScreenSaver;


        canvasGroup = panel.GetComponent<CanvasGroup>();*/
    }

    private void ShowScreenSaver()
    {
        canvasGroup.alpha = 0f;
        panel.gameObject.SetActive(true);
        StartCoroutine(FadeCanvas(fadeDuration, 1f, () =>
        {
        }));
    }

    private void HideScreenSaver()
    {
        StartCoroutine(FadeCanvas(fadeDuration, 0f, () =>
        {
            panel.gameObject.SetActive(false);
        }));
    }

    private IEnumerator FadeCanvas(float duration, float target, Action onComplete)
    {
        float StartAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha =  Mathf.Lerp(StartAlpha,target,t);
            yield return null;
        }
        canvasGroup.alpha = target;
        onComplete?.Invoke();
    }

}
