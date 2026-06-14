using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityPdfViewer;

public class ReportSectionView : MonoBehaviour, IProjectSectionView
{
    [Header("PopupData")]
    [SerializeField] private Transform textContainer;
    [SerializeField] private AboutDataUI _aboutDataPrefab;

    [Header("PopupData")]
    [SerializeField] private Transform listContainer;
    [SerializeField] private ReportDataUI _reportDataPrefab;
    [SerializeField] private List<GameObject> objectsToShow;
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private GameObject TabButton;
    [SerializeField] private PdfViewerUI _pdfViewerUI;
    [SerializeField] private Button backButton;
    private string FullFolderPath;
    private ProjectContext projectContext;

    public void Initialize(ProjectContext context)
    {
        if (_pdfViewerUI == null)
        {
            Debug.LogError("PDF VIEWER IS NULL");
        }

        _pdfViewerUI.pathMode = PdfPathMode.RelativeToStreamingAssets;
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

            string reportPath = Path.Combine(
                Application.streamingAssetsPath,
                FullFolderPath,
                reportData.pdfURL);

            var reportObj = Instantiate(_reportDataPrefab, listContainer);

            // Passed titleText into the UI initializer
            reportObj.Initialize(
                this,
                reportData.titleText,
                reportPath);

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
    }

    public void ShowUI()
    {
        foreach (GameObject showpanel in objectsToShow)
        {
            showpanel.SetActive(true);
        }
    }

    internal void ShowReportOnPopup(string data)
    {
        if (data == null || string.IsNullOrEmpty(data))
        {
            Debug.LogWarning("[Report Popup] Invalid report data.");
            return;
        }

        popupPanel.SetActive(true);

        if (_pdfViewerUI == null)
        {
            Debug.LogError("PDF VIEWER IS NULL");
        }
        if (backButton != null)
        { 
            backButton.gameObject.SetActive(false);
        }
        _pdfViewerUI.pathMode = PdfPathMode.RelativeToStreamingAssets;
        _pdfViewerUI.pdfPath = data;
        _pdfViewerUI.LoadPDF();
    }

    public void ValidateData(ProjectContext projectContext)
    {
        Debug.Log($"<color=cyan>[Report Validation] Context received! Project Title is: {projectContext.Data.projectTitle}</color>");

        bool shouldShowTab = false;
        this.projectContext = projectContext;
        ReportsTabData reportsTabData = projectContext.Data.reportsTabData;

        if (reportsTabData == null)
        {
            Debug.LogWarning("[Report Validation] ReportsTabData is NULL.");
            TabButton.SetActive(false);
            return;
        }

        if (reportsTabData.reportDatas == null)
        {
            Debug.LogWarning("[Report Validation] reportDatas list is NULL.");
            TabButton.SetActive(false);
            return;
        }

        if (reportsTabData.reportDatas.Count == 0)
        {
            Debug.LogWarning("[Report Validation] reportDatas list is empty.");
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

        if (!shouldShowTab)
        {
            Debug.LogWarning("[Report Validation] No valid report entries found.");
        }

        TabButton.SetActive(shouldShowTab);
    }

    private bool IsReportDataValid(ReportData reportData)
    {
        if (reportData == null)
        {
            Debug.LogWarning("[Report Validation] ReportData entry is NULL.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(reportData.pdfURL))
        {
            Debug.LogWarning("[Report Validation] Invalid pdfURL");
            return false;
        }

        // Added validation check for the titleText
        if (string.IsNullOrWhiteSpace(reportData.titleText))
        {
            Debug.LogWarning($"[Report Validation] Invalid titleText for report '{reportData.pdfURL}'.");
            return false;
        }

        string fullFolderPath = $"{projectContext.PanelFolderId}/{projectContext.ProjectFolderId}";

        string reportPath = Path.Combine(
            Application.streamingAssetsPath,
            fullFolderPath,
            reportData.pdfURL);

        if (!File.Exists(reportPath))
        {
            Debug.LogWarning(
                $"[Report Validation] PDF file not found.\n" +
                $"Title: {reportData.titleText}\n" +
                $"PDF URL: {reportData.pdfURL}\n" +
                $"Expected Path: {reportPath}");
            return false;
        }

        return true;
    }
}