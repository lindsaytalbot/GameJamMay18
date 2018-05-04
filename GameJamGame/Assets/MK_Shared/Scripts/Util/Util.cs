using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MightyKingdom
{
    public static class Util
    {
        public static void SetAlpha(this Image image, float alpha)
        {
            if (image == null) return;
            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }

        public static void SetAlpha(this Text text, float alpha)
        {
            Color c = text.color;
            c.a = alpha;
            text.color = c;
        }

        public static void SetAlpha(this TextMeshProUGUI text, float alpha)
        {
            Color c = text.color;
            c.a = alpha;
            text.color = c;
        }

        public static void SetAlpha(this SpriteRenderer image, float alpha)
        {
            if (image == null) return;
            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }

        public static int GetItemIndex<T>(this IList<T> list, T searchItem, int defaultIndex = 0)
        {
            int index = 0;

            foreach (T item in list)
            {
                if (item.Equals(searchItem))
                    return index;
                index++;
            }

            Debug.LogError("Item not found!");

            return defaultIndex;
        }

        public static string ColorToHTML(Color c)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}", ToByte(c.r), ToByte(c.g), ToByte(c.b));
        }

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public static void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (null == obj)
            {
                return;
            }

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                if (null == child)
                {
                    continue;
                }
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        public static List<Transform> GetChildrenRecursively(Transform t)
        {
            if (t == null)
                return new List<Transform>();

            List<Transform> list = new List<Transform>();

            foreach (Transform child in t)
            {
                list.AddRange(GetChildrenRecursively(child));
            }

            list.Add(t);
            return list;
        }
    }
}