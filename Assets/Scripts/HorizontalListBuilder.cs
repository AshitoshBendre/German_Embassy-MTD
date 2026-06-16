using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalListBuilder : MonoBehaviour
{
    [SerializeField] private Transform _contentContainer;
    [SerializeField] private ProjectCardUI _projectCardPrefab;
    [SerializeField] private ProjectDisplayManager _projectDisplayManager;

    private IDataLoader _loader;

    private void Awake()
    {
        _loader = new StreamingAssetsLoader();
    }
    public async void BuildProjectList(string folderID, UIManager manager)
    {
        manager.ShowScreen(this.gameObject, true, true, true);
        /// For Testing Only
        this.gameObject.SetActive(true);

        // Clearing any old Content
        foreach (Transform child in _contentContainer)
        {
            Destroy(child.gameObject);
        }
        List<ProjectContext> projectContexts = await _loader.LoadProjectsForPanelAsync(folderID);

        foreach (ProjectContext project in projectContexts)
        {
            ProjectCardUI card = Instantiate(_projectCardPrefab, _contentContainer);
            card.Initialize(project, _projectDisplayManager, manager);
        }
    }
}
