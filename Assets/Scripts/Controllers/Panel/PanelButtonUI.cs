using Helpers;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelButtonUI : MonoBehaviour
{
    [SerializeField] private Button _projectsButton;
    [SerializeField] private Button _mapButton;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private Image _panelCoverImage;
    [SerializeField] private HorizontalListBuilder _horizontalListBuilder;
    [SerializeField] private UIManager _manager;
    [SerializeField] private VideoManager _mapVideoManager;
    [SerializeField] private GameObject _mapContainer;
    private string _myFolderId;
    private PanelContext _panelContext;

    public void Initialize(PanelContext context)
    {
        _panelContext = context;
        _myFolderId = context.FolderId;

        _titleText.text = context.Data.titleText;

        //_projectsButton.onClick.AddListener(() => _horizontalListBuilder?.BuildProjectList(_myFolderId, _manager));

        ValidateMapButton();
        ImageHelper.LoadAndApplyImageAsync(context.FolderId, context.Data.imageURL, _panelCoverImage);
    }

    public void ClickProjectsButton()
    {
        //_projectsButton.onClick.AddListener(() => _horizontalListBuilder?.BuildProjectList(_myFolderId, _manager));
        _horizontalListBuilder?.BuildProjectList(_myFolderId, _manager);
    }

    private void ValidateMapButton()
    {
        
        if (string.IsNullOrWhiteSpace(_panelContext.Data.mapURL))
        {
            Debug.LogWarning(
                $"[PanelButtonUI] Map URL is empty for panel '{_panelContext.FolderId}'.");

            _mapButton.gameObject.SetActive(false);
            return;
        }

        string videoPath = Path.Combine(
            Application.streamingAssetsPath,
            _myFolderId,
            _panelContext.Data.mapURL);

        if (!File.Exists(videoPath))
        {
            Debug.LogWarning(
                $"[PanelButtonUI] Map video not found.\n" +
                $"Panel: {_panelContext.FolderId}\n" +
                $"Map URL: {_panelContext.Data.mapURL}\n" +
                $"Expected Path: {videoPath}");

            _mapButton.gameObject.SetActive(false);
            return;
        }

        _mapButton.gameObject.SetActive(true);

        _mapButton.onClick.RemoveAllListeners();
        _mapButton.onClick.AddListener(PlayMapVideo);
    }

    private void PlayMapVideo()
    {
        if (_mapVideoManager == null)
        {
            Debug.LogWarning("[PanelButtonUI] MapVideoManager is null.");
            return;
        }

        _mapContainer.SetActive(true);

        string videoPath = Path.Combine(
            _myFolderId,
            _panelContext.Data.mapURL);

        _mapVideoManager.PlayVideo(
            videoPath,
            true,
            OnMapVideoFinished);
    }
    private void OnMapVideoFinished()
    {
        _mapContainer.SetActive(false);
    }
}