using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Implementation of IDataLoader Which Utilizes the Local Streaming Assets for Data Loading
/// </summary>
public class StreamingAssetsLoader : MonoBehaviour, IDataLoader
{
    private string _basePath;

    private void Awake()
    {
        // Safe initialization tied to the Unity lifecycle
        _basePath = Application.streamingAssetsPath;
    }

    public async Task<List<PanelContext>> LoadStartupPanelsAsync()
    {
        var panels = new List<PanelContext>();

        // Fallback safety in case this is called before Awake executes
        string path = string.IsNullOrEmpty(_basePath) ? Application.streamingAssetsPath : _basePath;

        string[] panelDirectories = Directory.GetDirectories(path);

        foreach (string dir in panelDirectories)
        {
            string jsonPath = Path.Combine(dir, "paneldata.json");

            if (File.Exists(jsonPath))
            {
                string json = await File.ReadAllTextAsync(jsonPath);

                // --- DEBUG ADDED HERE ---
                if (string.IsNullOrWhiteSpace(json))
                {
                    Debug.LogError($"<color=red>[JSON ERROR]</color> paneldata.json in {dir} is empty!");
                }

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

        string path = string.IsNullOrEmpty(_basePath) ? Application.streamingAssetsPath : _basePath;
        string panelPath = Path.Combine(path, panelFolderId);

        if (!Directory.Exists(panelPath)) return projects;

        string[] projectDirectories = Directory.GetDirectories(panelPath);

        foreach (string projDir in projectDirectories)
        {
            string jsonPath = Path.Combine(projDir, "projectdata.json");

            if (File.Exists(jsonPath))
            {
                Debug.Log($"Json Path : {jsonPath}");
                string json = await File.ReadAllTextAsync(jsonPath);

                // --- DEBUG ADDED HERE ---
                Debug.Log($"<color=cyan>[JSON DEBUG]</color> Reading projectdata.json from: {projDir}");

                if (string.IsNullOrWhiteSpace(json))
                {
                    Debug.LogError($"<color=red>[JSON ERROR]</color> The JSON file in {projDir} is completely empty!");
                }
                else
                {
                    Debug.Log($"<color=yellow>[JSON RAW TEXT]</color>\n{json}");
                }
                // ------------------------

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