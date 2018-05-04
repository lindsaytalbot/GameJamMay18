using System;
using System.Collections.Generic;
using UnityEngine;

namespace MightyKingdom
{
    public class MKAdManager : MonoBehaviour
    {
        private static MKAdManager _instance;
        private static List<MKAdModule> adProviders = new List<MKAdModule>();
        private static bool adEnabled = true;
        private const string ADS_SAVE_KEY = "MK_AdsEnabled";
        public const string SHOW_AD_ERROR_NO_ADS = "ShowAds() was called but there were no ads available to show";
        public const string SHOW_AD_ERROR_NO_PROVIDERS = "ShowAds() was called but there were no providers available to show";

        public static bool AdsEnabled
        {
            //Return true if ads are enabled
            get
            {
                return adEnabled;
            }
            //Disables Ads and saves the setting
            set
            {
                MKPlayerPrefs.SetBool(ADS_SAVE_KEY, value);
                adEnabled = value;

                //Fetch when ads turned on
                if (adEnabled)
                    Fetch();
            }
        }

        /// <summary>
        /// Sets up the Ad Manager and fetchs ads.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            if (_instance != null)
                return;

            MKPlayerPrefs.Init();

            //Create GameObject and add components
            GameObject go = new GameObject();
            go.name = "MKAdManager";
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<MKAdManager>();

            MKLog.Log("MKAdManager created", "green");
            AdsEnabled = MKPlayerPrefs.GetBool(ADS_SAVE_KEY, true);
            //Add you AdModules here, in order of preference
            //You can have platform specific providers by wrapping them in compile tags
            //adProviders.Add(new ExampleAdModule());

#if UNITY_EDITOR
            adProviders.Add(new EditorAdModule());
#endif
        }

        void OnApplicationPause(bool paused)
        {
            if (!paused)
                Fetch();
        }

        /// <summary>
        /// Returns true if AdsEnabled and an ad module has an ad available
        /// </summary>
        /// <returns></returns>
        public static bool AdAvailable()
        {
            //Ads are disabled
            if (AdsEnabled == false)
                return false;

            foreach (MKAdModule adModule in adProviders)
            {
                if (adModule.AdAvailable())
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the name of the first available provider
        /// </summary>
        /// <returns></returns>
        public static string GetProvider()
        {
            foreach (MKAdModule adModule in adProviders)
            {
                if (adModule.AdAvailable())
                    return adModule.GetType().Name;
            }

            return "N//A";
        }

        /// <summary>
        /// Returns true if any AdProviders are available on this platform
        /// </summary>
        /// <returns></returns>
        public static bool AdProviderAvailable()
        {
            return adProviders.Count > 0;
        }

        /// <summary>
        /// Tells each ad provider to fetch an ad
        /// </summary>
        /// <param name="forceFetch"></param>
        public static void Fetch(bool forceFetch = false)
        {
            //Ads are disabled
            if (AdsEnabled == false)
                return;

            foreach (MKAdModule adModule in adProviders)
            {
                adModule.Fetch(forceFetch);
            }
        }

        /// <summary>
        /// Shows an Ad from the first available provider
        /// </summary>
        /// <param name="onSuccess">What to do once the ad has completed showing</param>
        /// <param name="onFailure">What to do if the ad failed to show. Passes in the error as a string</param>
        public static void ShowAd(string placementID, Action onSuccess, Action<string> onFailure)
        {
            if (AdProviderAvailable() == false)
            {
                onFailure(SHOW_AD_ERROR_NO_PROVIDERS);
                return;
            }

            foreach (MKAdModule adModule in adProviders)
            {
                if (adModule.AdAvailable())
                {
                    OnAdWatchStart();
                    adModule.ShowAd(placementID, onSuccess, onFailure);
                    return;
                }
            }

            onFailure(SHOW_AD_ERROR_NO_ADS);
        }

        /// <summary>
        /// Called by ShowAd. Pauses the game and all audio until the ad has completed
        /// </summary>
        public static void OnAdWatchStart()
        {
            Time.timeScale = 0f;
            MKAudioManager.PauseAll();
        }

        /// <summary>
        /// Called by OnAdWatchSucces/OnAdWatchFailure in MKAdModule when a module has stopped showing an ad.
        /// Resumes the game and all audio
        /// </summary>
        public static void OnAdWatchComplete()
        {
            Time.timeScale = 1f;
            Fetch();
            MKAudioManager.ResumeAll();
        }

        /// <summary>
        /// Shows the test suite for the given provider
        /// </summary>
        /// <param name="providerIndex">Index of the provider</param>
        public static void ShowTestSuite(int providerIndex)
        {
            if (providerIndex < 0 || providerIndex > adProviders.Count - 1)
                return;

            adProviders[providerIndex].ShowTestSuite();
        }
    }
}