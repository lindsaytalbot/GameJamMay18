using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poolable : MonoBehaviour
{
    public Poolable PrefabRef { get; protected set; }

    public virtual void OnLoaded(Poolable prefab, Transform parent, Vector3 localPos, Vector3 localScale, Quaternion localRotation)
    {
        PrefabRef = prefab;
        transform.SetParent(parent);
        if (parent != null)
            transform.localPosition = localPos;
        else
            transform.position = localPos;
        transform.localScale = localScale;
        transform.localRotation = localRotation;
        gameObject.SetActive(true);
    }

    public virtual void OnPooled()
    {
        transform.SetParent(null);
        gameObject.SetActive(false);
    }

    public void Pool()
    {
        ObjectPool.PoolItem(this);
    }
}