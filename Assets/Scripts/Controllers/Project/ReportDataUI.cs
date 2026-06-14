using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReportDataUI : MonoBehaviour
{
    private Button btn;
    [SerializeField] private TMP_Text text;
    private ReportSectionView sectionView;
    private string data;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    // Updated to accept titleText
    public void Initialize(ReportSectionView reportSectionView, string titleText, string pdfURL)
    {
        this.data = pdfURL;
        this.sectionView = reportSectionView;

        // Assign the title to the UI Text element
        if (this.text != null)
        {
            this.text.text = titleText;
        }
        else
        {
            Debug.LogWarning("[ReportDataUI] TMP_Text component is not assigned in the inspector.");
        }
    }

    private void OnClick()
    {
        if (sectionView == null)
        {
            Debug.LogWarning("[ReportDataUI] SectionView is null.");
            return;
        }

        if (data == null || string.IsNullOrEmpty(data))
        {
            Debug.LogWarning("[ReportDataUI] Report data is empty.");
            return;
        }
        sectionView.ShowReportOnPopup(data);
    }
}