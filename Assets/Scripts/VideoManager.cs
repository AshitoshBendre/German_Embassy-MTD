using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System;
[RequireComponent(typeof(VideoPlayer))]
public class VideoManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public RawImage rawImage;
    public CanvasGroup canvasGroup;

    [Header("RenderTexture Settings")]
    public int width = 5120;
    public int height = 2160;

    [Header("Fade")]
    public float fadeDuration = 0.3f;

    [Header("Placeholder")]
    public Sprite placeholderSprite;

    private VideoPlayer videoPlayer;
    private RenderTexture runtimeTexture;
    private bool exitOnComplete;
    private bool isFading;

    public bool IsPlaying { get; private set; }
    public bool IsVideoOpen => panel.activeSelf;
    private event Action action;
    // ------------------------------------------------
    // LIFECYCLE
    // ------------------------------------------------

    void Awake()
    {
        panel.SetActive(false);
        canvasGroup.alpha = 0f;
        videoPlayer = GetComponent<VideoPlayer>();

        CreateRenderTexture();
        AssignTextures();
        // Set once here — never need to repeat in PlayVideo
        videoPlayer.skipOnDrop = true;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.playOnAwake = false;
        videoPlayer.source = VideoSource.Url;

        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.loopPointReached += OnVideoCompleted;
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnPrepared;
            videoPlayer.loopPointReached -= OnVideoCompleted;
        }

        ReleaseRenderTexture();
    }

    // ------------------------------------------------
    // PUBLIC API
    // ------------------------------------------------

    public void PlayVideo(string clipPath, bool autoExit, Action onVideoEnd)
    {
        if (string.IsNullOrEmpty(clipPath))
        {
            Debug.LogWarning("[VideoManager] PlayVideo called with empty path.");
            return;
        }
        action = onVideoEnd;
        // If something is already running, stop it cleanly first
        StopAllCoroutines();
        videoPlayer.Stop();
        isFading = false;

        exitOnComplete = autoExit;
        IsPlaying = true;

        videoPlayer.url = BuildVideoUrl(clipPath);

        AssignTextures();
        canvasGroup.alpha = 1f;
        panel.SetActive(true);

        videoPlayer.Prepare();
    }

    public void CloseVideo()
    {
        if (!IsVideoOpen || isFading)
            return;

        IsPlaying = false;
        videoPlayer.Stop();

        StopAllCoroutines();
        StartCoroutine(FadeOutAndCleanup());
    }

    // ------------------------------------------------
    // VIDEO PLAYER CALLBACKS
    // ------------------------------------------------

    void OnPrepared(VideoPlayer vp)
    {
        vp.Play();
    }

    void OnVideoCompleted(VideoPlayer vp)
    {
        IsPlaying = false;

        if (exitOnComplete && IsVideoOpen && !isFading)
        {
            StopAllCoroutines();
            action?.Invoke();
            ClearRenderTexture();
            //StartCoroutine(FadeOutAndCleanup());
        }
    }

    // ------------------------------------------------
    // FADE + CLEANUP
    // ------------------------------------------------

    IEnumerator FadeOutAndCleanup()
    {
        isFading = true;

        float startAlpha = canvasGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        panel.SetActive(false);

        ClearRenderTexture();

        // Ready for next playback
        canvasGroup.alpha = 1f;
        isFading = false;
    }

    // ------------------------------------------------
    // RENDER TEXTURE
    // ------------------------------------------------

    void CreateRenderTexture()
    {
        ReleaseRenderTexture();
        runtimeTexture = new RenderTexture(width, height, 0);
        runtimeTexture.Create();
    }

    void AssignTextures()
    {
        videoPlayer.targetTexture = runtimeTexture;
        rawImage.texture = runtimeTexture;
    }

    void ClearRenderTexture()
    {
        if (placeholderSprite != null && placeholderSprite.texture != null)
        {
            Graphics.Blit(placeholderSprite.texture, runtimeTexture);
        }
        else
        {
            // Fallback: clear to black
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = runtimeTexture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = prev;
        }
    }

    void ReleaseRenderTexture()
    {
        if (videoPlayer != null)
            videoPlayer.targetTexture = null;

        if (runtimeTexture != null)
        {
            runtimeTexture.Release();
            Destroy(runtimeTexture);
            runtimeTexture = null;
        }
    }

    // ------------------------------------------------
    // HELPERS
    // ------------------------------------------------

    static string BuildVideoUrl(string clipPath)
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, clipPath);

#if UNITY_ANDROID && !UNITY_EDITOR
        return fullPath;
#else
        return "file://" + fullPath;
#endif
    }
}