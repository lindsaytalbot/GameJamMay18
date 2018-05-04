#if (!UNITY_EDITOR && UNITY_IOS)  || (!UNITY_EDITOR && UNITY_TVOS)
#define PLATFORM_IOS
#endif
#if !UNITY_EDITOR && UNITY_ANDROID
#define PLATFORM_ANDROID
#endif


using UnityEngine;
using Prime31;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using System.Linq;

namespace MightyKingdom
{
    //Wraps around Unity's player prefs and provides utility functions
    public class MKPlayerPrefs
    {
        public readonly static Version MK_CODE_VERSION = new Version(1, 4);
        public static UnityAction<List<string>> keyChangeListeners;

        private static bool configured = false;
        private static bool performedMigration = false;
        private const string LAST_SAVED_GAME_VERSION = "MK_LastSavedVersion";
        private const string LAST_SAVED_MK_CODE_VERSION = "MK_ShardCodeVersion";
        private const string LAST_SAVED_DATE = "MK_LastSaveDate";

        //Called automatically
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            if (configured)
                return;
            configured = true;

#if PLATFORM_IOS
            Prime31.iCloudManager.keyValueStoreDidChangeEvent += OnKeysChanged;
            Prime31.P31Prefs.synchronize();
#endif

            MKLog.Log("MKPlayerPrefs initialized", "green");
        }

        /// <param name="listener">The listener to call when keys change via iCloud</param>
        public static void AddKeyChangeListener(UnityAction<List<string>> listener)
        {
            keyChangeListeners += listener;
        }

        /// <param name="listener">The listener to remove</param>
        public static void RemoveKeyChangeListener(UnityAction<List<string>> listener)
        {
            keyChangeListeners -= listener;
        }

        /// <summary>
        /// Recieves a list of keys changed via iCloud sync. 
        /// Calls listeners added with "AddiCloudListener" and calls each with a list of keys that has been changed
        /// </summary>
        /// <param name="changedKeys"></param>
        public static void OnKeysChanged(List<object> changedKeys)
        {
            if (changedKeys == null)
                MKLog.LogError("ValueChangedListener, null");

            MKLog.Log("ValueChangedListener: " + changedKeys.Count + " changes");

            List<string> keys = new List<string>();

            for (int i = 0; i < changedKeys.Count; i++)
            {
                keys.Add(changedKeys[i].ToString());
            }

            if (keyChangeListeners != null)
                keyChangeListeners.Invoke(keys);
        }

        #region Get and set values
        public static bool HasKey(string key)
        {
#if PLATFORM_IOS
            return P31Prefs.hasKey(key);
#endif
            return PlayerPrefs.HasKey(key);
        }

        public static void DeleteKey(string key)
        {
#if PLATFORM_IOS
            P31Prefs.removeObjectForKey(key);
#endif
            PlayerPrefs.DeleteKey(key);
        }

        public static int GetInt(string key, int defValue = 0)
        {
#if PLATFORM_IOS
            FixIOSInt(key, true);
            return P31Prefs.hasKey(key) ? P31Prefs.getInt(key) : defValue;
#endif
            return PlayerPrefs.GetInt(key, defValue);
        }

        public static void SetInt(string key, int value)
        {

#if PLATFORM_IOS
            FixIOSInt(key, false);
            P31Prefs.setInt(key, value);   
#endif
            PlayerPrefs.SetInt(key, value);
        }

        public static float GetFloat(string key, float defValue = 0)
        {
#if PLATFORM_IOS
            FixIOSFloat(key, true);
            return P31Prefs.hasKey(key) ? P31Prefs.getFloat(key) : defValue;     
#endif
            return PlayerPrefs.GetFloat(key, defValue);
        }

        public static void SetFloat(string key, float value)
        {
#if PLATFORM_IOS
            FixIOSFloat(key, false);
            P31Prefs.setFloat(key, value);     
#endif
            PlayerPrefs.SetFloat(key, value);
        }

        public static string GetString(string key, string defValue = "")
        {
#if PLATFORM_IOS
            FixIOSString(key, true);
            return P31Prefs.hasKey(key) ? P31Prefs.getString(key) : defValue;         
#endif
            return PlayerPrefs.GetString(key, defValue);
        }

        public static void SetString(string key, string value)
        {
#if PLATFORM_IOS
            FixIOSString(key, false);
            P31Prefs.setString(key, value); 
#endif
            PlayerPrefs.SetString(key, value);
        }

        public static string[] GetStringArray(string key, string separator = ",")
        {
            string values = GetString(key);
            return values.Split(new string[] { separator }, System.StringSplitOptions.None);
        }

        public static void SetStringArray(string key, string[] values, string separator = ",")
        {
            string[] v = values.Where(a => a.Contains(separator)) as string[];
            if (v.Length > 0)
                MKLog.LogError(key + ": Values contain separator '"+separator+"' and may not get correctly!");

            string value = string.Join(separator, values);
            SetString(key, value);
        }

        public static bool GetBool(string key, bool defValue = false)
        {
            return GetInt(key, defValue ? 1 : 0) != 0;
        }

        public static void SetBool(string key, bool value)
        {
            SetInt(key, value ? 1 : 0);
        }

        public static DateTime GetLastSavedDate()
        {
            DateTime defValue = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return GetDate(LAST_SAVED_DATE, defValue);
        }

        public static DateTime GetDate(string key, DateTime defValue)
        {
            string dateString = GetString(key, "");

            DateTime result;
            if (string.IsNullOrEmpty(dateString) || !DateTime.TryParse(dateString, out result))
            {
                return defValue;
            }

            return result.ToLocalTime();
        }

        public static void SetDate(string key, DateTime value)
        {
            SetString(key, value.ToUniversalTime().ToString());
        }

        //Returns the last version the game was saved in. Useful for migration
        public static string GetLastSavedVersion()
        {
            //Uses playerprefs because android prefs might not be loaded yet
            return PlayerPrefs.GetString(LAST_SAVED_GAME_VERSION);
        }

        //Returns the last version the game was saved in. Useful for migration
        public static Version GetLastCodeVersion()
        {
            string versionString = PlayerPrefs.GetString(LAST_SAVED_MK_CODE_VERSION, null);

            if (versionString != null)
                MKLog.Log("Version string: " + versionString);
            else
                MKLog.Log("Version string null");

            if (string.IsNullOrEmpty(versionString))
                return new Version(0, 0);

            return new Version(versionString);
        }


        #endregion

        public static void Save()
        {
            SetString(LAST_SAVED_GAME_VERSION, Application.version);
            SetString(LAST_SAVED_MK_CODE_VERSION, MK_CODE_VERSION.ToString());
            SetDate(LAST_SAVED_DATE, DateTime.Now.ToUniversalTime());

            PlayerPrefs.Save();

            if (performedMigration)
            {
                PlayerPrefs.Save();
                performedMigration = false;
            }

#if PLATFORM_IOS
            Prime31.P31Prefs.synchronize();
#endif
        }


#if PLATFORM_IOS
        private static void FixIOSInt(string key, bool set)
        {
            if (PlayerPrefs.HasKey(key))
            {
                //MKLog.Log("Migrated Key " + key);
                int val = PlayerPrefs.GetInt(key);
                PlayerPrefs.DeleteKey(key);
                performedMigration = true;

                if (set)
                {
                    SetInt(key, val);
                }
            }
        }

        
        private static void FixIOSFloat(string key, bool set)
        {
            if (PlayerPrefs.HasKey(key))
            {
                //MKLog.Log("Migrated Key " + key);
                float val = PlayerPrefs.GetFloat(key);
                PlayerPrefs.DeleteKey(key);
                performedMigration = true;

                if (set)
                {
                    SetFloat(key, val);
                }
            }
        }

        private static void FixIOSString(string key, bool set)
        {
            if (PlayerPrefs.HasKey(key))
            {
                //MKLog.Log("Migrated Key " + key);
                string val = PlayerPrefs.GetString(key);
                PlayerPrefs.DeleteKey(key);
                performedMigration = true;

                if (set)
                {
                    SetString(key, val);
                }
            }
        }
#endif

        public static void ClearPrefs()
        {
#if PLATFORM_IOS
            P31Prefs.removeAll();
#endif
            PlayerPrefs.DeleteAll();
            Save();
        }
    }
}