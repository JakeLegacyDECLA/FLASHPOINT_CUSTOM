using UnityEditor;
using UnityEngine;
using System.IO;

public class FixMaterialLocation
{
    [MenuItem("Tools/Fix Material Location (Embedded)")]
    static void FixAll()
    {
        string[] guids = AssetDatabase.FindAssets("t:Model");
        int changed = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;

            if (importer != null && importer.materialLocation == ModelImporterMaterialLocation.External)
            {
                importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
                importer.SaveAndReimport();
                changed++;
            }
        }

        Debug.Log($"Listo. Se corrigieron {changed} modelos con Material Location External.");
    }
}