using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides a platform for static and non-monobehaviour classes to call StartCoroutine() from
/// </summary>
public class CoroutineHelper : MonoBehaviour
{
    private static CoroutineHelper _instance;
    public static CoroutineHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject();
                _instance = go.AddComponent<CoroutineHelper>();
                go.name = "CoroutineHelper";
                DontDestroyOnLoad(go);
            }

            return _instance;
        }
    }

    public static IEnumerator WaitForRealSeconds(float time)
    {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < start + time)
        {
            yield return null;
        }
    }
}
