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

    [SerializeField] private GameObject TabButton;
    [SerializeField] private List<GameObject> objectsToShow;
    public void Initialize(ProjectContext context)
    {
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

        if (textContainer == null)
        {
            Debug.LogWarning("[About Builder] Text Container is NULL.");
            return;
        }

        foreach (Transform child in textContainer)
        {
            Destroy(child.gameObject);
        }

        AboutData aboutData = context.Data.aboutTabData?.aboutDatas;

        if (aboutData?.textData == null)
        {
            Debug.LogWarning("[About Builder] textData is NULL.");
            return;
        }
        int createdCount = 0;
        int skippedCount = 0;


        foreach (string text in aboutData.textData)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                skippedCount++;
                Debug.LogWarning("[About Builder] Skipping empty text entry.");
                continue;
            }

            AboutDataUI dataUI = Instantiate(_aboutDataPrefab, textContainer);
            dataUI.Initialize(text);

            createdCount++;
        }

        Debug.Log(
            $"[About Builder] Created {createdCount} text blocks. Skipped {skippedCount} invalid entries.");

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

    public void ValidateData(ProjectContext projectContext)
    {
        bool isValid = IsAboutDataValid(projectContext);

        if (!isValid)
        {
            Debug.LogWarning(
                $"[About Validation] About tab disabled for project '{projectContext.ProjectFolderId}'.");
        }

        TabButton.SetActive(isValid);
    }
    private bool IsAboutDataValid(ProjectContext projectContext)
    {
        AboutTabData aboutTabData = projectContext.Data.aboutTabData;

        if (aboutTabData == null)
        {
            Debug.LogWarning("[About Validation] AboutTabData is NULL.");
            return false;
        }

        if (aboutTabData.aboutDatas == null)
        {
            Debug.LogWarning("[About Validation] AboutData is NULL.");
            return false;
        }

        if (aboutTabData.aboutDatas.textData == null)
        {
            Debug.LogWarning("[About Validation] textData list is NULL.");
            return false;
        }

        foreach (string text in aboutTabData.aboutDatas.textData)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                return true;
            }
        }

        Debug.LogWarning("[About Validation] No valid text entries found.");
        return false;
    }
}