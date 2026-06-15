using Helpers; // Required to use your ImageHelper
using UnityEngine;
using UnityEngine.UI; // Required for the Image component

public class ProjectCardUI : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _cardImage; // Replaced _titleText

    private ProjectContext _projectContext;

    public void Initialize(ProjectContext context, ProjectDisplayManager projectDisplayManager, UIManager manager)
    {
        _projectContext = context;

        // Load the thumbnail image asynchronously
        if (_cardImage != null && !string.IsNullOrWhiteSpace(context.Data.imageURL))
        {
            string fullFolderPath = $"{context.PanelFolderId}/{context.ProjectFolderId}";
            ImageHelper.LoadAndApplyImageAsync(fullFolderPath, context.Data.imageURL, _cardImage);
        }
        else if (_cardImage == null)
        {
            Debug.LogWarning($"[ProjectCardUI] Image reference is missing in the Inspector for {context.ProjectFolderId}!");
        }

        _button.onClick.RemoveAllListeners();

        // Pass the full context upward when clicked so the manager knows the folder paths
        _button.onClick.AddListener(() =>
        {
            projectDisplayManager?.LoadProjectContent(_projectContext, manager);
        });
    }
}