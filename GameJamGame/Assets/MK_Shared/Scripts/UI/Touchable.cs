// Touchable.cs
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.UI
{
    public class Touchable : Graphic
    {
        public override bool Raycast(Vector2 sp, Camera eventCamera)
        {
            return true;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}

#if UNITY_EDITOR
namespace UnityEngine.UI
{
    [CustomEditor(typeof(Touchable))]
    public class TouchableEditor : Editor
    {

        public override void OnInspectorGUI() { }
    }
}
#endif
