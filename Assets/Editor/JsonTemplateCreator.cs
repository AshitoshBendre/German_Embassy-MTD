using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class JsonTemplateCreator : EditorWindow
{
    private string projectName = "New Project";

    // UI Toggles
    private bool includeAbout = true;
    private bool includeVideos = true;
    private bool includeGallery = true;
    private bool includeReports = true;
    private bool fillDummyData = false;

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
        window.minSize = new Vector2(350, 260);
        window.maxSize = new Vector2(350, 260);
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        EditorGUILayout.LabelField("Project Setup", EditorStyles.boldLabel);
        projectName = EditorGUILayout.TextField("Project Name", projectName);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Include Tabs (Uncheck to make null)", EditorStyles.boldLabel);
        includeAbout = EditorGUILayout.Toggle("Include About", includeAbout);
        includeVideos = EditorGUILayout.Toggle("Include Videos", includeVideos);
        includeGallery = EditorGUILayout.Toggle("Include Gallery", includeGallery);
        includeReports = EditorGUILayout.Toggle("Include Reports", includeReports);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Data Population", EditorStyles.boldLabel);
        fillDummyData = EditorGUILayout.Toggle("Fill Dummy Data", fillDummyData);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Create Project"))
        {
            CreateProject(projectName, includeAbout, includeVideos, includeGallery, includeReports, fillDummyData);
            Close();
        }
    }

    private static void CreateProject(string projectName, bool incAbout, bool incVideos, bool incGallery, bool incReports, bool isDummy)
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

        // --- COPY PHYSICAL DUMMY FILES ---
        if (isDummy)
        {
            CopyTemplateFiles(projectFolder);
        }

        // --- DUMMY DATA GENERATION LOGIC ---

        AboutTabData aboutData = null;
        if (incAbout)
        {
            aboutData = new AboutTabData
            {
                imageURL = isDummy ? "Gallery/dummy_image.jpg" : "",
                aboutDatas = new AboutData
                {
                    textData = isDummy ? new List<string>
                    {
                        "The projectdataeeeee initiative represents a paradigm shift in synergistic, quantum-driven computing.",
                        "Furthermore, the core flux capacitor output has been stabilized at exactly 1.21 Gigawatts."
                    } : new List<string> { "" }
                }
            };
        }

        VideosTabData videosData = null;
        if (incVideos)
        {
            var vList = new List<VideoData>();
            if (isDummy)
            {
                for (int i = 1; i <= 4; i++)
                {
                    vList.Add(new VideoData { videoURL = "Videos/dummy_video.mp4", titleText = $"Project Video Phase {i}" });
                }
            }
            else
            {
                vList.Add(new VideoData { videoURL = "", titleText = "" });
            }
            videosData = new VideosTabData { idleTimeout = 2f, videoDatas = vList };
        }

        GalleryTabData galleryData = null;
        if (incGallery)
        {
            var gList = new List<GalleryData>();
            if (isDummy)
            {
                for (int i = 1; i <= 6; i++)
                {
                    gList.Add(new GalleryData { imageURL = "Gallery/dummy_image.jpg", titleText = $"Architecture Snapshot {i}" });
                }
            }
            else
            {
                gList.Add(new GalleryData { imageURL = "", titleText = "" });
            }
            galleryData = new GalleryTabData { galleryDatas = gList };
        }

        ReportsTabData reportsData = null;
        if (incReports)
        {
            var rList = new List<ReportData>();
            if (isDummy)
            {
                rList.Add(new ReportData
                {
                    titleText = "Q3 Idle Loop Automation Analysis",
                    textData = new List<string> { "Diagnostics indicate a 400% increase in virtual ingredient processing.", "Supply-chain mechanics are operating within expected parameters." }
                });
                rList.Add(new ReportData
                {
                    titleText = "Peripheral Latency Metrics",
                    textData = new List<string> { "Extensive stress testing reveals zero drop in polling rates.", "Sub-millisecond response times maintained during 42 Hz quantum wobble." }
                });
            }
            else
            {
                rList.Add(new ReportData { titleText = "", textData = new List<string> { "" } });
            }
            reportsData = new ReportsTabData { reportDatas = rList };
        }

        // -----------------------------------

        ProjectData data = new ProjectData
        {
            projectTitle = projectName,
            aboutTabData = aboutData,
            videosTabData = videosData,
            galleryTabData = galleryData,
            dashboardTabData = new DashboardTabData { imageVideosURLs = new List<string>() },
            reportsTabData = reportsData
        };

        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(Path.Combine(projectFolder, "projectdata.json"), json);

        AssetDatabase.Refresh();

        Object folderAsset = AssetDatabase.LoadAssetAtPath<Object>(projectFolder.Replace("\\", "/"));

        EditorGUIUtility.PingObject(folderAsset);
        Selection.activeObject = folderAsset;
    }

    // --------------------------------------------------
    // FILE COPY HELPERS
    // --------------------------------------------------

    private static void CopyTemplateFiles(string projectFolder)
    {
        // 1. Find the directory where this script resides
        string scriptDirectory = "Assets";
        string[] guids = AssetDatabase.FindAssets("JsonTemplateCreator t:MonoScript");

        if (guids.Length > 0)
        {
            string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            scriptDirectory = Path.GetDirectoryName(scriptPath);
        }

        // 2. Define source paths (expects files to be right next to the script)
        string sourceImagePath = Path.Combine(scriptDirectory, "template_image.jpg");
        string sourceVideoPath = Path.Combine(scriptDirectory, "template_video.mp4");

        // 3. Define destination paths
        string destImagePath = Path.Combine(projectFolder, "Gallery", "dummy_image.jpg");
        string destVideoPath = Path.Combine(projectFolder, "Videos", "dummy_video.mp4");

        // 4. Copy the files if they exist
        if (File.Exists(sourceImagePath))
        {
            File.Copy(sourceImagePath, destImagePath, true);
        }
        else
        {
            Debug.LogWarning($"[JsonTemplateCreator] Could not find 'template_image.jpg' in {scriptDirectory}. Please place it next to the script.");
        }

        if (File.Exists(sourceVideoPath))
        {
            File.Copy(sourceVideoPath, destVideoPath, true);
        }
        else
        {
            Debug.LogWarning($"[JsonTemplateCreator] Could not find 'template_video.mp4' in {scriptDirectory}. Please place it next to the script.");
        }
    }

    // --------------------------------------------------
    // COMMON JSON CREATION
    // --------------------------------------------------

    private static void CreateJsonFile<T>(string defaultFileName, T data)
    {
        string selectedPath = GetSelectedFolderPath();
        string filePath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(selectedPath, defaultFileName));
        string absolutePath = Path.GetFullPath(filePath);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(absolutePath, json);

        AssetDatabase.Refresh();

        Object asset = AssetDatabase.LoadAssetAtPath<Object>(filePath);
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

// Dummy PanelData class assuming it exists somewhere in your project to prevent compiler errors from CreatePanelJson()
public class PanelData
{
    public string titleText;
    public string imageURL;
}