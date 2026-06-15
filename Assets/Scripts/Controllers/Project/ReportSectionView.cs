using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ReportSectionView : MonoBehaviour, IProjectSectionView
{
    [Header("List Generation")]
    [SerializeField] private Transform listContainer;
    [SerializeField] private ReportDataUI _reportDataPrefab;
    [SerializeField] private List<GameObject> objectsToShow;
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private GameObject TabButton;
    [SerializeField] private Button backButton;

    [Header("Image Slideshow Viewer")]
    [SerializeField] private RawImage _reportRawImage; // Replaces the PDF Viewer
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _prevButton;

    private string FullFolderPath;
    private ProjectContext projectContext;
    public bool canValidate = true;

    // State management for the current slideshow
    private List<string> _currentImagePaths = new List<string>();
    private int _currentImageIndex = 0;
    private Texture2D _currentLoadedTexture;

    private void Awake()
    {
        // Bind the navigation buttons via code to ensure they always work
        if (_nextButton != null) _nextButton.onClick.AddListener(NextImage);
        if (_prevButton != null) _prevButton.onClick.AddListener(PrevImage);
    }

    public void Initialize(ProjectContext context)
    {
        projectContext = context;
        FullFolderPath = $"{context.PanelFolderId}/{context.ProjectFolderId}";
        ReportListBuilder(context.Data.reportsTabData.reportDatas);
    }

    private void ReportListBuilder(List<ReportData> data)
    {
        if (data == null)
        {
            Debug.LogWarning("[Report Builder] Data list is NULL.");
            return;
        }

        if (data.Count == 0)
        {
            Debug.LogWarning("[Report Builder] Data list is empty.");
            return;
        }

        foreach (Transform child in listContainer)
        {
            Destroy(child.gameObject);
        }

        int createdCount = 0;
        int skippedCount = 0;

        foreach (var reportData in data)
        {
            if (!IsReportDataValid(reportData))
            {
                skippedCount++;
                continue;
            }

            // reportData.pdfURL is now treated as a DIRECTORY name
            string reportDirectoryPath = Path.Combine(
                Application.streamingAssetsPath,
                FullFolderPath,
                reportData.pdfURL);

            var reportObj = Instantiate(_reportDataPrefab, listContainer);

            // Pass the absolute directory path to the UI element
            reportObj.Initialize(
                this,
                reportData.titleText,
                reportDirectoryPath);

            createdCount++;
        }

        Debug.Log($"[Report Builder] Created {createdCount} report items. Skipped {skippedCount} invalid entries.");
    }

    public void OnUIExit()
    {
        if (backButton != null)
        {
            backButton.gameObject.SetActive(true);
        }

        // Clean up memory when exiting the view
        if (_currentLoadedTexture != null)
        {
            Destroy(_currentLoadedTexture);
            _reportRawImage.texture = null;
        }
    }

    public void ShowUI()
    {
        foreach (GameObject showpanel in objectsToShow)
        {
            showpanel.SetActive(true);
        }
    }

    // Called by ReportDataUI when a report button is clicked
    internal void ShowReportOnPopup(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
        {
            Debug.LogWarning("[Report Popup] Invalid directory path or directory does not exist.");
            return;
        }

        popupPanel.SetActive(true);

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }

        LoadImagesFromDirectory(directoryPath);
    }

    private void LoadImagesFromDirectory(string directoryPath)
    {
        _currentImagePaths.Clear();

        // Get all files in the directory
        string[] files = Directory.GetFiles(directoryPath);

        // Filter out meta files and only keep image formats
        foreach (string file in files)
        {
            if (file.EndsWith(".meta")) continue;

            string extension = Path.GetExtension(file).ToLower();
            if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
            {
                _currentImagePaths.Add(file);
            }
        }

        if (_currentImagePaths.Count > 0)
        {
            _currentImageIndex = 0;
            DisplayImageAtIndex(_currentImageIndex);
        }
        else
        {
            Debug.LogWarning($"[Report Popup] No valid images found in directory: {directoryPath}");
            if (_reportRawImage != null) _reportRawImage.texture = null;
            UpdateButtonInteractability();
        }
    }

    private void DisplayImageAtIndex(int index)
    {
        if (index < 0 || index >= _currentImagePaths.Count || _reportRawImage == null) return;

        string imagePath = _currentImagePaths[index];

        if (File.Exists(imagePath))
        {
            // Clean up the old texture to prevent memory leaks
            if (_currentLoadedTexture != null)
            {
                Destroy(_currentLoadedTexture);
            }

            // Load the new image directly from disk
            byte[] fileData = File.ReadAllBytes(imagePath);
            _currentLoadedTexture = new Texture2D(2, 2);
            _currentLoadedTexture.LoadImage(fileData);

            _reportRawImage.texture = _currentLoadedTexture;
        }

        UpdateButtonInteractability();
    }

    public void NextImage()
    {
        if (_currentImageIndex < _currentImagePaths.Count - 1)
        {
            _currentImageIndex++;
            DisplayImageAtIndex(_currentImageIndex);
        }
    }

    public void PrevImage()
    {
        if (_currentImageIndex > 0)
        {
            _currentImageIndex--;
            DisplayImageAtIndex(_currentImageIndex);
        }
    }

    private void UpdateButtonInteractability()
    {
        if (_nextButton != null) _nextButton.interactable = (_currentImageIndex < _currentImagePaths.Count - 1);
        if (_prevButton != null) _prevButton.interactable = (_currentImageIndex > 0);
    }

    public void ValidateData(ProjectContext projectContext)
    {
        if (canValidate)
        {
            bool shouldShowTab = false;
            this.projectContext = projectContext;
            ReportsTabData reportsTabData = projectContext.Data.reportsTabData;

            if (reportsTabData == null || reportsTabData.reportDatas == null || reportsTabData.reportDatas.Count == 0)
            {
                TabButton.SetActive(false);
                return;
            }

            foreach (var reportData in reportsTabData.reportDatas)
            {
                if (IsReportDataValid(reportData))
                {
                    shouldShowTab = true;
                    break;
                }
            }

            TabButton.SetActive(shouldShowTab);
        }
    }

    private bool IsReportDataValid(ReportData reportData)
    {
        if (reportData == null || string.IsNullOrWhiteSpace(reportData.pdfURL) || string.IsNullOrWhiteSpace(reportData.titleText))
        {
            return false;
        }

        string fullFolderPath = $"{projectContext.PanelFolderId}/{projectContext.ProjectFolderId}";

        string reportDirectoryPath = Path.Combine(
            Application.streamingAssetsPath,
            fullFolderPath,
            reportData.pdfURL);

        // CHANGED: We now check if the DIRECTORY exists, rather than a single file.
        if (!Directory.Exists(reportDirectoryPath))
        {
            Debug.LogWarning(
                $"[Report Validation] Image directory not found.\n" +
                $"Title: {reportData.titleText}\n" +
                $"Expected Directory: {reportDirectoryPath}");
            return false;
        }

        return true;
    }

    private void OnDestroy()
    {
        // Safety cleanup to prevent memory leaks when the scene/object is destroyed
        if (_currentLoadedTexture != null)
        {
            Destroy(_currentLoadedTexture);
        }
    }
}