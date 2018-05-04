using System;
using UnityEngine;

namespace MightyKingdom
{
    public class MKAnalyticsSession : MonoBehaviour
    {
        //Session details
        private bool gameStarted;

        private ObservablePref<DateTime> installDate;
        private ObservablePref<int> sessionCount;
        private ObservablePref<string> previousSessionId;
        private ObservablePref<DateTime> lastSessionDate;
        private ObservablePref<int> daysPlayed;
        private const string sessionIdPref = "unity.player_sessionid";

        public int DaysPlayed { get { return daysPlayed.Value; } }
        public int SessionCount { get { return sessionCount.Value; } }

        protected void Awake()
        {
            DontDestroyOnLoad(gameObject);
            RecordInstallDate();
        }

        protected void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                GameEnd();
            }
            else
            {
                GameStart();
            }
        }

        protected void OnApplicationQuit()
        {
            GameEnd();
        }

        private void RecordInstallDate()
        {
            installDate = new ObservablePrefDateTime("MK_Analytics_installDate", DateTime.Now);
            installDate.Save();
        }

        protected virtual void GameStart()
        {
            if (gameStarted)
                return;
            gameStarted = true;          

            CheckForNewSession();
            CheckForNewDayPlayed();

            MKLog.Log("Game start");
        }

        protected virtual void GameEnd()
        {
            if (!gameStarted)
                return;
            gameStarted = false;

            previousSessionId.Value = PlayerPrefs.GetString(sessionIdPref);
            previousSessionId.Save();

            lastSessionDate.Value = DateTime.Today;
            lastSessionDate.Save();

            MKLog.Log("Game end");
        }

        /// <summary>
        /// Check if we need to increment the number of days played.
        /// </summary>
        private void CheckForNewDayPlayed()
        {
            lastSessionDate = new ObservablePrefDateTime("MK_Analytics_lastSessionDate", DateTime.Today);
            lastSessionDate.Save();

            daysPlayed = new ObservablePrefInt("MK_Analytics_daysPlayedPref", 1);

            var isNewDay = !lastSessionDate.Value.Equals(DateTime.Today);            
            if (isNewDay) IncrementDaysPlayed();
        }

        private void IncrementDaysPlayed()
        {
            daysPlayed.Value++;
            daysPlayed.Save();
            MKLog.Log("Incremented days played: " + daysPlayed.Value);
        }

        /// <summary>
        /// Check if unity has assigned a new session id.
        /// </summary>
        private void CheckForNewSession()
        {
            var currentSessionId = PlayerPrefs.GetString(sessionIdPref);

            previousSessionId = new ObservablePrefString("MK_Analytics_previous_sessionid", currentSessionId);

            sessionCount = new ObservablePrefInt("MK_Analytics_sessionCount", 1);

            var isNewSession = !previousSessionId.Value.Equals(currentSessionId);
            if (isNewSession) IncrementSessionCount();
        }

        private void IncrementSessionCount()
        {
            sessionCount.Value++;
            sessionCount.Save();
            MKLog.Log("New session: " + sessionCount.Value);
        }
    }
}