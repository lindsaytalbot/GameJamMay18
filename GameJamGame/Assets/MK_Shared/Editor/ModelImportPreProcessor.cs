using UnityEngine;
using UnityEditor;
using System.IO;

public class ModelImportPreProcessor : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        ModelImporter modelImporter = assetImporter as ModelImporter;

        if (modelImporter == null)
            return;

        modelImporter.materialName = ModelImporterMaterialName.BasedOnMaterialName;
        modelImporter.materialSearch = ModelImporterMaterialSearch.Everywhere;
        Debug.Log("Imported " + modelImporter.assetPath + " and rectified settings");
    }

    void OnPostprocessModel(GameObject go)
    {
        //Delete created FBM folders
        CleanFBMFolders.CleanDirectory(Path.GetDirectoryName(assetImporter.assetPath));
    }
}
