using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Controllers.Project
{
    public class SpeicalReportSectionView : IProjectSectionView
    {
        [Header("Top Section")]
        [SerializeField] private Image imageview;
        [Header("Settings")]
        [SerializeField] private Transform contentContainer;
        [SerializeField] private GameObject specialReportPrefab;
        private ProjectContext projectContext;
        public void Initialize(ProjectContext context)
        {
            projectContext = context;
  
        }

        public void OnUIExit()
        {
            throw new System.NotImplementedException();
        }

        public void ShowUI()
        {
            throw new System.NotImplementedException();
        }

        public void ValidateData(ProjectContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}