
using System.Collections.Generic;
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

    public void OnUISwitch()
    {
        popupPanel.SetActive(false);
    }
}