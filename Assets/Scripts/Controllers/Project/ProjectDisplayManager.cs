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
    [SerializeField] private GallerySectionView _gallerSection;
    // [SerializeField] private VideoSectionView _videoSection;
    // [SerializeField] private GallerySectionView _gallerySection;

    private IProjectSectionView _currentActiveView;
    private ProjectContext projectContext;
    public void LoadProjectContent(ProjectContext context)
    {
        projectContext = context;
        _projectTitle.text = context.Data.projectTitle;

        _aboutSection.Initialize(context);
        SwitchToView(_aboutSection);
    }

    private void SwitchToView(IProjectSectionView newView)
    {
        foreach(var panel in allPanels)
        {
            panel.SetActive(false);
        }
        _currentActiveView = newView;
        _currentActiveView.Initialize(projectContext);
        _currentActiveView.ShowUI();
    }


    public void SwitchProjectSectionView(int index)
    {
        switch (index)
        {
            case 0:
                {
                    SwitchToView(_aboutSection);
                    break;
                }
            case 1:
                {
                    SwitchToView(_gallerSection);
                    break;
                }
        }
    }
}
