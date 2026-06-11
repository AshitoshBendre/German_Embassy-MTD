
using UnityEngine;
using UnityEngine.UI;

public class ImageButtonUI : MonoBehaviour
{
    private Button btn;
    private GallerySectionView sectionView;
    private string imageUrl;
    private string titleText;
    [SerializeField] private Image image;

    private void Awake()
    {
        if(image == null)
            image = GetComponent<Image>();
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }
    public void Initialize(GallerySectionView sectionView, string imageUrl, string titleText, string fullFolderPath)
    {
        this.sectionView = sectionView;
        this.imageUrl = imageUrl;
        this.titleText = titleText;

        if(image!= null)
            Helpers.ImageHelper.LoadAndApplyImageAsync(fullFolderPath, imageUrl, image);

    }

    public void OnClick()
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || sectionView == null) return;

        sectionView.ShowPhotoOnMainImageRect(imageUrl, titleText);
    }
}