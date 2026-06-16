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

    [Header("Special Report Viewer (New)")]
    [SerializeField] private GameObject _specialReportPanel;
    [SerializeField] private RawImage _specialReportRawImage;
    [SerializeField] private Button _specialNextButton;
    [SerializeField] private Button _specialPrevButton;

    private string FullFolderPath;
    private ProjectContext projectContext;
    public bool canValidate = true;

    // State management for the regular slideshow
    private List<string> _currentImagePaths = new List<string>();
    private int _currentImageIndex = 0;
    private Texture2D _currentLoadedTexture;

    // State management for the Special Report slideshow
    private List<string> _specialImagePaths = new List<string>();
    private int _currentSpecialIndex = 0;
    private Texture2D _currentSpecialTexture;

    private void Awake()
    {
        // Bind the standard navigation buttons
        if (_nextButton != null) _nextButton.onClick.AddListener(NextImage);
        if (_prevButton != null) _prevButton.onClick.AddListener(PrevImage);

        // Bind the special report navigation buttons
        if (_specialNextButton != null) _specialNextButton.onClick.AddListener(NextSpecialImage);
        if (_specialPrevButton != null) _specialPrevButton.onClick.AddListener(PrevSpecialImage);
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

            string reportDirectoryPath = Path.Combine(
                Application.streamingAssetsPath,
                FullFolderPath,
                reportData.pdfURL);

            var reportObj = Instantiate(_reportDataPrefab, listContainer);

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

        // Clean up memory when exiting the standard view
        if (_currentLoadedTexture != null)
        {
            Destroy(_currentLoadedTexture);
            _reportRawImage.texture = null;
        }

        // Ensure special report is closed as well
        HideSpecialReport();
    }

    public void ShowUI()
    {
        foreach (GameObject showpanel in objectsToShow)
        {
            showpanel.SetActive(true);
        }
    }

    // =========================================================
    // --- STANDARD REPORT VIEWER LOGIC ---
    // =========================================================

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
        string[] files = Directory.GetFiles(directoryPath);

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
        if (index < 0 || index >= _currentImagePaths.Count || _reportRawImage == null)
        {
            // Safe check for the UI Instance, then pass the Prefix, Message, and Color (Orange)
            if (Helpers.ImageDebuggerUI.Instance != null)
            {
                Helpers.ImageDebuggerUI.Instance.AddLog("[WARN]", $"[Report Viewer] Aborted Display. Invalid index ({index}) or missing RawImage.", "#FFA500");
            }
            return;
        }

        string imagePath = _currentImagePaths[index];

        // Prefix, Message, Color (Cyan)
        if (Helpers.ImageDebuggerUI.Instance != null)
        {
            Helpers.ImageDebuggerUI.Instance.AddLog("[INFO]", $"[Report Viewer] Attempting to load image at index {index}: {imagePath}", "#00FFFF");
        }

        if (File.Exists(imagePath))
        {
            if (_currentLoadedTexture != null) Destroy(_currentLoadedTexture);

            // 1. Read bytes and create texture
            byte[] fileData = File.ReadAllBytes(imagePath);
            _currentLoadedTexture = new Texture2D(2, 2);
            _currentLoadedTexture.LoadImage(fileData);

            _reportRawImage.texture = _currentLoadedTexture;

            // 2. Format Aspect Ratio
            AspectRatioFitter aspectFitter = _reportRawImage.GetComponent<AspectRatioFitter>();
            if (aspectFitter == null) aspectFitter = _reportRawImage.gameObject.AddComponent<AspectRatioFitter>();

            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            aspectFitter.aspectRatio = (float)_currentLoadedTexture.width / _currentLoadedTexture.height;

            // 3. Center and Anchor
            RectTransform rawImageRect = _reportRawImage.GetComponent<RectTransform>();
            rawImageRect.pivot = new Vector2(0.5f, 0.5f);
            rawImageRect.anchorMin = new Vector2(0.5f, 0.5f);
            rawImageRect.anchorMax = new Vector2(0.5f, 0.5f);
            rawImageRect.anchoredPosition = Vector2.zero;

            // Success Log (Green)
            if (Helpers.ImageDebuggerUI.Instance != null)
            {
                Helpers.ImageDebuggerUI.Instance.AddLog("[SUCCESS]", $"[Report Viewer] Formatted image: {System.IO.Path.GetFileName(imagePath)}", "#00FF00");
            }
        }
        else
        {
            // Error Log (Red)
            if (Helpers.ImageDebuggerUI.Instance != null)
            {
                Helpers.ImageDebuggerUI.Instance.AddLog("[ERROR]", $"[Report Viewer] Disk Error: File not found at path: {imagePath}", "#FF0000");
            }
        }

        UpdateButtonInteractability();
    }

    /*private void DisplayImageAtIndex(int index)
    {
        if (index < 0 || index >= _currentImagePaths.Count || _reportRawImage == null) return;
        
        string imagePath = _currentImagePaths[index];

        if (File.Exists(imagePath))
        {
            
            if (_currentLoadedTexture != null) Destroy(_currentLoadedTexture);

            byte[] fileData = File.ReadAllBytes(imagePath);
            _currentLoadedTexture = new Texture2D(2, 2);
            _currentLoadedTexture.LoadImage(fileData);

            _reportRawImage.texture = _currentLoadedTexture;

            AspectRatioFitter aspectFitter = _reportRawImage.GetComponent<AspectRatioFitter>();
            if (aspectFitter == null) aspectFitter = _reportRawImage.gameObject.AddComponent<AspectRatioFitter>();

            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            aspectFitter.aspectRatio = (float)_currentLoadedTexture.width / _currentLoadedTexture.height;

            RectTransform rawImageRect = _reportRawImage.GetComponent<RectTransform>();
            rawImageRect.pivot = new Vector2(0.5f, 0.5f);
            rawImageRect.anchorMin = new Vector2(0.5f, 0.5f);
            rawImageRect.anchorMax = new Vector2(0.5f, 0.5f);
            rawImageRect.anchoredPosition = Vector2.zero;
        }

        UpdateButtonInteractability();
    }*/

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

    // =========================================================
    // --- SPECIAL REPORT VIEWER LOGIC (NEW) ---
    // =========================================================

    public void ShowSpecialReport()
    {
        if (projectContext == null) return;

        _specialReportPanel.SetActive(true);
        _specialImagePaths.Clear();

        // Path: StreamingAssets/PanelID/ProjectID/Special Report
        string specialDirectoryPath = Path.Combine(
            Application.streamingAssetsPath,
            projectContext.PanelFolderId,
            projectContext.ProjectFolderId,
            "Special Report");

        if (Directory.Exists(specialDirectoryPath))
        {
            string[] files = Directory.GetFiles(specialDirectoryPath);
            foreach (string file in files)
            {
                if (file.EndsWith(".meta")) continue;

                string extension = Path.GetExtension(file).ToLower();
                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                {
                    _specialImagePaths.Add(file);
                }
            }
        }
        else
        {
            Debug.LogWarning($"[Special Report] Directory not found: {specialDirectoryPath}");
        }

        if (_specialImagePaths.Count > 0)
        {
            _currentSpecialIndex = 0;
            DisplaySpecialImageAtIndex(_currentSpecialIndex);
        }
        else
        {
            Debug.LogWarning("[Special Report] No valid images found.");
            if (_specialReportRawImage != null) _specialReportRawImage.texture = null;
            UpdateSpecialButtonInteractability();
        }
    }

    public void HideSpecialReport()
    {
        if (_specialReportPanel != null) _specialReportPanel.SetActive(false);

        _currentSpecialIndex = -1;

        if (_currentSpecialTexture != null)
        {
            Destroy(_currentSpecialTexture);
            if (_specialReportRawImage != null) _specialReportRawImage.texture = null;
        }
    }

    private void DisplaySpecialImageAtIndex(int index)
    {
        if (index < 0 || index >= _specialImagePaths.Count || _specialReportRawImage == null) return;

        string imagePath = _specialImagePaths[index];

        if (File.Exists(imagePath))
        {
            if (_currentSpecialTexture != null) Destroy(_currentSpecialTexture);

            byte[] fileData = File.ReadAllBytes(imagePath);
            _currentSpecialTexture = new Texture2D(2, 2);
            _currentSpecialTexture.LoadImage(fileData);

            _specialReportRawImage.texture = _currentSpecialTexture;

            // Enforce aspect ratio and centering
            AspectRatioFitter aspectFitter = _specialReportRawImage.GetComponent<AspectRatioFitter>();
            if (aspectFitter == null) aspectFitter = _specialReportRawImage.gameObject.AddComponent<AspectRatioFitter>();

            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            aspectFitter.aspectRatio = (float)_currentSpecialTexture.width / _currentSpecialTexture.height;

            RectTransform rawImageRect = _specialReportRawImage.GetComponent<RectTransform>();
            rawImageRect.pivot = new Vector2(0.5f, 0.5f);
            rawImageRect.anchorMin = new Vector2(0.5f, 0.5f);
            rawImageRect.anchorMax = new Vector2(0.5f, 0.5f);
            rawImageRect.anchoredPosition = Vector2.zero;
        }

        UpdateSpecialButtonInteractability();
    }

    public void NextSpecialImage()
    {
        if (_specialReportPanel == null || !_specialReportPanel.activeSelf) return;

        if (_currentSpecialIndex < _specialImagePaths.Count - 1)
        {
            _currentSpecialIndex++;
            DisplaySpecialImageAtIndex(_currentSpecialIndex);
        }
    }

    public void PrevSpecialImage()
    {
        if (_specialReportPanel == null || !_specialReportPanel.activeSelf) return;

        if (_currentSpecialIndex > 0)
        {
            _currentSpecialIndex--;
            DisplaySpecialImageAtIndex(_currentSpecialIndex);
        }
    }

    private void UpdateSpecialButtonInteractability()
    {
        if (_specialNextButton != null) _specialNextButton.interactable = (_currentSpecialIndex < _specialImagePaths.Count - 1);
        if (_specialPrevButton != null) _specialPrevButton.interactable = (_currentSpecialIndex > 0);
    }

    // =========================================================

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
        if (_currentLoadedTexture != null) Destroy(_currentLoadedTexture);
        if (_currentSpecialTexture != null) Destroy(_currentSpecialTexture);
    }
}