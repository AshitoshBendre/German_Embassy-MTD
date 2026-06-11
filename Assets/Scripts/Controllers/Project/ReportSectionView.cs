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
    private string FullFolderPath;

    public void Initialize(ProjectContext context)
    {
        FullFolderPath = $"{context.PanelFolderId}/{context.ProjectFolderId}";
        ReportListBuilder(context.Data.reportsTabData.reportDatas);
    }
    private void ReportListBuilder(List<ReportData> data)
    {
        if (data.Count == 0) return;

        foreach (Transform child in listContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < data.Count; i++)
        {
            var videoObj = Instantiate(_reportDataPrefab, listContainer);
            var reportBtnUI = videoObj.GetComponent<ReportDataUI>();
            reportBtnUI.Initialize(this, data[i].titleText, data[i].textData);
        }
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
        popupPanel.SetActive(true); 
        foreach (Transform child in textContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (string t in data)
        {
            var aboutObj = Instantiate(_aboutDataPrefab);
            var dataUI = aboutObj.GetComponent<AboutDataUI>();
            dataUI.Initialize(t);

            aboutObj.transform.SetParent(textContainer);
        }

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
}