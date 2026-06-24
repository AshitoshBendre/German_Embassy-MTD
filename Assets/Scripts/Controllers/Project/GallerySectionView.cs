using ScrollCarousel;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GallerySectionView : MonoBehaviour, IProjectSectionView
{
    [Header("Top Section")]
    [SerializeField] private Carousel carousel;

    [Header("Bottom Section")]
    [SerializeField] private Transform gridcontentContainer;
    [SerializeField] private ImageButtonUI imageButtonPrefab;
    [SerializeField] private List<GameObject> objectsToShow;
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private GameObject TabButton;
    [SerializeField] private Button backButton;

    [Header("Dashboard Viewer (Updated to RawImage)")]
    [SerializeField] private string projectID;
    [SerializeField] private GameObject dashboardViewerPanel;
    [SerializeField] private RawImage dashboardRawImage; // Now using RawImage directly
    [SerializeField] private Button dashboardNextButton;
    [SerializeField] private Button dashboardPrevButton;
    [SerializeField] private Button dashboardCloseButton;

    private ProjectContext projectContext;

    // State for main gallery
    private int currentImageIndex = -1;
    private List<GalleryData> validGalleryList = new();
    public bool canValidate = true;

    // State for dashboard viewer
    private List<string> dashboardImagePaths = new List<string>();
    private int currentDashboardIndex = -1;
    private Texture2D currentDashboardTexture; // Only tracking raw texture now!

    private void Awake()
    {
        if (dashboardNextButton != null) dashboardNextButton.onClick.AddListener(DashboardNextImage);
        if (dashboardPrevButton != null) dashboardPrevButton.onClick.AddListener(DashboardPrevImage);
        if (dashboardCloseButton != null) dashboardCloseButton.onClick.AddListener(CloseDashboardViewer);
    }

    public void Initialize(ProjectContext context)
    {
        projectContext = context;
        ImageGridBuilder(context.Data.galleryTabData.galleryDatas);
    }

    private async void ImageGridBuilder(List<GalleryData> data)
    {
        List<RectTransform> generatedRects = new List<RectTransform>();
        validGalleryList.Clear();
        if (carousel != null) carousel.ClearItems();

        foreach (Transform child in gridcontentContainer)
        {
            Destroy(child.gameObject);
        }

        LayoutGroup layoutGroup = gridcontentContainer.GetComponent<LayoutGroup>();
        if (layoutGroup != null) layoutGroup.enabled = false;

        int count = 0;
        string fullFolderPath = $"{projectContext.PanelFolderId}/{projectContext.ProjectFolderId}";

        foreach (var galleryData in data)
        {
            if (!IsGalleryDataValid(galleryData)) continue;

            validGalleryList.Add(galleryData);

            var imageObj = Instantiate(imageButtonPrefab, gridcontentContainer);
            generatedRects.Add(imageObj.GetComponent<RectTransform>());

            imageObj.AddComponent<CarouselButton>();
            var imageBtnUI = imageObj.GetComponent<ImageButtonUI>();

            imageBtnUI.Initialize(this, galleryData.imageURL, fullFolderPath);
            imageObj.gameObject.name = $"Image {count}";

            _ = imageBtnUI.LoadImageButton();

            count++;

            if (count % 3 == 0)
            {
                await Task.Yield();
            }
        }

        if (carousel != null && generatedRects.Count > 0)
        {
            int calculatedStart = generatedRects.Count / 2;

            carousel.StartItem = calculatedStart;
            carousel.SetItems(generatedRects);
            carousel.ForceUpdate();
        }
    }

    private async Task LoadButtonsInBatches(List<ImageButtonUI> buttons, int batchSize = 1)
    {
        if (carousel != null)
            carousel.ForceUpdate();
        for (int i = 0; i < buttons.Count; i += batchSize)
        {
            List<Task> batch = new();

            for (int j = i; j < Mathf.Min(i + batchSize, buttons.Count); j++)
            {
                batch.Add(buttons[j].LoadImageButton());
            }

            await Task.WhenAll(batch);
            await Task.Delay(1);
        }
    }

    // =========================================================
    // --- UPDATED DASHBOARD VIEWER LOGIC (RAW IMAGE) ---
    // =========================================================
    public void OpenDashboardViewer()
    {
        dashboardViewerPanel.SetActive(true);
        dashboardImagePaths.Clear();

        string dashboardDirectory = Path.Combine(
            Application.streamingAssetsPath,
            projectID,
            "Dashboard");

        if (Directory.Exists(dashboardDirectory))
        {
            string[] files = Directory.GetFiles(dashboardDirectory);
            foreach (string file in files)
            {
                if (file.EndsWith(".meta")) continue;

                string extension = Path.GetExtension(file).ToLower();
                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                {
                    dashboardImagePaths.Add(file);
                }
            }
        }
        else
        {
            Debug.LogWarning($"[Dashboard Viewer] Directory does not exist: {dashboardDirectory}");
        }

        if (dashboardImagePaths.Count > 0)
        {
            currentDashboardIndex = 0;
            DisplayDashboardImage(currentDashboardIndex);
        }
        else
        {
            Debug.LogWarning("[Dashboard Viewer] No valid images found in Dashboard folder.");
            if (dashboardRawImage != null) dashboardRawImage.texture = null;
            UpdateDashboardButtons();
        }
    }

    private void DisplayDashboardImage(int index)
    {
        if (index < 0 || index >= dashboardImagePaths.Count || dashboardRawImage == null) return;

        string imagePath = dashboardImagePaths[index];

        if (File.Exists(imagePath))
        {
            // Destroy old texture to prevent memory leaks
            if (currentDashboardTexture != null) Destroy(currentDashboardTexture);

            // 1. Load the raw bytes directly into Texture2D (Zero Sprite CPU overhead!)
            byte[] fileData = File.ReadAllBytes(imagePath);
            currentDashboardTexture = new Texture2D(2, 2);
            currentDashboardTexture.LoadImage(fileData);

            // 2. Assign directly to RawImage
            dashboardRawImage.texture = currentDashboardTexture;

            // 3. Strict Aspect Ratio Fitter
            AspectRatioFitter aspectFitter = dashboardRawImage.GetComponent<AspectRatioFitter>();
            if (aspectFitter == null)
            {
                aspectFitter = dashboardRawImage.gameObject.AddComponent<AspectRatioFitter>();
            }
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            aspectFitter.aspectRatio = (float)currentDashboardTexture.width / currentDashboardTexture.height;

            // 4. Center and Anchor properly (Ported from Report viewer)
            RectTransform rawImageRect = dashboardRawImage.GetComponent<RectTransform>();
            rawImageRect.pivot = new Vector2(0.5f, 0.5f);
            rawImageRect.anchorMin = new Vector2(0.5f, 0.5f);
            rawImageRect.anchorMax = new Vector2(0.5f, 0.5f);
            rawImageRect.anchoredPosition = Vector2.zero;
        }

        UpdateDashboardButtons();
    }

    private void DashboardNextImage()
    {
        if (dashboardViewerPanel == null || !dashboardViewerPanel.activeSelf) return;

        if (currentDashboardIndex < dashboardImagePaths.Count - 1)
        {
            currentDashboardIndex++;
            DisplayDashboardImage(currentDashboardIndex);
        }
    }

    private void DashboardPrevImage()
    {
        if (dashboardViewerPanel == null || !dashboardViewerPanel.activeSelf) return;

        if (currentDashboardIndex > 0)
        {
            currentDashboardIndex--;
            DisplayDashboardImage(currentDashboardIndex);
        }
    }

    private void UpdateDashboardButtons()
    {
        if (dashboardNextButton != null)
            dashboardNextButton.gameObject.SetActive(currentDashboardIndex < dashboardImagePaths.Count - 1);

        if (dashboardPrevButton != null)
            dashboardPrevButton.gameObject.SetActive(currentDashboardIndex > 0);
    }

    private void CloseDashboardViewer()
    {
        if (dashboardViewerPanel == null || !dashboardViewerPanel.activeSelf) return;

        dashboardViewerPanel.SetActive(false);
        currentDashboardIndex = -1;

        if (currentDashboardTexture != null) Destroy(currentDashboardTexture);
        if (dashboardRawImage != null) dashboardRawImage.texture = null;
    }

    // =========================================================

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
        currentImageIndex = -1;

        if (dashboardViewerPanel != null) dashboardViewerPanel.SetActive(false);
        if (currentDashboardTexture != null) Destroy(currentDashboardTexture);
        if (dashboardRawImage != null) dashboardRawImage.texture = null;
    }

    public void ValidateData(ProjectContext projectContext)
    {
        if (canValidate)
        {
            this.projectContext = projectContext;

            bool shouldShowTab = false;

            GalleryTabData galleryTabData = projectContext.Data.galleryTabData;

            if (galleryTabData == null || galleryTabData.galleryDatas == null || galleryTabData.galleryDatas.Count == 0)
            {
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

            TabButton.SetActive(shouldShowTab);
        }
    }

    private void StartGalleryPreload(ProjectContext projectContext)
    {
        GalleryTabData galleryTabData = projectContext.Data.galleryTabData;
        if (galleryTabData?.galleryDatas == null) return;

        string fullFolderPath = $"{projectContext.PanelFolderId}/{projectContext.ProjectFolderId}";

        foreach (var galleryData in galleryTabData.galleryDatas)
        {
            if (!IsGalleryDataValid(galleryData)) continue;

            _ = Helpers.ImageHelper.PreloadImageAsync(fullFolderPath, galleryData.imageURL);
        }
    }

    private bool IsGalleryDataValid(GalleryData galleryData)
    {
        if (galleryData == null || string.IsNullOrWhiteSpace(galleryData.imageURL))
        {
            return false;
        }
        return true;
    }

    private void OnDestroy()
    {
        if (currentDashboardTexture != null) Destroy(currentDashboardTexture);
    }
}