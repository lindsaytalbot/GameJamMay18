using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public delegate void ValueChangedEvent();
public delegate void ValueChangedEvent<T>(Observable<T> value);

public class Observable<T>
{
    protected T value;

    protected ValueChangedEvent onValueChanged;

    public Observable() { }

    public Observable(T value) : this()
    {
        this.value = value;
    }

    public T Value
    {
        get { return value; }
        set
        {
            if (EqualityComparer<T>.Default.Equals(this.value, value))
                return;

            this.value = value;
            if (onValueChanged != null)
                onValueChanged();
        }
    }

    public void AddObserver(ValueChangedEvent observer)
    {
        onValueChanged += observer;
    }

    public void RemoveObserver(ValueChangedEvent observer)
    {
        onValueChanged -= observer;
    }

    public static implicit operator T(Observable<T> observable)
    {
        return observable.value;
    }

    public static Observable<T> Find(string parameter)
    {
        const string paramName = "parameter";

        if (parameter == null)
            throw new System.ArgumentNullException(paramName);

        var index = parameter.LastIndexOf('.');
        if (index < 0)
            throw new System.ArgumentException("Parameter must be of the form (Type.Property)", paramName);

        var typeName = parameter.Substring(0, index);
        var propertyName = parameter.Substring(index + 1);

        var type = System.Type.GetType(typeName);
        if (type == null)
            throw new System.ArgumentException(string.Format("Type ({0}) not defined", typeName), paramName);

        var typeInstanceProperty = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        if (typeInstanceProperty == null)
            throw new System.ArgumentException(string.Format("No public static property (Instance) found in ({0})", type), paramName);

        var typeInstance = typeInstanceProperty.GetValue(null, null);
        if (typeInstance == null)
            throw new System.NullReferenceException(string.Format("Property (Instance) in ({0}) was null", type));

        var observableProperty = type.GetProperty(propertyName);
        if (observableProperty == null)
            throw new System.ArgumentException(string.Format("No public property ({0}) found in ({1})", propertyName, type), paramName);

        object value = null;
        try
        {
            value = observableProperty.GetValue(typeInstance, null);
        }
        catch (System.Exception)
        {
            throw new System.InvalidCastException(string.Format("Property (Instance) in ({0}) could not be cast to ({0})", type));
        }
        if (value == null)
            throw new System.NullReferenceException(string.Format("Property ({0}) in ({1}) was null", propertyName, type));

        var observable = value as Observable<T>;
        if (observable == null)
            throw new System.InvalidCastException(string.Format("Property ({0}) in ({1}) of type ({2}) could not be cast to ({3})", propertyName, type, value.GetType(), typeof(Observable<T>)));

        return observable;
    }

    public override string ToString()
    {
        return value.ToString();
    }
}
