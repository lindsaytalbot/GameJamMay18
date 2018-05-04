using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIDeviceScaling : MonoBehaviour {

    private enum TestTargets
    {
        Mobile, 
        TV,
        iPhoneX
    }

#if UNITY_EDITOR
    [SerializeField]
    private TestTargets testTarget;
#endif

    private RectTransform rect;

    [Header("TV Border")]

    [SerializeField]
    private float tvBufferSize = 60; //based on 1080*1920

    [Header("iPhoneX Padding")]

    [SerializeField]
    private float iPhoneXNotchSize = 71; //based on 1080*1920

    // Use this for initialization
    void Start()
    {
        rect = GetComponent<RectTransform>();

#if UNITY_EDITOR
        switch (testTarget)
        {
            case TestTargets.Mobile:
                AdjustForMobile();
                break;
            case TestTargets.TV:
                AdjustForTV();
                break;
            case TestTargets.iPhoneX:
                AdjustForIphoneX();
                OrientationChecker.OnOrientationChange += UpdateOrientation;
                break;
            default:
                break;
        }

        return;
#endif

        if (PlatformDetection.IsTVInput)
        {
            AdjustForTV();
        }
        else if (PlatformDetection.IsIPhoneX)
        {
            AdjustForIphoneX();
            OrientationChecker.OnOrientationChange += UpdateOrientation;
        }
        else
        {
            AdjustForMobile();
        }
    }

    [ContextMenu("Adjust for TV")]
    void AdjustForTV()
    {
#if UNITY_EDITOR
        rect = GetComponent<RectTransform>();
#endif

        rect.offsetMax = new Vector2(-tvBufferSize, -tvBufferSize);
        rect.offsetMin = new Vector2(tvBufferSize, tvBufferSize);
    }

    [ContextMenu("Adjust for Mobile")]
    void AdjustForMobile()
    {
#if UNITY_EDITOR
        rect = GetComponent<RectTransform>();
#endif

        rect.offsetMax = Vector2.zero;
        rect.offsetMin = Vector2.zero;
    }

    [ContextMenu("Adjust for iPhoneX")]
    void AdjustForIphoneX()
    {
        DeviceOrientation orientation = Input.deviceOrientation;

#if UNITY_EDITOR
        rect = GetComponent<RectTransform>();

        if (rect.rect.width > rect.rect.height)
            orientation = DeviceOrientation.LandscapeLeft;
        else
            orientation = DeviceOrientation.Portrait;
#endif

        AdjustForIphoneX(orientation);
    }

    void UpdateOrientation(DeviceOrientation newOrientation)
    {
        AdjustForIphoneX();
    }

    void AdjustForIphoneX(DeviceOrientation newOrientation)
    {
        //Only care about notch in portrait
		if (newOrientation == DeviceOrientation.Portrait) 
		{
			rect.offsetMax = new Vector2 (0, -iPhoneXNotchSize);
			rect.offsetMin = new Vector2 (0, 0);
		}
		else if (newOrientation == DeviceOrientation.PortraitUpsideDown) 
		{
			rect.offsetMax = new Vector2 (0, 0);
			rect.offsetMin = new Vector2 (0, iPhoneXNotchSize);
		} 
		else //if (newOrientation == DeviceOrientation.LandscapeLeft || newOrientation == DeviceOrientation.) 
		{
			rect.offsetMax = new Vector2 (-iPhoneXNotchSize, -iPhoneXNotchSize);
			rect.offsetMin = new Vector2 (iPhoneXNotchSize, iPhoneXNotchSize); //Gap for home button on bottom
		}
    }
}
