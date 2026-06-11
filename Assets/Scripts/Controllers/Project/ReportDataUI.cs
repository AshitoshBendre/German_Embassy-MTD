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
    private List<String> data;
    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    public void Initialize(ReportSectionView reportSectionView, string titleText, List<String> data)
    {
        text.text = titleText;
        this.data = data;
        sectionView = reportSectionView;
    }
    private void OnClick()
    {
        if (data.Count ==0 || sectionView == null) return;

        sectionView.ShowReportOnPopup(data);
    }
}