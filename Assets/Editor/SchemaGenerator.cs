using System.IO;
using NJsonSchema;
using UnityEditor;
using UnityEngine;

public static class SchemaGenerator
{
    [MenuItem("Tools/Generate JSON Schemas")]
    public static void GenerateSchemas()
    {
        string schemaFolder = Path.Combine(Application.streamingAssetsPath, "Schemas");

        // Safely check and create the directory
        if (!Directory.Exists(schemaFolder))
        {
            Directory.CreateDirectory(schemaFolder);
        }

        // ProjectData schema will automatically nest and include the definitions for AboutTabData, VideosTabData, etc.
        GenerateSchema<ProjectData>(schemaFolder);
        GenerateSchema<PanelData>(schemaFolder);

        AssetDatabase.Refresh();

        Debug.Log($"Schemas generated successfully at: {schemaFolder}");
    }

    private static void GenerateSchema<T>(string outputFolder)
    {
        JsonSchema schema = JsonSchema.FromType<T>();
        string filePath = Path.Combine(outputFolder, $"{typeof(T).Name}.schema.json");

        File.WriteAllText(filePath, schema.ToJson());
        Debug.Log($"Generated schema: {filePath}");
    }
}