using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using MightyKingdom;

public static class CleanEmptyDirectories
{
    [MenuItem("Tools/Mighty Kingdom/Clean Empty Directories", false, 0)]
    public static void Clean()
    {
        CleanEmptyDirectory("Assets");
    }

    private static bool CleanEmptyDirectory(string dir)
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

        bool areSubDirsEmpty = true;
        foreach (string subDir in subDirs)
        {
            areSubDirsEmpty &= CleanEmptyDirectory(subDir);
        }

        if (areSubDirsEmpty && IsEmptyDirectory(dir))
        {
            MKLog.Log("Removed: " + dir, "yellow");
            AssetDatabase.MoveAssetToTrash(dir);
            return true;
        }
        return false;
    }

    private static bool IsEmptyDirectory(string dir)
    {
        string[] files;
        try
        {
            files = Directory.GetFiles(dir);
        }
        catch
        {
            return false;
        }

        foreach (string file in files)
        {
            string name = Path.GetFileName(file);
            if (!name.EndsWith(".meta") && !name.StartsWith(".") && !name.EndsWith(".DS_Store"))
                return false;
        }
        return true;
    }
}
