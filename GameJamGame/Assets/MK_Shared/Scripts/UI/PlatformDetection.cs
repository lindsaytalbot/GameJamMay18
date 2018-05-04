using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlatformDetection : MonoBehaviour
{
    public static bool IsTVInput { get; protected set; }
    public static bool IsIPhoneX { get; protected set; }

    private const int IphonexWidth = 1125;
    private const int IphonexHeight = 2436;

    [SerializeField]
    private EventSystem standardEventSystem;

    [SerializeField]
    private EventSystem tvOSEventSystem;

    protected void Awake()
    {
        UpdateEventSystem();
    }

    public void UpdateEventSystem()
    {
#if UNITY_TVOS
		standardEventSystem.gameObject.SetActive(false);
		tvOSEventSystem.gameObject.SetActive(true);
		EventSystem.current = tvOSEventSystem;
		IsTVInput = true;
#else
        standardEventSystem.gameObject.SetActive(true);
        tvOSEventSystem.gameObject.SetActive(false);
        EventSystem.current = standardEventSystem;

#if !UNITY_EDITOR //Allow editor testing for TV input
        IsTVInput = SystemInfo.deviceModel.ToLower().Contains("aft"); //amazon fire TV check
#endif

        IsIPhoneX = Application.platform == RuntimePlatform.IPhonePlayer &&
            ((Screen.width == IphonexWidth && Screen.height == IphonexHeight) ||  //iPhoneX portrait
            (Screen.width == IphonexHeight && Screen.height == IphonexWidth)); //iPhoneX widescreen
#endif
    }
}
