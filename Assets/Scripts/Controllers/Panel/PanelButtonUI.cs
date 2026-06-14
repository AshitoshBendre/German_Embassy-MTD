using Helpers;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelButtonUI : MonoBehaviour
{
    [Header("Core UI")]
    [SerializeField] private Button _projectsButton; // The "Explore" button
    [SerializeField] private Button _mapButton;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private Image _panelCoverImage;

    [Header("Managers")]
    [SerializeField] private HorizontalListBuilder _horizontalListBuilder;
    [SerializeField] private UIManager _manager;

    [Header("Video Controllers")]
    [SerializeField] private VideoManager _mapVideoManager;
    [SerializeField] private GameObject _mapContainer;

    [Tooltip("The controller handling the cinematic intro video before showing projects.")]
    [SerializeField] private VideoUIController _introVideoController;

    private string _myFolderId;
    private PanelContext _panelContext;

    public void Initialize(PanelContext context)
    {
        _panelContext = context;
        _myFolderId = context.FolderId;
        _titleText.text = context.Data.titleText;

        // 1. Wire up the Projects/Explore button entirely in code
        _projectsButton.onClick.RemoveAllListeners();
        _projectsButton.onClick.AddListener(OnProjectsButtonClicked);

        ValidateMapButton();
        ImageHelper.LoadAndApplyImageAsync(context.FolderId, context.Data.imageURL, _panelCoverImage);
    }

    // ─── EXPLORE BUTTON FLOW ──────────────────────────────────────────────────

    private void OnProjectsButtonClicked()
    {
        // If we have a cinematic intro video, play it and wait.
        if (_introVideoController != null)
        {
            // Unsubscribe first to prevent double-firing if clicked multiple times
            _introVideoController.OnVideoFinished -= BuildProjectList;
            _introVideoController.OnVideoFinished += BuildProjectList;

            _introVideoController.PlayVideo();
        }
        else
        {
            // Fallback: If there is no intro video assigned, just build the list instantly
            BuildProjectList();
        }
    }

    private void BuildProjectList()
    {
        // Clean up the listener so it doesn't fire again unexpectedly
        if (_introVideoController != null)
        {
            _introVideoController.OnVideoFinished -= BuildProjectList;
        }

        Debug.Log($"[PanelButtonUI] Building Project List for Folder ID: {_myFolderId}");
        _horizontalListBuilder?.BuildProjectList(_myFolderId, _manager);
    }

    // ─── MAP BUTTON FLOW ──────────────────────────────────────────────────────

    private void ValidateMapButton()
    {
        if (string.IsNullOrWhiteSpace(_panelContext.Data.mapURL))
        {
            _mapButton.gameObject.SetActive(false);
            return;
        }

        string videoPath = Path.Combine(Application.streamingAssetsPath, _myFolderId, _panelContext.Data.mapURL);
        if (!File.Exists(videoPath))
        {
            _mapButton.gameObject.SetActive(false);
            return;
        }

        _mapButton.gameObject.SetActive(true);
        _mapButton.onClick.RemoveAllListeners();
        _mapButton.onClick.AddListener(PlayMapVideo);
    }

    private void PlayMapVideo()
    {
        if (_mapVideoManager == null) return;

        _mapContainer.SetActive(true);
        string videoPath = Path.Combine(_myFolderId, _panelContext.Data.mapURL);
        _mapVideoManager.PlayVideo(videoPath, true, OnMapVideoFinished);
    }

    private void OnMapVideoFinished()
    {
        _mapContainer.SetActive(false);
    }
}