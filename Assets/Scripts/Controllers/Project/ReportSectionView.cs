using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

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
    private string FullFolderPath;

    public void Initialize(ProjectContext context)
    {
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

            var reportObj = Instantiate(_reportDataPrefab, listContainer);

            reportObj.Initialize(
                this,
                reportData.titleText,
                reportData.textData);

            createdCount++;
        }

        Debug.Log(
            $"[Report Builder] Created {createdCount} report items. Skipped {skippedCount} invalid entries.");

    }
    public void OnUISwitch()
    {
        throw new System.NotImplementedException();
    }

    public void ShowUI()
    {
        foreach (GameObject showpanel in objectsToShow)
        {
            showpanel.SetActive(true);
        }
    }

    internal void ShowReportOnPopup(List<string> data)
    {
        if (data == null || data.Count == 0)
        {
            Debug.LogWarning("[Report Popup] Invalid report data.");
            return;
        }

        popupPanel.SetActive(true);

        foreach (Transform child in textContainer)
        {
            Destroy(child.gameObject);
        }

        int createdCount = 0;
        int skippedCount = 0;

        foreach (string text in data)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                skippedCount++;
                continue;
            }

            var aboutObj = Instantiate(_aboutDataPrefab, textContainer);
            aboutObj.Initialize(text); 
            createdCount++;
        }
        Debug.Log(
        $"[Report Popup] Created {createdCount} text blocks. Skipped {skippedCount} invalid entries.");
        StartCoroutine(RefreshLayoutRoutine());
    }
    private IEnumerator RefreshLayoutRoutine()
    {
        VerticalLayoutGroup verticalLayoutGroup = textContainer.GetComponent<VerticalLayoutGroup>();
        if (verticalLayoutGroup != null)
        {
            verticalLayoutGroup.enabled = false;

            // Wait for unity to finish the current frame's rendering and layout
            yield return new WaitForEndOfFrame();

            verticalLayoutGroup.enabled = true;
        }
    }

    public void ValidateData(ProjectContext projectContext)
    {
        bool shouldShowTab = false;

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

        if (string.IsNullOrWhiteSpace(reportData.titleText))
        {
            Debug.LogWarning("[Report Validation] Report title is empty.");
            return false;
        }

        if (reportData.textData == null)
        {
            Debug.LogWarning(
                $"[Report Validation] textData is NULL for report '{reportData.titleText}'.");
            return false;
        }

        if (reportData.textData.Count == 0)
        {
            Debug.LogWarning(
                $"[Report Validation] textData is empty for report '{reportData.titleText}'.");
            return false;
        }

        foreach (string text in reportData.textData)
        {
            if (!string.IsNullOrWhiteSpace(text))
                return true;
        }

        Debug.LogWarning(
            $"[Report Validation] No valid text entries found for report '{reportData.titleText}'.");

        return false;
    }
}