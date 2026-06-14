
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GallerySectionView : MonoBehaviour, IProjectSectionView
{
    [Header("Top Section")]
    [SerializeField] private Image imageView;
    [SerializeField] private TMP_Text imagecaption;

    [Header("Bottom Section")]
    [SerializeField] private Transform gridcontentContainer;
    [SerializeField] private ImageButtonUI imageButtonPrefab;
    [SerializeField] private List<GameObject> objectsToShow;
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private GameObject TabButton;
    [SerializeField] private Button backButton;
    private ProjectContext projectContext;

    public void Initialize(ProjectContext context)
    {
        projectContext = context;
        ImageGridBuilder(context.Data.galleryTabData.galleryDatas);
    }


    private void ImageGridBuilder(List<GalleryData> data)
    {
        if (data.Count == 0) return;
        foreach(Transform child in gridcontentContainer)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < data.Count; i++)
        {
            /*
            var imgObj = new GameObject($"ImageButton{i}");
            imgObj.AddComponent<Button>();
            var img = imgObj.AddComponent<Image>();
            string fullFolderPath = $"{projectContext.PanelFolderId}/{projectContext.ProjectFolderId}";
            var imgBtn = imgObj.AddComponent<ImageButtonUI>();
            imgBtn.Initialize(this, data[i].imageURL, data[i].titleText);
            Helpers.ImageHelper.LoadAndApplyImageAsync(fullFolderPath, data[i].imageURL, img);
            imgObj.transform.SetParent(gridcontentContainer);
            */

            if (!IsGalleryDataValid( data[i]))
                continue; 
           

            var imageObj = Instantiate(imageButtonPrefab, gridcontentContainer);
            var imageBtnUI = imageObj.GetComponent<ImageButtonUI>();
            string fullFolderPath = $"{projectContext.PanelFolderId}/{projectContext.ProjectFolderId}";
            imageBtnUI.Initialize(this, data[i].imageURL, data[i].titleText, fullFolderPath);
            //Helpers.ImageHelper.LoadAndApplyImageAsync(fullFolderPath, data[i].imageURL, img);


            /*if(i== 0)
            {
                imgBtn.OnClick();
            }*/

        }
    }

    public void ShowPhotoOnMainImageRect(string imageURL, string titleText)
    {
        if (backButton!= null)
        {
            backButton.gameObject.SetActive(false);
        }
        popupPanel.SetActive(true);
        string fullFolderPath = $"{projectContext.PanelFolderId}/{projectContext.ProjectFolderId}";
        imagecaption.text = titleText;
        Helpers.ImageHelper.LoadAndApplyImageAsync(fullFolderPath, imageURL, imageView);

    }

    public void ShowUI()
    {
        foreach (GameObject showpanel in objectsToShow)
        {
            showpanel.SetActive(true);
        }
    }

    public void OnUIExit()
    {
        if (backButton != null)
        {
            backButton.gameObject.SetActive(true);
        }
        popupPanel.SetActive(false);
    }

    public void ValidateData(ProjectContext projectContext)
    {
        this.projectContext = projectContext;

        bool shouldShowTab = false;

        GalleryTabData galleryTabData = projectContext.Data.galleryTabData;

        if (galleryTabData == null)
        {
            Debug.LogWarning("[Gallery Validation] GalleryTabData is NULL.");
            TabButton.SetActive(false);
            return;
        }

        if (galleryTabData.galleryDatas == null)
        {
            Debug.LogWarning("[Gallery Validation] galleryDatas list is NULL.");
            TabButton.SetActive(false);
            return;
        }

        if (galleryTabData.galleryDatas.Count == 0)
        {
            Debug.LogWarning("[Gallery Validation] galleryDatas list is empty.");
            TabButton.SetActive(false);
            return;
        }

        foreach (var galleryData in galleryTabData.galleryDatas)
        {
            if (IsGalleryDataValid(galleryData))
            {
                shouldShowTab = true;
                break;
            }
        }

        if (!shouldShowTab)
        {
            Debug.LogWarning(
                $"[Gallery Validation] No valid gallery entries found for project '{projectContext.ProjectFolderId}'.");
        }

        TabButton.SetActive(shouldShowTab);
    }

    private bool IsGalleryDataValid(GalleryData galleryData)
    {
        if (galleryData == null)
        {
            Debug.LogWarning("[Gallery Validation] GalleryData entry is NULL.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(galleryData.imageURL))
        {
            Debug.LogWarning(
                $"[Gallery Validation] Invalid imageURL for entry with title '{galleryData.titleText}'.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(galleryData.titleText))
        {
            Debug.LogWarning(
                $"[Gallery Validation] Invalid titleText for image '{galleryData.imageURL}'.");
            return false;
        }

        string fullFolderPath =
            $"{projectContext.PanelFolderId}/{projectContext.ProjectFolderId}";

        string imagePath = Path.Combine(
            Application.streamingAssetsPath,
            fullFolderPath,
            galleryData.imageURL);

        if (!File.Exists(imagePath))
        {
            Debug.LogWarning(
                $"[Gallery Validation] Image file not found.\n" +
                $"Title: {galleryData.titleText}\n" +
                $"Image URL: {galleryData.imageURL}\n" +
                $"Expected Path: {imagePath}");
            return false;
        }

        return true;
    }
}