
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(VideoManager))]
public class VideoSectionView : MonoBehaviour, IProjectSectionView
{
    [Header("Top Section")]
    [SerializeField] private TMP_Text videoCaptions;

    [Header("Bottom Sections")]
    [SerializeField] private Transform videoContainer;
    [SerializeField] private VideoButtonUI videoButtonPrefab;
    [SerializeField] private List<GameObject> objectsToShow;
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private GameObject TabButton;
    [SerializeField] private float defaultIdleTimeOut = 8f;
    private VideoManager videoManager;
    private ProjectContext projectContext;
    private string FullFolderPath;
    private Coroutine idleTimeoutCoroutine;
    private void Awake()
    {
        videoManager = GetComponent<VideoManager>();
    }

    private void HandleOnVideoEnd()
    {
        if (projectContext == null)
            return;

        if (idleTimeoutCoroutine != null)
        {
            GlobalTimer.Stop(idleTimeoutCoroutine);
        }

        idleTimeoutCoroutine = GlobalTimer.Start(
            (projectContext.Data.videosTabData.idleTimeout != 0f) ? projectContext.Data.videosTabData.idleTimeout : defaultIdleTimeOut,
            () =>
            {
                popupPanel.SetActive(false);
                videoManager.CloseVideo();
                idleTimeoutCoroutine = null;
            });
    }

    public void Initialize(ProjectContext context)
    {
        projectContext = context;
        FullFolderPath = $"{context.PanelFolderId}/{context.ProjectFolderId}";
        VideoListBuilder(context.Data.videosTabData.videoDatas);
    }

    private void VideoListBuilder(List<VideoData> data)
    {
        if (data == null)
        {
            Debug.LogWarning("[Video Builder] Data list is NULL.");
            return;
        }

        if (data.Count == 0)
        {
            Debug.LogWarning("[Video Builder] Data list is empty.");
            return;
        }

        foreach (Transform child in videoContainer)
        {
            Destroy(child.gameObject);
        }

        int createdCount = 0;
        int skippedCount = 0;

        foreach (var videoData in data)
        {
            if (!IsVideoDataValid(videoData))
            {
                skippedCount++;
                continue;
            }

            var videoObj = Instantiate(videoButtonPrefab, videoContainer);
            var videoBtnUI = videoObj.GetComponent<VideoButtonUI>();

            string videoPath = Path.Combine(FullFolderPath, videoData.videoURL);

            videoBtnUI.Initialize(
                this,
                videoPath,
                videoData.titleText);

            createdCount++;
        }

        Debug.Log(
            $"[Video Builder] Created {createdCount} video items. Skipped {skippedCount} invalid entries.");

    }

    public void ShowVideoOnMainRectRawImage(string videoURL, string titleText)
    {
        popupPanel.SetActive(true);
        videoCaptions.text = titleText;
        videoManager.PlayVideo(videoURL, true, HandleOnVideoEnd);
    }

    public void ShowUI()
    {
        foreach (GameObject showpanel in objectsToShow)
        {
            showpanel.SetActive(true);
        }
    }

    public void OnUIExit()
    {

        if (idleTimeoutCoroutine != null)
        {
            GlobalTimer.Stop(idleTimeoutCoroutine);
            idleTimeoutCoroutine = null;
        }

        popupPanel.SetActive(false);
        videoManager.CloseVideo();
    }

    public void ValidateData(ProjectContext projectContext)
    {
        this.projectContext = projectContext;

        bool shouldShowTab = false;

        VideosTabData videosTabData = projectContext.Data.videosTabData;

        if (videosTabData == null)
        {
            Debug.LogWarning("[Video Validation] VideosTabData is NULL.");
            TabButton.SetActive(false);
            return;
        }

        if (videosTabData.videoDatas == null)
        {
            Debug.LogWarning("[Video Validation] videoDatas list is NULL.");
            TabButton.SetActive(false);
            return;
        }

        if (videosTabData.videoDatas.Count == 0)
        {
            Debug.LogWarning("[Video Validation] videoDatas list is empty.");
            TabButton.SetActive(false);
            return;
        }

        foreach (var videoData in videosTabData.videoDatas)
        {
            if (IsVideoDataValid(videoData))
            {
                shouldShowTab = true;
                break;
            }
        }

        if (!shouldShowTab)
        {
            Debug.LogWarning(
                $"[Video Validation] No valid video entries found for project '{projectContext.ProjectFolderId}'.");
        }

        TabButton.SetActive(shouldShowTab);
    }

    private bool IsVideoDataValid(VideoData videoData)
    {
        if (videoData == null)
        {
            Debug.LogWarning("[Video Validation] GalleryData entry is NULL.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(videoData.videoURL))
        {
            Debug.LogWarning(
                $"[Video Validation] Invalid videoURL for entry with title '{videoData.titleText}'.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(videoData.titleText))
        {
            Debug.LogWarning(
                $"[Video Validation] Invalid titleText for video '{videoData.videoURL}'.");
            return false;
        }

        string fullFolderPath =
            $"{projectContext.PanelFolderId}/{projectContext.ProjectFolderId}";

        string videoPath = Path.Combine(
            Application.streamingAssetsPath,
            fullFolderPath,
            videoData.videoURL);

        if (!File.Exists(videoPath))
        {
            Debug.LogWarning(
                $"[Video Validation] video file not found.\n" +
                $"Title: {videoData.titleText}\n" +
                $"Video URL: {videoData.videoURL}\n" +
                $"Expected Path: {videoPath}");
            return false;
        }

        return true;
    }
}
