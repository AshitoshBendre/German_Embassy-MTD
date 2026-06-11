
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VideoButtonUI : MonoBehaviour
{
    private Button btn;
    private VideoSectionView sectionView;
    private string videoURL;
    private string titleText;
    [SerializeField] private TMP_Text title;
    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }
    public void Initialize(VideoSectionView sectionView, string videoURL, string titleText)
    {
        this.sectionView = sectionView;
        this.videoURL = videoURL;
        this.titleText = titleText;
        title.text = titleText;
    }

    public void OnClick()
    {
        if (string.IsNullOrWhiteSpace(videoURL) || sectionView == null) return;

        sectionView.ShowVideoOnMainRectRawImage(videoURL, titleText);
    }
}