using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class MKExtensions
{
    /// <summary>
    /// Destroy all the child objects of the transform.
    /// </summary>
    public static void ClearChildren(this Transform transform)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
                Object.DestroyImmediate(transform.GetChild(i).gameObject);
            else
                Object.Destroy(transform.GetChild(i).gameObject);
#else
            Object.Destroy(transform.GetChild(i).gameObject);
#endif
        }
    }

    /// <summary>
    /// Returns the angle of the vector in radians.
    /// </summary>
    public static float AngleRadians(this Vector2 vec)
    {
        return Mathf.Atan2(vec.y, vec.x);
    }

    /// <summary>
    /// Returns the angle of the vector in degrees.
    /// </summary>
    public static float Angle(this Vector2 vec)
    {
        return Mathf.Atan2(vec.y, vec.x) * Mathf.Rad2Deg;
    }

    #region IList Extensions

    /// <summary>
    /// Returns a random item from the list that has at least one element.
    /// </summary>
    public static T RandomItem<T>(this IList<T> list)
    {
        if (list.Count == 0) throw new System.IndexOutOfRangeException("Cannot select a random item from an empty list");
        return list[Random.Range(0, list.Count)];
    }

    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        var count = list.Count;
        for (var i = 0; i < count - 1; i++)
        {
            var r = Random.Range(i, count);
            var tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }

    /// <summary>
    /// Removes and returns the last element of the specified list.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public static T RemoveLast<T>(this IList<T> list)
    {
        var last = list.Count - 1;
        if (last < 0)
            throw new System.IndexOutOfRangeException();
        T element = list[last];
        list.RemoveAt(last);
        return element;
    }

    #endregion

    #region IDictionary Extensions

    /// <summary>
    /// Removes the key from the dictionary and provides the removed value.
    /// </summary>
    public static bool Remove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out TValue value)
    {
        if (dictionary.TryGetValue(key, out value))
            return dictionary.Remove(key);
        return false;
    }

    /// <summary>
    /// Returns the default value of type U if the key does not exist in the dictionary
    /// </summary>
    public static U GetOrDefault<T, U>(this IDictionary<T, U> dic, T key)
    {
        if (dic.ContainsKey(key))
            return dic[key];
        return default(U);
    }

    /// <summary>
    /// Returns an existing value U for key T, or creates a new instance of type U using the default constructor, 
    /// adds it to the dictionary and returns it.
    /// </summary>
    public static U GetOrInsertNew<T, U>(this IDictionary<T, U> dic, T key)
        where U : new()
    {
        if (dic.ContainsKey(key))
            return dic[key];
        U newObj = new U();
        dic[key] = newObj;
        return newObj;
    }

    /// <summary>
    /// Adds a collection to an existing collection
    /// </summary>
    public static void AddCollection<TKey, TValue>(this ICollection<KeyValuePair<TKey, TValue>> to, ICollection<KeyValuePair<TKey, TValue>> from, bool overwriteExisting)
    {
        if (from == null)
            return;

        foreach (var fromKVP in from)
        {
            bool alreadyExists = false;

            //Check for existing entry
            foreach (var toKVP in to)
            {
                if(toKVP.Key.Equals(fromKVP.Key))
                {
                    //Remove existing
                    if (overwriteExisting)
                        to.Remove(toKVP);
                    else
                        alreadyExists = true;

                    break;
                }
            }

            //Key already exists, do not overwrite
            if (alreadyExists)
            {
                continue;
            }

            to.Add(fromKVP);
        }
    }

    #endregion
}