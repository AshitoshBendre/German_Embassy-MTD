using System.Collections.Generic;

public interface IProjectSectionView 
{
    void Initialize(ProjectContext context);
    void ShowUI();

    void OnUIExit();
    void ValidateData(ProjectContext context);
}
