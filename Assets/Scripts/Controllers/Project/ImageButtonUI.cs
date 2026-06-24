
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ImageButtonUI : MonoBehaviour
{
    private Button btn;
    private GallerySectionView sectionView;
    private string imageUrl;
   /* private string titleText;*/
    private string fullFolderPath;
    [SerializeField] private Image image;
    [SerializeField] private AspectRatioFitter aspectRationFitter;
    private void Awake()
    {
        if(image == null)
            image = GetComponent<Image>();

        if (image != null && image.sprite == null)
        {
            image.sprite = Sprite.Create(Texture2D.blackTexture, new Rect(0, 0, 1, 1), Vector2.zero);
            image.color = new Color(1, 1, 1, 0); // Completely transparent
        }
    }
    public void Initialize(GallerySectionView sectionView, string imageUrl, /*string titleText,*/ string fullFolderPath)
    {
        this.sectionView = sectionView;
        this.imageUrl = imageUrl;
        /*this.titleText = titleText;*/
        this.fullFolderPath = fullFolderPath;
        /*if(image!= null)
            Helpers.ImageHelper.LoadAndApplyImageAsync(fullFolderPath, imageUrl, image);
*/
    }

    public async Task LoadImageButton()
    {
        Debug.Log($"Loading {imageUrl}");

        await Helpers.ImageHelper.LoadAndApplyImageAsync(
            fullFolderPath,
            imageUrl,
            image);
        if (image != null) image.color = Color.white;

        if (aspectRationFitter != null)
        {
            if (image.sprite == null) return;

            aspectRationFitter.aspectRatio = (float)image.sprite.texture.width / image.sprite.texture.height;
        }
        Debug.Log($"Loaded {imageUrl}");
    }

    public void OnClick()
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || sectionView == null) return;
/*
        sectionView.ShowPhotoOnMainImageRect(imageUrl, titleText);*/
    }
}