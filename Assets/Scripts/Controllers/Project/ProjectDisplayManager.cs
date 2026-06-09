using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProjectDisplayManager : MonoBehaviour
{
    public List<GameObject> allPanels;

    [Header("Master Display Elements")]
    [SerializeField] private TMP_Text _projectTitle;

    [Header("View References")]
    [SerializeField] private AboutSectionView _aboutSection;
    // [SerializeField] private VideoSectionView _videoSection;
    // [SerializeField] private GallerySectionView _gallerySection;

    private IProjectSectionView _currentActiveView;

    public void LoadProjectContent(ProjectContext context)
    {
        _projectTitle.text = context.Data.projectTitle;

        _aboutSection.Initialize(context);
        SwitchToView(_aboutSection);
    }

    public void SwitchToView(IProjectSectionView newView)
    {
        foreach(var panel in allPanels)
        {
            panel.SetActive(false);
        }
        _currentActiveView = newView;
        _currentActiveView.ShowUI();
    }
}
