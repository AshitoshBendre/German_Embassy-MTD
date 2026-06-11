using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectCardUI : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _titleText;

    private ProjectContext _projectContext;

    public void Initialize(ProjectContext context, ProjectDisplayManager projectDisplayManager, UIManager manager)
    {
        _projectContext = context;
        _titleText.text = context.Data.projectTitle;

        _button.onClick.RemoveAllListeners();

        // Pass the full context upward when clicked so the manager knows the folder paths for images
        _button.onClick.AddListener(() =>
        {
            projectDisplayManager?.LoadProjectContent(_projectContext,manager);

        });
    }
}