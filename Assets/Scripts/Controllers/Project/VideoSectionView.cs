
using System.Collections.Generic;
using TMPro;
using UnityEngine;
[RequireComponent(typeof(VideoManager))]
public class VideoSectionView : MonoBehaviour, IProjectSectionView
{
    [Header("Top Section")]
    [SerializeField] private TMP_Text videoCaptions;

    [Header("Bottom Sections")]
    [SerializeField] private Transform videoContainer;
    //[SerializeField] private VideoButtonUI videoButtonPrefab;

    private ProjectContext projectContext;
    private string FullFolderPath;
    public void Initialize(ProjectContext context)
    {
        projectContext = context;
        FullFolderPath = $"{context.PanelFolderId}/{context.ProjectFolderId}";
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

        }
    }

    public void ShowUI()
    {
        throw new System.NotImplementedException();
    }
}
