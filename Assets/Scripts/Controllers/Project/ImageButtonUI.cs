
using UnityEngine;
using UnityEngine.UI;

public class ImageButtonUI : MonoBehaviour
{
    private Button btn;
    private GallerySectionView sectionView;
    private string imageUrl;
    private string titleText;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }
    public void Initialize(GallerySectionView sectionView, string imageUrl, string titleText)
    {
        this.sectionView = sectionView;
        this.imageUrl = imageUrl;
        this.titleText = titleText;
    }

    public void OnClick()
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || sectionView == null) return;

        sectionView.ShowPhotoOnMainImageRect(imageUrl, titleText);
    }
}