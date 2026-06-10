
using UnityEngine;
using UnityEngine.UI;

public class VideoButtonUI : MonoBehaviour
{
    private Button btn;
    private VideoSectionView sectionView;
    private string imageUrl;
    private string titleText;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }
    public void Initialize(VideoSectionView sectionView, string imageUrl, string titleText)
    {
        this.sectionView = sectionView;
        this.imageUrl = imageUrl;
        this.titleText = titleText;
    }

    public void OnClick()
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || sectionView == null) return;

        //sectionView.ShowPhotoOnMainImageRect(imageUrl, titleText);
    }
}