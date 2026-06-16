using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GallerySectionView : MonoBehaviour, IProjectSectionView
{
    [Header("Top Section")]
    [SerializeField] private Image imageView;
    /*[SerializeField] private TMP_Text imagecaption;*/

    [Header("Bottom Section")]
    [SerializeField] private Transform gridcontentContainer;
    [SerializeField] private ImageButtonUI imageButtonPrefab;
    [SerializeField] private List<GameObject> objectsToShow;
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private GameObject TabButton;
    [SerializeField] private Button backButton;

    [Header("Navigation Controls")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    [Header("Dashboard Viewer (New)")]
    [SerializeField] private string projectID;
    [SerializeField] private GameObject dashboardViewerPanel;
    [SerializeField] private Image dashboardImageView; // Now using standard UI Image
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
    private Texture2D currentDashboardTexture;
    private Sprite currentDashboardSprite; // Track sprite to prevent memory leaks

    private void Awake()
    {
        // Bind the new dashboard buttons via code to ensure they always work
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
        List<ImageButtonUI> buttons = new();
        validGalleryList.Clear();

        foreach (Transform child in gridcontentContainer)
        {
            Destroy(child.gameObject);
        }
        LayoutGroup layoutGroup = gridcontentContainer.GetComponent<LayoutGroup>();

        if (layoutGroup != null)
            layoutGroup.enabled = false;

        foreach (var galleryData in data)
        {
            if (!IsGalleryDataValid(galleryData))
                continue;

            validGalleryList.Add(galleryData);

            var imageObj = Instantiate(imageButtonPrefab, gridcontentContainer);

            string fullFolderPath = $"{projectContext.PanelFolderId}/{projectContext.ProjectFolderId}";

            var imageBtnUI = imageObj.GetComponent<ImageButtonUI>();

            imageBtnUI.Initialize(
                this,
                galleryData.imageURL,
                galleryData.titleText,
                fullFolderPath);

            buttons.Add(imageBtnUI);
        }

        if (layoutGroup != null)
        {
            layoutGroup.enabled = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(gridcontentContainer as RectTransform);
        }

        await Task.Yield();
        await LoadButtonsInBatches(buttons, 1);
    }

    private async Task LoadButtonsInBatches(List<ImageButtonUI> buttons, int batchSize = 1)
    {
        for (int i = 0; i < buttons.Count; i += batchSize)
        {
            List<Task> batch = new();

            for (int j = i; j < Mathf.Min(i + batchSize, buttons.Count); j++)
            {
                batch.Add(buttons[j].LoadImageButton());
            }

            await Task.WhenAll(batch);
            await Task.Delay(400);
        }
    }

    public void ShowPhotoOnMainImageRect(string imageURL, string titleText)
    {
        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }

        popupPanel.SetActive(true);
        string fullFolderPath = $"{projectContext.PanelFolderId}/{projectContext.ProjectFolderId}";
        /*imagecaption.text = titleText;*/
        Helpers.ImageHelper.LoadAndApplyImageAsync(fullFolderPath, imageURL, imageView);

        currentImageIndex = validGalleryList.FindIndex(x => x.imageURL == imageURL);
        UpdateButtonStates();
    }

    public void ShowNextImage()
    {
        if (currentImageIndex >= 0 && currentImageIndex < validGalleryList.Count - 1)
        {
            currentImageIndex++;
            GalleryData data = validGalleryList[currentImageIndex];
            ShowPhotoOnMainImageRect(data.imageURL, data.titleText);
        }
    }

    public void ShowPreviousImage()
    {
        if (currentImageIndex > 0)
        {
            currentImageIndex--;
            GalleryData data = validGalleryList[currentImageIndex];
            ShowPhotoOnMainImageRect(data.imageURL, data.titleText);
        }
    }

    private void UpdateButtonStates()
    {
        if (nextButton != null)
        {
            bool hasNext = (currentImageIndex >= 0 && currentImageIndex < validGalleryList.Count - 1);
            nextButton.gameObject.SetActive(hasNext);
        }

        if (prevButton != null)
        {
            bool hasPrev = (currentImageIndex > 0);
            prevButton.gameObject.SetActive(hasPrev);
        }
    }

    // =========================================================
    // --- NEW DASHBOARD VIEWER LOGIC ---
    // =========================================================

    public void OpenDashboardViewer()
    {
        dashboardViewerPanel.SetActive(true);
        dashboardImagePaths.Clear();

        // Target path: StreamingAssets/PanelFolderID/Dashboard/
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
            if (dashboardImageView != null) dashboardImageView.sprite = null;
            UpdateDashboardButtons();
        }
    }

    private void DisplayDashboardImage(int index)
    {
        if (index < 0 || index >= dashboardImagePaths.Count || dashboardImageView == null) return;

        string imagePath = dashboardImagePaths[index];

        if (File.Exists(imagePath))
        {
            // CRITICAL: Destroy both the old sprite AND the old texture to prevent memory leaks
            if (currentDashboardSprite != null) Destroy(currentDashboardSprite);
            if (currentDashboardTexture != null) Destroy(currentDashboardTexture);

            // 1. Load the raw bytes into a Texture2D
            byte[] fileData = File.ReadAllBytes(imagePath);
            currentDashboardTexture = new Texture2D(2, 2);
            currentDashboardTexture.LoadImage(fileData);

            // 2. Convert the Texture2D into a UI Sprite
            currentDashboardSprite = Sprite.Create(
                currentDashboardTexture,
                new Rect(0, 0, currentDashboardTexture.width, currentDashboardTexture.height),
                new Vector2(0.5f, 0.5f) // Centers the pivot
            );

            // 3. Assign to the Image component
            dashboardImageView.sprite = currentDashboardSprite;
            dashboardImageView.preserveAspect = true;

            // --- STRICT ASPECT RATIO FITTER ---
            // This guarantees the image scales properly inside its parent bounds without stretching
            AspectRatioFitter aspectFitter = dashboardImageView.GetComponent<AspectRatioFitter>();
            if (aspectFitter == null)
            {
                aspectFitter = dashboardImageView.gameObject.AddComponent<AspectRatioFitter>();
            }
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            aspectFitter.aspectRatio = (float)currentDashboardTexture.width / currentDashboardTexture.height;
        }

        UpdateDashboardButtons();
    }

    private void DashboardNextImage()
    {
        // SAFETY CHECK: Only proceed if THIS specific dashboard panel is actually open
        if (dashboardViewerPanel == null || !dashboardViewerPanel.activeSelf) return;

        if (currentDashboardIndex < dashboardImagePaths.Count - 1)
        {
            currentDashboardIndex++;
            DisplayDashboardImage(currentDashboardIndex);
        }
    }

    private void DashboardPrevImage()
    {
        // SAFETY CHECK: Only proceed if THIS specific dashboard panel is actually open
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
        // SAFETY CHECK: Only close and clean up if THIS specific panel is the open one
        if (dashboardViewerPanel == null || !dashboardViewerPanel.activeSelf) return;

        dashboardViewerPanel.SetActive(false);
        currentDashboardIndex = -1;

        // Clean up both texture and sprite
        if (currentDashboardSprite != null) Destroy(currentDashboardSprite);
        if (currentDashboardTexture != null) Destroy(currentDashboardTexture);

        if (dashboardImageView != null) dashboardImageView.sprite = null;
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

        // Force close without active check when exiting the entire UI section
        if (dashboardViewerPanel != null) dashboardViewerPanel.SetActive(false);
        if (currentDashboardSprite != null) Destroy(currentDashboardSprite);
        if (currentDashboardTexture != null) Destroy(currentDashboardTexture);
        if (dashboardImageView != null) dashboardImageView.sprite = null;
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

            StartGalleryPreload(projectContext);
            TabButton.SetActive(shouldShowTab);
        }
    }

    private async void StartGalleryPreload(ProjectContext projectContext)
    {
        GalleryTabData galleryTabData = projectContext.Data.galleryTabData;

        if (galleryTabData?.galleryDatas == null)
            return;

        string fullFolderPath = $"{projectContext.PanelFolderId}/{projectContext.ProjectFolderId}";

        List<Task> preloadTasks = new();

        foreach (var galleryData in galleryTabData.galleryDatas)
        {
            if (!IsGalleryDataValid(galleryData))
                continue;

            preloadTasks.Add(
                Helpers.ImageHelper.PreloadImageAsync(
                    fullFolderPath,
                    galleryData.imageURL));
        }

        await Task.WhenAny(
            Task.WhenAll(preloadTasks),
            Task.Delay(1000));
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
        // Clean up loaded dashboard texture to avoid memory leaks
        if (currentDashboardSprite != null) Destroy(currentDashboardSprite);
        if (currentDashboardTexture != null) Destroy(currentDashboardTexture);
    }
}