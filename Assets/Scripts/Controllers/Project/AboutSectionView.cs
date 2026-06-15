using Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AboutSectionView : MonoBehaviour, IProjectSectionView
{
    [Header("UI References")]
    [SerializeField] private Image _aboutImage; // Replaced textContainer/prefab with a direct Image reference
    [SerializeField] private GameObject TabButton;
    [SerializeField] private List<GameObject> objectsToShow;
    [SerializeField] private Button backButton;

    [Header("Settings")]
    public bool canValidate = true;

    public void Initialize(ProjectContext context)
    {
        AboutTabData aboutTabData = context.Data.aboutTabData;

        // Load the image if the URL exists
        if (aboutTabData != null && !string.IsNullOrWhiteSpace(aboutTabData.imageURL))
        {
            if (_aboutImage != null)
            {
                string fullFolderPath = $"{context.PanelFolderId}/{context.ProjectFolderId}";

                // Using your previously commented-out helper to load the image
                ImageHelper.LoadAndApplyImageAsync(fullFolderPath, aboutTabData.imageURL, _aboutImage);
                Debug.Log($"[About Builder] Loading image from: {aboutTabData.imageURL}");
            }
            else
            {
                Debug.LogWarning("[About Builder] _aboutImage UI reference is not assigned in the Inspector!");
            }
        }
        else
        {
            Debug.LogWarning("[About Builder] Image URL is missing or empty.");
        }

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }
    }

    public void OnUIExit()
    {
        if (backButton != null)
        {
            backButton.gameObject.SetActive(true);
        }
    }

    public void ShowUI()
    {
        foreach (GameObject showpanel in objectsToShow)
        {
            showpanel.SetActive(true);
        }
    }

    public void ValidateData(ProjectContext projectContext)
    {
        if (canValidate)
        {
            bool isValid = IsAboutDataValid(projectContext);

            if (!isValid)
            {
                Debug.LogWarning($"[About Validation] About tab disabled for project '{projectContext.ProjectFolderId}' due to missing image.");
            }

            if (TabButton != null)
            {
                TabButton.SetActive(isValid);
            }
        }
    }

    private bool IsAboutDataValid(ProjectContext projectContext)
    {
        AboutTabData aboutTabData = projectContext.Data.aboutTabData;

        if (aboutTabData == null)
        {
            Debug.LogWarning("[About Validation] AboutTabData is NULL.");
            return false;
        }

        // The validation now strictly checks if we have a valid string for the image URL
        if (string.IsNullOrWhiteSpace(aboutTabData.imageURL))
        {
            Debug.LogWarning("[About Validation] Image URL is empty. Tab will be hidden.");
            return false;
        }

        return true;
    }
}