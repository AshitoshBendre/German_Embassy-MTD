
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ImageButtonUI : MonoBehaviour
{
    private Button btn;
    private GallerySectionView sectionView;
    private string imageUrl;
    private string titleText;
    private string fullFolderPath;
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

        Debug.Log($"Loaded {imageUrl}");
    }

    public void OnClick()
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || sectionView == null) return;

        sectionView.ShowPhotoOnMainImageRect(imageUrl, titleText);
    }
}