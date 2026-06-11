
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

    private VideoManager videoManager;
    private ProjectContext projectContext;
    private string FullFolderPath;

    private void Awake()
    {
        videoManager = GetComponent<VideoManager>();
    }
    public void Initialize(ProjectContext context)
    {
        projectContext = context;
        FullFolderPath = $"{context.PanelFolderId}/{context.ProjectFolderId}";
        VideoListBuilder(context.Data.videosTabData.videoDatas);
    }

    private void VideoListBuilder(List<VideoData> data)
    {
        if (data.Count == 0) return;

        foreach(Transform child in videoContainer)
        {
            Destroy(child.gameObject);
        }

        for(int i=0; i<data.Count; i++)
        {
            var videoObj = Instantiate(videoButtonPrefab, videoContainer);
            var videoBtnUI = videoObj.GetComponent<VideoButtonUI>();
            string videoPath = Path.Combine(FullFolderPath, data[i].videoURL);
            videoBtnUI.Initialize(this, videoPath, data[i].titleText);
        }
    }

    public void ShowVideoOnMainRectRawImage(string videoURL, string titleText)
    {
        videoCaptions.text = titleText;
        videoManager.PlayVideo(videoURL, true);
    }

    public void ShowUI()
    {
        foreach (GameObject showpanel in objectsToShow)
        {
            showpanel.SetActive(true);
        }
    }

    public void OnUISwitch()
    {
        throw new System.NotImplementedException();
    }
}
