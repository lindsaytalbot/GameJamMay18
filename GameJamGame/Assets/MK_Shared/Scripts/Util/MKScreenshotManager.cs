using System;
using UnityEngine;
#if UNITY_EDITOR
namespace MightyKingdom
{
    /// <summary>
    /// Used to take development screen shots in editor
    /// </summary>
    public class MKScreenshotManager : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            new GameObject("MKScreenshotManager", typeof(MKScreenshotManager)).hideFlags |= HideFlags.HideAndDontSave;
        }

        protected void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) TakeScreenShot(1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) TakeScreenShot(2);
            if (Input.GetKeyDown(KeyCode.Alpha3)) TakeScreenShot(3);
            if (Input.GetKeyDown(KeyCode.Alpha4)) TakeScreenShot(4);
        }

        /// <summary>
        /// Takes a screen shot and saves it to disk with the date it was taken
        /// </summary>
        /// <param name="superSamples"></param>
        protected void TakeScreenShot(int superSamples)
        {
            string fileName = "/../"+ DateTime.Now.ToString("yyyy-MM-dd [HH.mm.ss]x" + superSamples) + ".png";
            Debug.Log("Capture screenshot " + fileName);
            ScreenCapture.CaptureScreenshot(Application.dataPath + fileName, superSamples);
        }
    }
}
#endif