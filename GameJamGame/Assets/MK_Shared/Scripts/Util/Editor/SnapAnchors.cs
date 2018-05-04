using UnityEditor;
using UnityEngine;

public class SnapAnchors : MonoBehaviour
{
    [MenuItem("UI/Anchor Around Object")]
    static void uGUIAnchorAroundObject()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            if (o == null)
                return;

            var r = o.GetComponent<RectTransform>();

            if (r == null || r.parent == null)
                continue;

#if UNITY_EDITOR
            Undo.RecordObject(o, "SnapAnchors");
#endif
            AnchorRect(r);
        }
    }

    static void AnchorRect(RectTransform r)
    {
        var p = r.parent.GetComponent<RectTransform>();

        var offsetMin = r.offsetMin;
        var offsetMax = r.offsetMax;
        var _anchorMin = r.anchorMin;
        var _anchorMax = r.anchorMax;

        var parent_width = p.rect.width;
        var parent_height = p.rect.height;

        var anchorMin = new Vector2(_anchorMin.x + (offsetMin.x / parent_width),
                                    _anchorMin.y + (offsetMin.y / parent_height));
        var anchorMax = new Vector2(_anchorMax.x + (offsetMax.x / parent_width),
                                    _anchorMax.y + (offsetMax.y / parent_height));

        r.anchorMin = anchorMin;
        r.anchorMax = anchorMax;

        r.offsetMin = new Vector2(0, 0);
        r.offsetMax = new Vector2(0, 0);
        r.pivot = new Vector2(0.5f, 0.5f);
    }
}