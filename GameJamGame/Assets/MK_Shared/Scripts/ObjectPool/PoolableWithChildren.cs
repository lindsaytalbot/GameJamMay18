using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolableWithChildren : Poolable {

    [SerializeField]
    private PoolableEvents[] children;

    public override void OnLoaded(Poolable prefab, Transform parent, Vector3 localPos, Vector3 localScale, Quaternion localRotation)
    {
        base.OnLoaded(prefab, parent, localPos, localScale, localRotation);

        foreach (PoolableEvents child in children)
            child.OnLoaded();
    }

    public override void OnPooled()
    {
        base.OnPooled();

        foreach (PoolableEvents child in children)
            child.OnPooled();
    }

    [ContextMenu("Get Children")]
    private void GetChildren()
    {
        children = GetComponentsInChildren<PoolableEvents>();
    }
}

public abstract class PoolableEvents : MonoBehaviour
{
    public abstract void OnPooled();
    public virtual void OnLoaded() { }
}
