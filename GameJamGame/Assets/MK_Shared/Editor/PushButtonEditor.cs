using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PushButtonEditor
{
    [MenuItem("GameObject/UI/PushButton")]
    private static void AddConfig()
    {
        Transform parent = null;

        if (Selection.gameObjects != null && Selection.gameObjects.Length > 0)
            parent = Selection.gameObjects[0].transform;

        //Parent must contain a canvas element
        if (parent == null || parent.GetComponentInParent<Canvas>() == null)
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();

            if (canvas != null)
            {
                parent = canvas.transform;
            }
            else
            {
                //No canvas present in the scene. Create one
                GameObject canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                parent = canvasGO.transform;
                canvas = canvasGO.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                //Create event system if none are present
                if (GameObject.FindObjectOfType<EventSystem>() == null)
                {
                    new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                }

                //TODO write canvas setup function
                CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 1;
                canvasScaler.referenceResolution = new Vector2(1080, 1920);
            }
        }

        GameObject pushButtonGO = new GameObject("PushButton", typeof(RectTransform), typeof(Touchable), typeof(PushButton));
        GameObject pushButtonAnimation = new GameObject("PushButton_Animator", typeof(RectTransform), typeof(Animator), typeof(Image));

        pushButtonGO.transform.SetParent(parent);
        pushButtonGO.transform.localScale = Vector3.one;

        //Configure pushbutton default settings
        PushButton pushButton = pushButtonGO.GetComponent<PushButton>();
        pushButton.SetStateColours();
        pushButton.buttonAnimator = pushButtonAnimation.GetComponent<Animator>();
        pushButton.buttonAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        pushButton.targetGraphic = pushButtonAnimation.GetComponent<Graphic>();

        //configure button size and position
        RectTransform pushButtonRect = pushButtonGO.GetComponent<RectTransform>();
        pushButtonRect.localPosition = Vector3.zero;
        pushButtonRect.sizeDelta = new Vector2(200, 100);

        //Make child button in same spot and size as original button.
        RectTransform rt = pushButtonAnimation.GetComponent<RectTransform>();
        rt.SetParent(pushButtonGO.transform);
        rt.transform.localScale = Vector3.one;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.localPosition = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    [MenuItem("Tools/PushButton/Replace Button With Push Button")]
    public static void ReplaceButtonWithPushButton()
    {
        List<Button> buttons = new List<Button>();
        foreach (GameObject go in Selection.gameObjects)
        {
            Button button = go.GetComponent<Button>();
            if (button != null)
                buttons.Add(button);
        }

        List<GameObject> created = new List<GameObject>();

        string names = "";

        foreach (Button button in buttons)
        {
            names += button.name + "\n";
        }

        if (buttons.Count == 0)
        {
            EditorUtility.DisplayDialog("No buttons selected", "Cannot replace with PushButtons, as no Buttons are selected. Please select the Button GameObjects you wish to replace", "Okay");
        }
        else if (EditorUtility.DisplayDialog("Replace with PushButtons?", "This with replace the select Buttons with PushButtons, and cannot be undone. Callbacks will be preserved but layouts may need to be manually corrected.\n\n The following buttons are selected:\n" + names, "Replace", "Cancel"))
        {
            foreach (Button button in buttons)
            {
                GameObject buttonGO = button.gameObject;

                //Make push button in same spot and size as original button.
                GameObject go = new GameObject();
                go.name = buttonGO.name + "_Animator";
                RectTransform rt = go.AddComponent<RectTransform>();
                rt.SetParent(button.transform);
                rt.transform.localScale = Vector3.one;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.localPosition = Vector2.zero;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                //Move all children under the scaler object
                foreach (Transform child in button.GetComponentsInChildren<Transform>(true))
                {
                    if (child.gameObject != go && child.gameObject != buttonGO)
                    {
                        child.SetParent(go.transform);
                    }
                }

                //Copy the button image to the scaler if it has one
                Image buttonImage = buttonGO.GetComponent<Image>();
                if (buttonImage != null)
                {
                    Image newImage = go.AddComponent<Image>();
                    newImage.sprite = buttonImage.sprite;
                    newImage.color = buttonImage.color;
                    newImage.raycastTarget = false;
                    newImage.material = buttonImage.material;
                    newImage.type = buttonImage.type;
                    newImage.preserveAspect = buttonImage.preserveAspect;
                    newImage.fillMethod = buttonImage.fillMethod;
                    newImage.fillOrigin = buttonImage.fillOrigin;
                    newImage.fillAmount = buttonImage.fillAmount;
                    newImage.fillCenter = buttonImage.fillCenter;
                    GameObject.DestroyImmediate(buttonImage);
                }

                //Copy the button image to the scaler if it has one
                RawImage buttonRawImage = buttonGO.GetComponent<RawImage>();
                if (buttonRawImage != null)
                {
                    RawImage newImage = go.AddComponent<RawImage>();
                    newImage.texture = buttonRawImage.texture;
                    newImage.color = buttonRawImage.color;
                    newImage.raycastTarget = false;
                    newImage.material = buttonRawImage.material;
                    newImage.uvRect = buttonRawImage.uvRect;
                    GameObject.DestroyImmediate(buttonRawImage);
                }

                //Copy on click actions
                PushButton pb = buttonGO.AddComponent<PushButton>();
                pb.onClickActions = button.onClick;

                //Button animator
                Animator animator = go.AddComponent<Animator>();
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                pb.buttonAnimator = animator;

                created.Add(buttonGO);
                GameObject.DestroyImmediate(button);
            }
        }

        Selection.objects = created.ToArray();
    }
}
