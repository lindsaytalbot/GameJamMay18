using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MightyKingdom;

public class Test : MonoBehaviour {

    public int value;

    public Text text;

    [SerializeField]
    AudioClip clip1;

    [SerializeField]
    AudioClip clip2;

    [Header("Ad Testing")]
    [SerializeField]
    Image adAvailability;

    public void Awake()
    {
        MKPlayerPrefs.Init();
        MKPlayerPrefs.AddKeyChangeListener(OnKeysChanged);

        MKPlayerPrefs.SetInt("testInt", 0);
        MKPlayerPrefs.SetString("testString", "1");

        MKPlayerPrefs.SetStringArray("testString", new string[] { "Test Value1", "Test Value2", "bad sep," });

        MKPlayerPrefs.SetFloat("testFloat", 2f);
        MKPlayerPrefs.Save();

        MKPlayerPrefs.GetInt("testInt");
        MKPlayerPrefs.GetString("testString");
        MKPlayerPrefs.GetFloat("testFloat");
    }

    void Start()
    {
        LoadFromDisk();
    }

    private void NotificationFailure(string errorMessage)
    {
        MKLog.LogError(errorMessage);
    }

    public void OnKeysChanged(List<string> keys)
    {
        if (keys.Contains("test"))
        {
            MKLog.Log("test callback, key 'test'");
            value = MKPlayerPrefs.GetInt("test");
        }
    }

    void Update()
    {
        text.text = value.ToString();
        adAvailability.color = MKAdManager.AdAvailable() ? Color.green : Color.red;
    }

    public void IncrimentValue()
    {
        value++;
    }

    public void LoadFromDisk()
    {
        value = MKPlayerPrefs.GetInt("test");
    }

    public void SaveToDisk()
    {
        MKPlayerPrefs.SetInt("test", value);
    }

    public void PlayMusic1()
    {
        MKAudioManager.PlayMusic(clip1, 2, 1, true, MKAudioManager.FadeTypes.CrossFade);
    }

    public void PlayMusic2()
    {
        MKAudioManager.PlayMusic(clip2, 2, 1, true, MKAudioManager.FadeTypes.CrossFade);
    }

    public void WatchAd()
    {
        MKAdManager.ShowAd("test",() => { MKLog.Log("AdWatchSuccess"); }, (string error) => { MKLog.LogError(error); });
    }
}
