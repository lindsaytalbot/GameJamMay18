using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MightyKingdom;

public class PermissionManager : MonoBehaviour
{
    //Lazily instantiate on first reference
    private static PermissionManager _instance;

    public const string WRITE_EXTERNAL_STORAGE = "WRITE_EXTERNAL_STORAGE";

    static List<PermissionCallback> callbacks = new List<PermissionCallback>();

    /// <summary>
    /// Adds and sets all the required game components
    /// Must be created before calling any functions
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init()
    {
        if (_instance != null)
            return;

        //Create GameObject and add components
        GameObject go = new GameObject();
        go.name = "MKPermissionManager";
        _instance = go.AddComponent<PermissionManager>();
        DontDestroyOnLoad(go);
    }

    public static void TryRequestPermission(string permission, Action onSuccess, Action onDenied)
    {
        if(HasPermission(permission))
        {
            onSuccess();
            return;
        }

        callbacks.Add(new PermissionCallback() { permission = permission, permittedCallback = onSuccess, deniedCallback = onDenied });
        RequestPermission(permission);

#if UNITY_EDITOR
        CheckCallbacks();
#endif
    }

    public static void CheckCallbacks()
    {
        foreach (PermissionCallback callback in callbacks)
        {
            bool hasPermission = HasPermission(callback.permission);
            if (hasPermission && callback.permittedCallback != null)
            {
                callback.permittedCallback();
            }
            else if (!hasPermission && callback.deniedCallback != null)
            {
                callback.deniedCallback();
            }
        }

        callbacks.Clear();
    }

    //Hack, but it works. Unity loses and regains focus when permission dialogs appear
    protected void OnApplicationPause(bool paused)
    {
        if (paused)
            return;

        CheckCallbacks();
    }

#if !UNITY_ANDROID || UNITY_EDITOR
    static bool hasRequested = true;
    static bool hasPermission = false;

    public static void RequestPermission(string permission)
    {
        hasPermission = true;
    }

    public static bool HasPermission(string permission)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            return true;
        return hasPermission;
    }

    public static bool HasRequestedPermission(string permission)
    {
        bool result = hasRequested;
        hasRequested = true;
        return result;
    }
#else
    public static void RequestPermission(string permission)
    {
        using (AndroidJavaClass bridge = new AndroidJavaClass("com.mightykingdom.permissions.PermissionBridge"))
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            string res = bridge.CallStatic<string>("RequestPermission", jo, permission);
        }
    }

    public static bool HasPermission(string permission)
    {
        bool result = false;

        using (AndroidJavaClass bridge = new AndroidJavaClass("com.mightykingdom.permissions.PermissionBridge"))
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            result = bridge.CallStatic<bool>("CheckPermission", jo, permission);
        }

        return result;
    }

	public static bool HasRequestedPermission(string permission)
    {
        bool result = false;

        using (AndroidJavaClass bridge = new AndroidJavaClass("com.mightykingdom.permissions.PermissionBridge"))
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            result = bridge.CallStatic<bool>("HasRequestedPermission", jo, permission);
        }

        return result;
    }
#endif

    protected struct PermissionCallback
    {
        public string permission;
        public Action permittedCallback;
        public Action deniedCallback;        
    }
}