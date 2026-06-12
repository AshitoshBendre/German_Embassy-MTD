using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VideoUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VideoManager videoManager;
    [SerializeField] private Button playButton;

    [Header("Hide After Delay")]
    [Tooltip("Objects that will fade out and hide shortly after the video starts.")]
    [SerializeField] private List<GameObject> hideOnPlay = new();

    [Header("On Video Finished - Hide")]
    [Tooltip("Objects that will be hidden when the video finishes.")]
    [SerializeField] private List<GameObject> onVideoFinishHide = new();

    [Header("On Video Finished - Show")]
    [Tooltip("Objects that will be shown when the video finishes.")]
    [SerializeField] private List<GameObject> onVideoFinishShow = new();

    [Header("Video")]
    [Tooltip("Relative path inside StreamingAssets. Example: Common Assets/Button Animation.webm")]
    [SerializeField] private string videoPath;

    [SerializeField] private bool autoExit = true;

    [Header("Delay")]
    [Tooltip("How long to wait after playback starts before hiding the UI.")]
    [SerializeField] private float hideDelay = 0.3f;

    [Header("Fade")]
    [Tooltip("Duration of fade-out animation before objects are hidden.")]
    [SerializeField] private float fadeOutDuration = 0.3f;

    [Header("Unity Events")]
    [SerializeField] private UnityEvent onVideoStartedEvent;
    [SerializeField] private UnityEvent onVideoFinishedEvent;

    public event Action OnVideoStarted;
    public event Action OnVideoFinished;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (playButton != null)
        {
            //playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(PlayVideo);
        }

        ValidateVideoPath();
    }

    private void OnDestroy()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(PlayVideo);
        }
    }

    private void ValidateVideoPath()
    {
        if (string.IsNullOrWhiteSpace(videoPath))
        {
            Debug.LogWarning(
                $"[VideoUIController] Video path is empty on '{gameObject.name}'.");
            return;
        }

        string fullPath = Path.Combine(
            Application.streamingAssetsPath,
            videoPath);

        if (!File.Exists(fullPath))
        {
            Debug.LogWarning(
                $"[VideoUIController] Video file not found.\n" +
                $"Relative Path: {videoPath}\n" +
                $"Expected Path: {fullPath}");
        }
    }

    public void PlayVideo()
    {
        if (videoManager == null)
        {
            Debug.LogWarning(
                $"[VideoUIController] VideoManager is missing on '{gameObject.name}'.");
            return;
        }

        onVideoStartedEvent?.Invoke();
        OnVideoStarted?.Invoke();

        Debug.Log($"[VideoUIController] Playing video: {videoPath}");

        videoManager.PlayVideo(
            videoPath,
            autoExit,
            HandleVideoFinished);

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

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
            if (obj == null)
                continue;

            CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                Debug.LogWarning(
                    $"[VideoUIController] '{obj.name}' is missing a CanvasGroup.");
                continue;
            }

            canvasGroups.Add(canvasGroup);
        }

        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;

            float alpha = Mathf.Lerp(
                1f,
                0f,
                elapsed / fadeOutDuration);

            foreach (var canvasGroup in canvasGroups)
            {
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = alpha;
                }
            }

            yield return null;
        }

        foreach (var canvasGroup in canvasGroups)
        {
            if (canvasGroup != null)
            {
                canvasGroup.gameObject.SetActive(false);
                canvasGroup.alpha = 1f;
            }
        }
    }

    private void HandleVideoFinished()
    {
        Debug.Log("[VideoUIController] Video Finished.");

        SetObjectsActive(onVideoFinishHide, false);
        SetObjectsActive(onVideoFinishShow, true);

        onVideoFinishedEvent?.Invoke();
        OnVideoFinished?.Invoke();
    }

    private static void SetObjectsActive(
        List<GameObject> objects,
        bool state)
    {
        if (objects == null)
            return;

        foreach (var obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(state);
            }
        }
    }
}