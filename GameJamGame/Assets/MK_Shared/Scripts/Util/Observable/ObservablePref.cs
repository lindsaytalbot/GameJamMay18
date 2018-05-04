using MightyKingdom;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObservablePref<T> : Observable<T>
{
    protected string key;
    protected T defaultValue;

    public ObservablePref() { }

    public ObservablePref(string key, T defaultValue = default(T)) : base()
    {
        Initialize(key, defaultValue);
    }

    public void Initialize(string key, T defaultValue = default(T))
    {
        this.key = key;
        this.defaultValue = defaultValue;
        LoadPref();
        onValueChanged += SavePref;

        //Reload if prefs loaded from cloud
        MKPlayerPrefs.AddKeyChangeListener((List<string> keys) => { LoadPref(); });
    }

    public void Save()
    {
        SavePref();
    }

    protected abstract void LoadPref();
    protected abstract void SavePref();
}

public class ObservablePrefInt : ObservablePref<int>
{
    public ObservablePrefInt() : base() { }
    public ObservablePrefInt(string key, int defaultValue = default(int)) : base(key, defaultValue) { }

    protected override void LoadPref()
    {
        value = MKPlayerPrefs.GetInt(key, defaultValue);
    }

    protected override void SavePref()
    {
        MKPlayerPrefs.SetInt(key, value);
        MKPlayerPrefs.Save();
    }
}

public class ObservablePrefBool : ObservablePref<bool>
{
    public ObservablePrefBool() : base() { }
    public ObservablePrefBool(string key, bool defaultValue = default(bool)) : base(key, defaultValue) { }

    protected override void LoadPref()
    {
        value = MKPlayerPrefs.GetBool(key, defaultValue);
    }

    protected override void SavePref()
    {
        MKPlayerPrefs.SetBool(key, value);
        MKPlayerPrefs.Save();
    }
}

public class ObservablePrefFloat : ObservablePref<float>
{
    public ObservablePrefFloat() : base() { }
    public ObservablePrefFloat(string key, float defaultValue = default(float)) : base(key, defaultValue) { }

    protected override void LoadPref()
    {
        value = MKPlayerPrefs.GetFloat(key, defaultValue);
    }

    protected override void SavePref()
    {
        MKPlayerPrefs.SetFloat(key, value);
        MKPlayerPrefs.Save();
    }
}

public class ObservablePrefString : ObservablePref<string>
{
    public ObservablePrefString() : base() { }
    public ObservablePrefString(string key, string defaultValue = default(string)) : base(key, defaultValue) { }

    protected override void LoadPref()
    {
        value = MKPlayerPrefs.GetString(key, defaultValue);
    }

    protected override void SavePref()
    {
        MKPlayerPrefs.SetString(key, value);
        MKPlayerPrefs.Save();
    }
}

public class ObservablePrefDateTime : ObservablePref<DateTime>
{
    public ObservablePrefDateTime() : base() { }
    public ObservablePrefDateTime(string key, DateTime defaultValue = default(DateTime)) : base(key, defaultValue) { }

    protected override void LoadPref()
    {
        value = MKPlayerPrefs.GetDate(key, defaultValue);
    }

    protected override void SavePref()
    {
        MKPlayerPrefs.SetDate(key, value);
        MKPlayerPrefs.Save();
    }
}

public class ObservablePrefEnum<T> : ObservablePref<T>
    where T : struct, IConvertible
{
    public ObservablePrefEnum() : base() { }
    public ObservablePrefEnum(string key, T defaultValue = default(T)) : base(key, defaultValue) { }

    protected override void LoadPref()
    {
        try
        {
            value = (T)Enum.Parse(typeof(T), MKPlayerPrefs.GetInt(key, defaultValue.ToInt32(null)).ToString());
        }
        catch
        {
            value = defaultValue;
        }
    }

    protected override void SavePref()
    {
        MKPlayerPrefs.SetInt(key, value.ToInt32(null));
        MKPlayerPrefs.Save();
    }
}

public class ObservablePrefDictionary<TObservable, TValue>
    where TObservable : ObservablePref<TValue>, new()
{
    protected Dictionary<string, TObservable> values;

    protected string keyPrefix;
    protected TValue defaultValue;

    public ObservablePrefDictionary(string keyPrefix, TValue defaultValue = default(TValue))
    {
        values = new Dictionary<string, TObservable>();
        this.keyPrefix = keyPrefix;
        this.defaultValue = defaultValue;
    }

    public TObservable this[string key]
    {
        get
        {
            string fullKey = keyPrefix + key;
            TObservable value;
            if (values.TryGetValue(fullKey, out value))
                return value;

            value = new TObservable();
            value.Initialize(fullKey, defaultValue);
            values.Add(fullKey, value);
            return value;
        }
    }
}
