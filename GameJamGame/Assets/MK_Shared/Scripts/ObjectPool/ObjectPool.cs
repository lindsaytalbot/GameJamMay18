using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MightyKingdom;

public static class ObjectPool
{
    private static Dictionary<Poolable, Stack<Poolable>> pools = new Dictionary<Poolable, Stack<Poolable>>();
#if UNITY_EDITOR
    private static Transform poolParent;
#endif

    public static Poolable LoadPrefab(Poolable prefab, Transform parent)
    {
        return LoadPrefab(prefab, Vector3.zero, Vector3.one, Quaternion.identity, parent);
    }

    public static Poolable LoadPrefab(Poolable prefab, Vector3 localPos)
    {
        return LoadPrefab(prefab, localPos, Vector3.one, Quaternion.identity, null);
    }

    //Get instance from pool
    public static Poolable LoadPrefab(Poolable prefab, Vector3 localPos, Vector3 localScale, Quaternion localRotation, Transform parent = null)
    {
#if UNITY_EDITOR
        //Don't use object pool when game not running
        if(!Application.isPlaying)
        {
            Poolable created = GameObject.Instantiate(prefab, parent);
            created.transform.localPosition = localPos;
            created.transform.localRotation = localRotation;
            created.transform.localScale = localScale;
            return created;
        }
#endif

        if (prefab == null)
        {
            MKLog.LogError("Attempted to load NULL prefab");
            return null;
        }

        Stack<Poolable> pool = GetPool(prefab);

        Poolable spawned = null;

        if (pool.Count > 0)
        {
            spawned = pool.Pop();
        }
        else
        {
//            Debug.LogError("Item created! " + prefab.name);
            spawned = Object.Instantiate(prefab, parent, false);
        }

        spawned.OnLoaded(prefab, parent, localPos, localScale, localRotation);
        return spawned;
    }

    //Return instance to pool
    public static void PoolItem(Poolable item)
    {
        if (item == null)
            return;

#if UNITY_EDITOR 
        //Don't try and use object pool when game not running
        if(!Application.isPlaying)
        {
            GameObject.DestroyImmediate(item.gameObject);
            return;
        }

        if (poolParent == null)
        {
            poolParent = new GameObject().transform;
            poolParent.name = "POOL_PARENT";
        }
#endif

        //Unable to pool this object, just destroy it
        if (item.PrefabRef == null)
        {
            Debug.LogError("Prefab not configured for " + item.name);
            GameObject.Destroy(item.gameObject);
            return;
        }

        Stack<Poolable> pool = GetPool(item.PrefabRef);
        if (pool.Contains(item))
        {
            Debug.Log("ALREADY POOLED " + item.name);
            return;
        }

        pool.Push(item);
        item.OnPooled();

#if UNITY_EDITOR
        item.transform.SetParent(poolParent);
#endif
    }

    private static Stack<Poolable> GetPool(Poolable prefab)
    {
        if (!pools.ContainsKey(prefab) || pools[prefab] == null)
        {
            pools[prefab] = new Stack<Poolable>();
        }

        return pools[prefab];
    }

    //Destroys all objects in all pools and clears it
    public static void EmptyPool()
    {
        foreach (var kvp in pools)
        {
            if (kvp.Value != null)
            {
                foreach (var poolable in kvp.Value)
                {
                    if (poolable != null)
                        Object.Destroy(poolable.gameObject);
                }
            }
        }

        pools = new Dictionary<Poolable, Stack<Poolable>>();
    }

    public static void PrewarmItem(Poolable prefab, int count)
    {
        Poolable created = null;

        for (int i = 0; i < count; i++)
        {
            created = Object.Instantiate(prefab);
            created.OnLoaded(prefab, null, Vector2.zero, Vector3.one, Quaternion.identity);
            created.Pool();
        }
    }
}
