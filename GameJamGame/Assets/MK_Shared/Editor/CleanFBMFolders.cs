using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using MightyKingdom;

public static class CleanFBMFolders
{
    [MenuItem("Tools/Mighty Kingdom/Clean FBM Folders", false, 0)]
    public static void Clean()
    {
        CleanDirectory("Assets");
    }

    public static bool CleanDirectory(string dir)
    {
        string[] subDirs;
        try
        {
            subDirs = Directory.GetDirectories(dir);
        }
        catch
        {
            return false;
        }

        foreach (string subDir in subDirs)
        {

            CleanDirectory(subDir);
        }

        if (IsFBMFolder(dir))
        {
            MKLog.Log("Removed: " + dir, "yellow");
            AssetDatabase.MoveAssetToTrash(dir);
            return true;
        }
        return false;
    }

    private static bool IsFBMFolder(string dir)
    {
        return (dir.Contains(".fbm"));
    }
}
