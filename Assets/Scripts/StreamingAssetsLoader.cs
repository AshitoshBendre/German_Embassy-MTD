
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Implementation of IDataLoader Which Utilizes the Local Streaming Assets for Data Loading
/// </summary>
public class StreamingAssetsLoader : IDataLoader
{
    private readonly string _basePath = Application.streamingAssetsPath;
    
    public async Task<List<PanelContext>> LoadStartupPanelsAsync()
    {
        var panels = new List<PanelContext>();

        string[] panelDirectories = Directory.GetDirectories(_basePath);

        foreach (string dir in panelDirectories)
        {
            string jsonPath = Path.Combine(dir, "paneldata.json");

            if (File.Exists(jsonPath))
            {
                string json = await File.ReadAllTextAsync(jsonPath);
                PanelData data = JsonUtility.FromJson<PanelData>(json);

                panels.Add(new PanelContext
                {
                    FolderId = new DirectoryInfo(dir).Name,
                    Data = data
                });
            }
        }
        return panels;
    }

    public async Task<List<ProjectContext>> LoadProjectsForPanelAsync(string panelFolderId)
    {
        var projects = new List<ProjectContext>();
        string panelPath = Path.Combine(Application.streamingAssetsPath, panelFolderId);

        if (!Directory.Exists(panelPath)) return projects;

        string[] projectDirectories = Directory.GetDirectories(panelPath);

        foreach (string projDir in projectDirectories)
        {
            string jsonPath = Path.Combine(projDir, "projectdata.json");

            if (File.Exists(jsonPath))
            {
                string json = await File.ReadAllTextAsync(jsonPath);
                ProjectData data = JsonUtility.FromJson<ProjectData>(json);

                // Package the data AND the routing paths into the Context
                projects.Add(new ProjectContext
                {
                    PanelFolderId = panelFolderId,
                    ProjectFolderId = new DirectoryInfo(projDir).Name, // Extracts the folder name (e.g., "project1")
                    Data = data
                });
            }
        }
        return projects;
    }

}