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
        // Requires PanelData to exist elsewhere in your project
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
        EditorGUILayout.LabelField("Include Tabs (Uncheck to generate empty arrays)", EditorStyles.boldLabel);
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

        // --- SAFE DATA GENERATION LOGIC ---
        // Initialize everything with empty lists to prevent JsonUtility from crashing on nulls

        AboutTabData aboutData = new AboutTabData { aboutDatas = new AboutData { textData = new List<string>() } };
        if (incAbout)
        {
            aboutData.imageURL = isDummy ? "Gallery/dummy_image.jpg" : "";
            if (isDummy)
            {
                aboutData.aboutDatas.textData = new List<string>
                {
                    "The projectdataeeeee initiative represents a paradigm shift in synergistic, quantum-driven computing.",
                    "Furthermore, the core flux capacitor output has been stabilized at exactly 1.21 Gigawatts."
                };
            }
            else
            {
                aboutData.aboutDatas.textData = new List<string> { "" };
            }
        }

        VideosTabData videosData = new VideosTabData { videoDatas = new List<VideoData>() };
        if (incVideos)
        {
            videosData.idleTimeout = 2f;
            if (isDummy)
            {
                for (int i = 1; i <= 4; i++)
                {
                    videosData.videoDatas.Add(new VideoData
                    {
                        videoURL = "Videos/dummy_video.mp4",
                        titleText = $"Project Video Phase {i}",
                        thumbnailURL = "Gallery/dummy_image.jpg" // Added thumbnail support
                    });
                }
            }
            else
            {
                videosData.videoDatas.Add(new VideoData
                {
                    videoURL = "",
                    titleText = "",
                    thumbnailURL = "" // Added thumbnail support
                });
            }
        }

        GalleryTabData galleryData = new GalleryTabData { galleryDatas = new List<GalleryData>() };
        if (incGallery)
        {
            if (isDummy)
            {
                for (int i = 1; i <= 6; i++)
                {
                    galleryData.galleryDatas.Add(new GalleryData { imageURL = "Gallery/dummy_image.jpg", titleText = $"Architecture Snapshot {i}" });
                }
            }
            else
            {
                galleryData.galleryDatas.Add(new GalleryData { imageURL = "", titleText = "" });
            }
        }

        ReportsTabData reportsData = new ReportsTabData { reportDatas = new List<ReportData>() };
        if (incReports)
        {
            if (isDummy)
            {
                reportsData.reportDatas.Add(new ReportData
                {
                    pdfURL = "Reports/SamplePDF.pdf",
                    titleText = "Q3 Idle Loop Automation Analysis"
                });
                reportsData.reportDatas.Add(new ReportData
                {
                    pdfURL = "Reports/SamplePDF.pdf",
                    titleText = "Peripheral Latency Metrics"
                });
            }
            else
            {
                reportsData.reportDatas.Add(new ReportData { pdfURL = "", titleText = "" });
            }
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
        string scriptDirectory = "Assets";
        string[] guids = AssetDatabase.FindAssets("JsonTemplateCreator t:Script");

        if (guids.Length > 0)
        {
            string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            scriptDirectory = Path.GetDirectoryName(scriptPath).Replace("\\", "/");
        }

        string sourceImagePath = $"{scriptDirectory}/template_image.jpg";
        string sourceVideoPath = $"{scriptDirectory}/template_video.mp4";

        string safeProjectFolder = projectFolder.Replace("\\", "/");
        string destImagePath = $"{safeProjectFolder}/Gallery/dummy_image.jpg";
        string destVideoPath = $"{safeProjectFolder}/Videos/dummy_video.mp4";

        if (File.Exists(Path.GetFullPath(sourceImagePath)))
        {
            AssetDatabase.CopyAsset(sourceImagePath, destImagePath);
        }
        else
        {
            Debug.LogWarning($"[JsonTemplateCreator] Could not find '{sourceImagePath}'.");
        }

        if (File.Exists(Path.GetFullPath(sourceVideoPath)))
        {
            AssetDatabase.CopyAsset(sourceVideoPath, destVideoPath);
        }
        else
        {
            Debug.LogWarning($"[JsonTemplateCreator] Could not find '{sourceVideoPath}'.");
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