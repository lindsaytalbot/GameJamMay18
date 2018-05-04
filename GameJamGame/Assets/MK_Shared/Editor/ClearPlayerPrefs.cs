using UnityEngine;
using UnityEditor;
using MightyKingdom;

public static class ClearPlayerPrefs
{
    [MenuItem("Tools/Mighty Kingdom/Clear PlayerPrefs", false, 0)]
    public static void ClearPrefs()
    {
        MKPlayerPrefs.ClearPrefs();
        MKLog.Log("Player Prefs Cleared", "yellow");
    }
}