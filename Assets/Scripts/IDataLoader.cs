using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Abstraction layer for data fetching
/// </summary>
public interface IDataLoader
{
    Task<List<PanelContext>> LoadStartupPanelsAsync();
    Task<List<ProjectData>> LoadProjectsForPanelAsync(string panelFolderId);
}
