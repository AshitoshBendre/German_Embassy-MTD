using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectCardUI : MonoBehaviour
{
    public static event Action<ProjectData> OnClick;

    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _titleText;

    private ProjectData _projectData;

    public void Initialize(ProjectData data)
    {
        _projectData = data;
        _titleText.text = data.projectTitle;
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => OnClick?.Invoke(_projectData));
    }
}
