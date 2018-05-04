using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public enum TextChangeStyle
{
    Instant,
    Tween,
    TweenUnscaled,
    Frozen
}

public class ObserverText : MonoBehaviour
{
    [SerializeField]
    protected string formatString;

    protected TMP_Text text;

    protected readonly StringBuilder currentString = new StringBuilder();
    protected readonly List<KeyValuePair<string, Observable<int>>> replacements = new List<KeyValuePair<string, Observable<int>>>();

    protected readonly Observable<int> value = new Observable<int>(0);
    public Observable<int> Value { get { return value; } }

    protected Tween tween;

    [SerializeField]
    protected TextChangeStyle changeStyle;
    public TextChangeStyle ChangeStyle
    {
        get { return changeStyle; }
        set
        {
            if (value != changeStyle)
            {
                changeStyle = value;
                OnValueChanged();
            }
        }
    }

    protected bool initilized;

    [SerializeField]
    protected float tweenTime = 0.5f;
    public float TweenTime
    {
        get { return tweenTime; }
        set { tweenTime = value; }
    }

    public string FormatString
    {
        get { return formatString; }
        set
        {
            ClearReplacements();

            formatString = value;

            int end = 0;
            while (true)
            {
                int start = formatString.IndexOf('{', end);
                end = formatString.IndexOf('}', end);

                if (start == -1 || end == -1)
                    break;

                var key = formatString.Substring(start, ++end - start);

                if (!replacements.Any(p => key == p.Key))
                {
                    var name = key.Substring(1, key.Length - 2);
                    Observable<int> observable;
                    try
                    {
                        observable = Observable<int>.Find(name);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e);
                        observable = new Observable<int>(-1);
                    }
                    observable.AddObserver(OnValueChanged);
                    replacements.Add(new KeyValuePair<string, Observable<int>>(key, observable));
                }
            }

            OnValueChanged();
        }
    }

    public void SetFormatString(Observable<int> observable)
    {
        const string key0 = "{0}";
        SetFormatString(key0, observable);
    }

    public void SetValue(int value)
    {
        var temp = changeStyle;
        Value.Value = value;
        changeStyle = TextChangeStyle.Instant;
        OnValueChanged();
        changeStyle = temp;
    }

    public void SetFormatString(string format, Observable<int> observable0)
    {
        ClearReplacements();
        formatString = format;
        observable0.AddObserver(OnValueChanged);
        replacements.Add(new KeyValuePair<string, Observable<int>>(format, observable0));
        OnValueChanged();
    }
    
    public void ForceUpdate()
    {
        OnValueChanged();
    }


    protected virtual void OnValueChanged()
    {
        Start(); //init

        if (changeStyle == TextChangeStyle.Frozen)
        {
            tween.Kill();
            UpdateText();
            return;
        }

        if (replacements.Count == 0)
        {
            text.text = formatString;
            return;
        }

        if (changeStyle == TextChangeStyle.Instant)
        {
            Value.Value = replacements[0].Value;
            UpdateText();
        }
        else
        {
            tween.Kill();

            tween = DOTween.To(() => Value, x =>
            {
                Value.Value = x;
                UpdateText();
            }, replacements[0].Value, tweenTime);

            tween.SetUpdate(changeStyle == TextChangeStyle.TweenUnscaled);
        }
    }

    protected void UpdateText()
    {
        currentString.Length = 0; // clear
        currentString.Append(formatString);

        for (int i = replacements.Count - 1; i >= 0; i--)
        {
            var kvp = replacements[i];
            int value = i == 0 ? Value : kvp.Value;
            var text = value.ToString("n0");
            currentString.Replace(kvp.Key, text);
        }

        text.text = currentString.ToString();
    }

    protected void ClearReplacements()
    {
        Value.Value = 0;

        for (int i = replacements.Count - 1; i >= 0; i--)
        {
            var kvp = replacements[i];
            var observable = kvp.Value;
            if (observable != null)
                observable.RemoveObserver(OnValueChanged);
        }

        replacements.Clear();
    }

    protected void Awake()
    {
        text = GetComponent<TMP_Text>();
        if (text == null)
            throw new System.ArgumentNullException("text");
    }

    protected void Start()
    {
        if (initilized)
            return;

        initilized = true;

        // only set format string on start if some other script hasn't already done it
        if (replacements.Count == 0)
        {
            // frozen change style should start at 0, but everything else should start instantly
            if (changeStyle != TextChangeStyle.Frozen)
            {
                var temp = changeStyle;
                changeStyle = TextChangeStyle.Instant;

                FormatString = formatString;

                changeStyle = temp;
            }
            else
            {
                FormatString = formatString;
            }
        }
    }

    protected void OnDestroy()
    {
        ClearReplacements();
    }
}
