using System.IO;
using UnityEditor;
using UnityEngine;

public class JsonTemplateCreator : EditorWindow
{
    private string projectName = "New Project";
    private static string targetPath;

    // --------------------------------------------------
    // PANEL JSON
    // --------------------------------------------------

    [MenuItem("Tools/Templates/Panel Data JSON")]
    public static void CreatePanelJson()
    {
        CreateJsonFile("paneldata.json", new PanelData
        {
            titleText = "",
            imageURL = ""
        });
    }

    // --------------------------------------------------
    // PROJECT JSON
    // --------------------------------------------------

    [MenuItem("Tools/Templates/Project Data JSON")]
    public static void OpenProjectWindow()
    {
        targetPath = GetSelectedFolderPath();
        var window = GetWindow<JsonTemplateCreator>(true, "Create Project");
        window.minSize = new Vector2(350, 100);
        window.maxSize = new Vector2(350, 100);
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        EditorGUILayout.LabelField("Project Name", EditorStyles.boldLabel);

        projectName = EditorGUILayout.TextField(projectName);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Create Project"))
        {
            CreateProject(projectName);
            Close();
        }
    }

    private static void CreateProject(string projectName)
    {
        string projectFolder = Path.Combine(targetPath, projectName);

        // Ensure unique folder name
        int counter = 1;
        string originalFolder = projectFolder;

        while (Directory.Exists(projectFolder))
        {
            projectFolder = $"{originalFolder}_{counter}";
            counter++;
        }

        Directory.CreateDirectory(projectFolder);

        Directory.CreateDirectory(Path.Combine(projectFolder, "Gallery"));
        Directory.CreateDirectory(Path.Combine(projectFolder, "Videos"));
        Directory.CreateDirectory(Path.Combine(projectFolder, "Reports"));
        Directory.CreateDirectory(Path.Combine(projectFolder, "Dashboard"));

        ProjectData data = new ProjectData
        {
            projectTitle = projectName,

            aboutTabData = new AboutTabData
            {
                imageURL = "",
                aboutDatas = new()
                {
                    
                }
                /*factsTitle = "",
                factsDatas = new()
            {
                new FactsData()
            }*/
            },

            videosTabData = new VideosTabData
            {
                videoDatas = new()
            {
                new VideoData()
            }
            },

            galleryTabData = new GalleryTabData
            {
                galleryDatas = new()
            {
                new GalleryData()
            }
            },

            dashboardTabData = new DashboardTabData(),

            reportsTabData = new ReportsTabData
            {
                reportDatas = new()
            {
                new ReportData()
            }
            }
        };

        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(
            Path.Combine(projectFolder, "projectdata.json"),
            json);

        AssetDatabase.Refresh();

        Object folderAsset =
            AssetDatabase.LoadAssetAtPath<Object>(
                projectFolder.Replace("\\", "/"));

        EditorGUIUtility.PingObject(folderAsset);
        Selection.activeObject = folderAsset;
    }
    // --------------------------------------------------
    // COMMON JSON CREATION
    // --------------------------------------------------

    private static void CreateJsonFile<T>(
        string defaultFileName,
        T data)
    {
        string selectedPath = GetSelectedFolderPath();

        string filePath = AssetDatabase.GenerateUniqueAssetPath(
            Path.Combine(selectedPath, defaultFileName));

        string absolutePath = Path.GetFullPath(filePath);

        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(absolutePath, json);

        AssetDatabase.Refresh();

        Object asset =
            AssetDatabase.LoadAssetAtPath<Object>(filePath);

        EditorGUIUtility.PingObject(asset);
        Selection.activeObject = asset;
    }

    private static string GetSelectedFolderPath()
    {
        string path = "Assets";

        Object selected = Selection.activeObject;

        if (selected != null)
        {
            path = AssetDatabase.GetAssetPath(selected);

            if (!Directory.Exists(path))
            {
                path = Path.GetDirectoryName(path);
            }
        }

        return path;
    }
}