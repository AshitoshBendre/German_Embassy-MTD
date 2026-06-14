using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class VideoUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VideoManager videoManager;
    // Removed the Button playButton. This script shouldn't manage UI clicks directly.

    [Header("Hide After Delay")]
    [Tooltip("Objects that fade out shortly after the video starts.")]
    [SerializeField] private List<GameObject> hideOnPlay = new();

    [Header("On Video Finished - Hide")]
    [Tooltip("Objects hidden when the video finishes.")]
    [SerializeField] private List<GameObject> onVideoFinishHide = new();

    [Header("On Video Finished - Show")]
    [Tooltip("Objects shown when the video finishes.")]
    [SerializeField] private List<GameObject> onVideoFinishShow = new();

    [Header("Video Settings")]
    [SerializeField] private string videoPath;
    [SerializeField] private bool autoExit = true;
    [SerializeField] private float hideDelay = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    // We keep the UnityEvents just in case you want to trigger particle systems or sounds in the Inspector
    [Header("Optional Unity Events")]
    [SerializeField] private UnityEvent onVideoStartedEvent;
    [SerializeField] private UnityEvent onVideoFinishedEvent;

    // Pure C# events for rock-solid code architecture
    public event Action OnVideoStarted;
    public event Action OnVideoFinished;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        ValidateVideoPath();
    }

    private void ValidateVideoPath()
    {
        if (string.IsNullOrWhiteSpace(videoPath)) return;

        string fullPath = Path.Combine(Application.streamingAssetsPath, videoPath);
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"[VideoUIController] Video file not found at: {fullPath}");
        }
    }

    // Call this from your PanelButtonUI orchestrator
    public void PlayVideo()
    {
        if (videoManager == null)
        {
            Debug.LogError($"[VideoUIController] VideoManager is missing on '{gameObject.name}'.");
            return;
        }

        onVideoStartedEvent?.Invoke();
        OnVideoStarted?.Invoke();

        Debug.Log($"[VideoUIController] Playing intro video: {videoPath}");

        videoManager.PlayVideo(videoPath, autoExit, HandleVideoFinished);

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideUIAfterDelay());
    }

    private IEnumerator HideUIAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        yield return StartCoroutine(FadeOutObjects(hideOnPlay));
    }

    private IEnumerator FadeOutObjects(List<GameObject> objects)
    {
        List<CanvasGroup> canvasGroups = new();

        foreach (var obj in objects)
        {
            if (obj == null) continue;
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            if (cg != null) canvasGroups.Add(cg);
        }

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);

            foreach (var cg in canvasGroups) cg.alpha = alpha;
            yield return null;
        }

        foreach (var cg in canvasGroups)
        {
            cg.gameObject.SetActive(false);
            cg.alpha = 1f;
        }
    }

    private void HandleVideoFinished()
    {
        Debug.Log("[VideoUIController] Intro Video Finished.");

        SetObjectsActive(onVideoFinishHide, false);
        SetObjectsActive(onVideoFinishShow, true);

        onVideoFinishedEvent?.Invoke();
        OnVideoFinished?.Invoke();
    }

    private static void SetObjectsActive(List<GameObject> objects, bool state)
    {
        if (objects == null) return;
        foreach (var obj in objects)
        {
            if (obj != null) obj.SetActive(state);
        }
    }
}