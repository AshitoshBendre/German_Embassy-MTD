using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalListBuilder : MonoBehaviour
{
    [SerializeField] private Transform _contentContainer;
    [SerializeField] private ProjectCardUI _projectCardPrefab;

    private IDataLoader _loader;

    private void Awake()
    {
        _loader = new StreamingAssetsLoader();

        PanelButtonUI.OnClick += BuildProjectList;
    }
    private void OnDestroy()
    {
        PanelButtonUI.OnClick -= BuildProjectList;
    }
    public async void BuildProjectList(string folderID)
    {
        // Clearing any old Content
        foreach(Transform child in _contentContainer)
        {
            Destroy(child.gameObject);
        }

        List<ProjectData> projects = await _loader.LoadProjectsForPanelAsync(folderID);

        foreach(ProjectData project in projects)
        {
            ProjectCardUI card = Instantiate(_projectCardPrefab, _contentContainer);
            card.Initialize(project);
        }
    }
}
