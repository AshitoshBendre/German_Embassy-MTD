using Namotion.Reflection;
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
    [SerializeField] private GallerySectionView _gallerySection;
    [SerializeField] private VideoSectionView _videoSection;
    [SerializeField] private ReportSectionView _reportSection;
    private IProjectSectionView _currentActiveView;
    private ProjectContext projectContext;

    private void Awake()
    {
        HideAllViewPanels();
    }
    public void LoadProjectContent(ProjectContext context, UIManager manager)
    {
        manager.ShowScreen(this.gameObject, true, true, true,true);
        projectContext = context;
        ValidateTabs();
        _projectTitle.text = context.Data.projectTitle;
        _projectTitle.gameObject.SetActive(true);
        HideAllViewPanels();
        // The Zeroth Index Should Always have Tabs List 
        allPanels[0].SetActive(true);
        //_aboutSection.Initialize(context);
        //SwitchToView(_aboutSection);
    }

    private void ValidateTabs()
    {
        _aboutSection.ValidateData(projectContext);
        _gallerySection.ValidateData(projectContext);
        _videoSection.ValidateData(projectContext);
        _reportSection.ValidateData(projectContext);
    }

    private void HideAllViewPanels()
    {
        foreach (var panel in allPanels)
        {
            panel.SetActive(false);
        }
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
                    SwitchToView(_gallerySection);
                    break;
                }
            case 2:
                {
                    SwitchToView(_videoSection);
                    break;
                }
            case 3:
                {
                    SwitchToView(_reportSection);
                    break;
                }
        }
    }
}
