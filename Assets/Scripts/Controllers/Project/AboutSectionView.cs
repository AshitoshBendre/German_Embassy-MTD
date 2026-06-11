using Helpers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AboutSectionView : MonoBehaviour, IProjectSectionView
{
    [Header("PopupData")]
    [SerializeField] private Transform textContainer;
    [SerializeField] private AboutDataUI _aboutDataPrefab;
    //[Header("Top Section")]
    //[SerializeField] private Image _aboutCoverImage;

    //[Header("Bottom Section (Facts)")]
    //[SerializeField] private TMP_Text _factsTitleText;
    //[SerializeField] private Transform _factsContainer;
    //[SerializeField] private FactRowUI _factRowPrefab;

    [SerializeField] private List<GameObject> objectsToShow;
    public void Initialize(ProjectContext context)
    {
        AboutTabData aboutData = context.Data.aboutTabData;

        /*if (!string.IsNullOrWhiteSpace(aboutData.imageURL))
        {
            string fullFolderPath = $"{context.PanelFolderId}/{context.ProjectFolderId}";
            ImageHelper.LoadAndApplyImageAsync(fullFolderPath, aboutData.imageURL, _aboutCoverImage);
        }*/

        /*_factsTitleText.text = aboutData.factsTitle;

        foreach(Transform child in _factsContainer)
        {
            Destroy(child.gameObject);
        }*/

        /* if(aboutData.factsDatas != null)
         {
             foreach(FactsData fact in aboutData.factsDatas)
             {
                 if (fact.IsInvalid()) continue;
                 FactRowUI factRow = Instantiate(_factRowPrefab, _factsContainer);
                 factRow.Initialize(fact.leftText, fact.rightText);
             }
         }*/

        if (textContainer == null) return;
        foreach(Transform child in textContainer)
        {
            Destroy(child.gameObject);
        }
        AboutData data = context.Data.aboutTabData.aboutDatas;
        foreach (string t in data.textData)
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
    public void OnUISwitch()
    {
        throw new System.NotImplementedException();
    }

    public void ShowUI()
    {
        foreach(GameObject showpanel in objectsToShow)
        {
            showpanel.SetActive(true);
        }
    }
}