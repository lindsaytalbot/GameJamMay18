using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics.Experimental;

namespace MightyKingdom
{
    public class MKAnalytics
    {
        public static bool DebugMode { get; set; }

        //Lazily instantiate on first reference
        protected static MKAnalyticsSession sessionManager;
        private static List<MKAnalyticsModule> analyticsModules = new List<MKAnalyticsModule>();

        /// <summary>
        /// Automatically loads session manager and
        /// analytics providers from Resources\Analytics
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            if (sessionManager != null)
                return;

            MKPlayerPrefs.Init();
            Load();            

            MKLog.Log("MKAnalytics loaded. " + analyticsModules.Count + " modules loaded", "green");
        }

        private static void Load()
        {
            //Load session manager
            MKAnalyticsSession[] managers = Resources.LoadAll<MKAnalyticsSession>("Analytics");
            if (managers.Length > 0)
            {
                sessionManager = GameObject.Instantiate<MKAnalyticsSession>(managers[0]);
                GameObject.DontDestroyOnLoad(sessionManager.gameObject);
            }
            else
            {
                MKLog.LogError("No session manager available");
            }

            //Load modules
            foreach (MKAnalyticsModule module in Resources.LoadAll<MKAnalyticsModule>("Analytics"))
            {
                analyticsModules.Add(module);
                module.InitSession();
            }
        }

        public static void ScreenVisit(string screenID)
        {
            AnalyticsEvent.ScreenVisit(screenID);
        }

        #region Progression
        public static void GameStart()
        {
            ReportEvent(StandardEventNames.GAME_START);
            AnalyticsEvent.GameStart();
        }

        public static void GameOver(string levelName = null, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> details = null;

            if (levelName != null)
            {
                details = new Dictionary<string, object>()
                {
                    { StandardEventNames.Parameters.LEVEL_NAME, levelName }
                };

                details.AddCollection(extraDetails, false);
            }

            ReportEvent(StandardEventNames.GAME_OVER, details);
            AnalyticsEvent.GameOver(levelName, extraDetails);
        }

        public static void LevelStart(string levelName, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> details = new Dictionary<string, object>
            {
                { StandardEventNames.Parameters.LEVEL_NAME, levelName },
            };

            details.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.LEVEL_START, details);
            AnalyticsEvent.LevelStart(levelName, extraDetails);
        }

        //Called when a level has been completed successfully
        public static void LevelComplete(string levelName, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> details = new Dictionary<string, object>
            {
                { StandardEventNames.Parameters.LEVEL_NAME, levelName },
            };

            details.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.LEVEL_COMPLETE, details);
            AnalyticsEvent.LevelComplete(levelName, extraDetails);
        }

        //Called when a level has been completed successfully and has a score
        public static void LevelComplete(string levelName, int score, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> details = new Dictionary<string, object>
            {
                { StandardEventNames.Parameters.LEVEL_NAME, levelName },
                { "score", score }
            };

            if(extraDetails == null)
                extraDetails = new Dictionary<string, object>();

            extraDetails["score"] = score;
            details.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.LEVEL_COMPLETE, details);
            AnalyticsEvent.LevelComplete(levelName, extraDetails);
        }

        public static void LevelQuit(string levelName, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> details = new Dictionary<string, object>
            {
                { StandardEventNames.Parameters.LEVEL_NAME, levelName },
            };

            details.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.LEVEL_QUIT, details);
            AnalyticsEvent.LevelQuit(levelName, extraDetails);
        }

        public static void LevelFail(string levelName, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> details = new Dictionary<string, object>
            {
                { StandardEventNames.Parameters.LEVEL_NAME, levelName },
            };

            details.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.LEVEL_FAIL, details);
            AnalyticsEvent.LevelFail(levelName, extraDetails);
        }

        public static void LevelUp(int oldLevel, int newLevel, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> details = new Dictionary<string, object>
            {
                { StandardEventNames.Parameters.OLD_LEVEL_INDEX, oldLevel },
                { StandardEventNames.Parameters.NEW_LEVEL_INDEX, newLevel },
            };

            details.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.LEVEL_UP, details);
            AnalyticsEvent.LevelUp(oldLevel, newLevel, extraDetails);
        }
        #endregion

        #region Onboarding and Tutorial
        public static void FirstInteraction(string actionId = null, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> gameDetails = new Dictionary<string, object>()
            {
                {"game_version", Application.version }
            };

            if (actionId != null)
                gameDetails.Add(StandardEventNames.Parameters.ACTION_ID, actionId);

            gameDetails.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.FIRST_INTERACTION, gameDetails);

            AnalyticsEvent.FirstInteraction(actionId, extraDetails);
        }

        public static void TutorialStart(string tutorialId, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> tutorialDetails = new Dictionary<string, object>()
            {
                {StandardEventNames.Parameters.TUTORIAL_ID, tutorialId },
            };

            tutorialDetails.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.TUTORIAL_START, tutorialDetails);
            AnalyticsEvent.TutorialStart(tutorialId, extraDetails);
        }

        public static void TutorialStep(string tutorialId, int step, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> tutorialDetails = new Dictionary<string, object>()
            {
                {StandardEventNames.Parameters.TUTORIAL_ID, tutorialId },
                {StandardEventNames.Parameters.STEP_INDEX, step },
            };

            tutorialDetails.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.TUTORIAL_STEP, tutorialDetails);
            AnalyticsEvent.TutorialStep(step, tutorialId, extraDetails);
        }

        public static void TutorialComplete(string tutorialId, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> tutorialDetails = new Dictionary<string, object>()
            {
                {StandardEventNames.Parameters.TUTORIAL_ID, tutorialId },
            };

            tutorialDetails.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.TUTORIAL_COMPLETE, tutorialDetails);
            AnalyticsEvent.TutorialComplete(tutorialId, extraDetails);
        }
        #endregion

        #region Engagement and Social
        public static void PushNotificationEnabled(Dictionary<string, object> extraDetails = null)
        {
            ReportEvent(StandardEventNames.PUSH_NOTIFICATION_ENABLE, null);
            AnalyticsEvent.PushNotificationEnable(extraDetails);
        }

        public static void PushNotificationClick(string messageId, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> notificationDetails = new Dictionary<string, object>()
            {
                {StandardEventNames.Parameters.MESSAGE_ID, messageId },
            };

            notificationDetails.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.PUSH_NOTIFICATION_CLICK, notificationDetails);
            AnalyticsEvent.PushNotificationClick(messageId, extraDetails);
        }

        public static void AchievementUnlocked(string achievementId, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> achievementDetails = new Dictionary<string, object>()
            {
                {StandardEventNames.Parameters.ACHIEVEMENT_ID, achievementId },
            };

            achievementDetails.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.ACHIEVEMENT_UNLOCK, achievementDetails);
            AnalyticsEvent.AchievementUnlocked(achievementId, extraDetails);
        }

        public static void AchievementStep(string achievementId, int step, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> achievementDetails = new Dictionary<string, object>()
            {
                {StandardEventNames.Parameters.ACHIEVEMENT_ID, achievementId },
                {StandardEventNames.Parameters.STEP_INDEX, step },
            };

            achievementDetails.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.ACHIEVEMENT_STEP, achievementDetails);
            AnalyticsEvent.AchievementStep(step, achievementId, extraDetails);
        }

        public static void SocialShare(ShareType shareType, SocialNetwork socialNetwork, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> socialDetails = new Dictionary<string, object>()
            {
                {StandardEventNames.Parameters.SHARE_TYPE, shareType },
                {StandardEventNames.Parameters.SOCIAL_NETWORK, socialNetwork },
            };

            socialDetails.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.SOCIAL_SHARE, socialDetails);
            AnalyticsEvent.SocialShare(shareType, socialNetwork, eventData:extraDetails);
        }
        #endregion

        #region Monetization
        public static void StorePurchase(StoreType storeType, string purchaseId, string purchaseLocation, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> purchaseDetails = new Dictionary<string, object>()
            {
                {StandardEventNames.Parameters.TYPE, storeType },
                {StandardEventNames.Parameters.PURCHASE_ID, purchaseId },
            };

            purchaseDetails.AddCollection(extraDetails, false);

            ReportEvent("store_purchase", purchaseDetails);
        }

        public static void StoreOpened(StoreType storeType, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> storeDetails = new Dictionary<string, object>()
            {
                {StandardEventNames.Parameters.TYPE, storeType },
            };

            storeDetails.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.STORE_OPENED, storeDetails);
            AnalyticsEvent.StoreOpened(storeType);
        }

        public static void StoreItemClicked(StoreType storeType, string itemId, string itemName = null, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> storeDetails = new Dictionary<string, object>()
            {
                {StandardEventNames.Parameters.TYPE, storeType },
                {StandardEventNames.Parameters.ITEM_ID, itemId },
            };

            if (itemName != null)
                storeDetails.Add(StandardEventNames.Parameters.ITEM_NAME, itemName);

            storeDetails.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.STORE_ITEM_CLICK, storeDetails);
            AnalyticsEvent.StoreItemClick(storeType, itemId, itemName, extraDetails);
        }

        public static void CurrencyAcquired(string name, AcquisitionType type, AcquisitionSource source, float amount, float balance, string purchaseId = null, string purchaseName = null, int purchaseQty = 1, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> currencyDetails = new Dictionary<string, object>()
            {
                {StandardEventNames.Parameters.CURRENCY_NAME, name },
                {StandardEventNames.Parameters.TYPE, type.ToString() },
                {StandardEventNames.Parameters.SOURCE, source.ToString() },
                {StandardEventNames.Parameters.CURRENCY_AMOUNT, amount },
                {StandardEventNames.Parameters.CURRENCY_BALANCE, balance },
                {StandardEventNames.Parameters.PURCHASE_QUANTITY, purchaseQty },
            };

            if (purchaseId != null)
                currencyDetails.Add(StandardEventNames.Parameters.PURCHASE_ID, purchaseId);

            if (purchaseName != null)
                currencyDetails.Add(StandardEventNames.Parameters.PURCHASE_NAME, purchaseName);

            currencyDetails.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.CURRENCY_ACQUIRED, currencyDetails);
            AnalyticsEvent.CurrencyAcquired(name, type, source, amount, balance, purchaseId, purchaseName, purchaseQty, extraDetails);
        }

        public static void ConsumableSpent(string name, float amount, float balance, string itemPurchased = null, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> consumableDetails = new Dictionary<string, object>()
            {
                {StandardEventNames.Parameters.CONSUMABLE_NAME, name },
                {StandardEventNames.Parameters.CONSUMABLE_AMOUNT, amount },
                {StandardEventNames.Parameters.CONSUMABLE_BALANCE, balance },
            };

            if (itemPurchased != null)
                consumableDetails.Add(StandardEventNames.Parameters.CONSUMABLE_ITEM_PURCHASED, itemPurchased);

            consumableDetails.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.CONSUMABLE_SPENT, consumableDetails);
            AnalyticsEvent.ConsumableSpent(name, amount, balance, itemPurchased, extraDetails);
        }

        public static void ItemAcquired(string name, AcquisitionType type, AcquisitionSource source, string resourceType = null, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> itemDetails = new Dictionary<string, object>()
            {
                {StandardEventNames.Parameters.ITEM_NAME, name },
                {StandardEventNames.Parameters.TYPE, type },
                {StandardEventNames.Parameters.SOURCE, source },
            };

            if (resourceType != null)
                itemDetails.Add(StandardEventNames.Parameters.RESOURCE_TYPE, resourceType);

            itemDetails.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.ITEM_ACQUIRED, itemDetails);
            AnalyticsEvent.ItemAcquired(name, type, source, resourceType, extraDetails);
        }

        public static void AdOffer(string placementID, string provider, Dictionary<string, object> extraDetails = null)
        {
            Dictionary<string, object> adDetails = new Dictionary<string, object>()
            {
                {StandardEventNames.Parameters.AD_PLACEMENT_ID, placementID },
                {StandardEventNames.Parameters.AD_NETWORK_ID, provider }
            };

            adDetails.AddCollection(extraDetails, false);

            ReportEvent(StandardEventNames.AD_OFFER, adDetails);
            AnalyticsEvent.AdOffer(true, provider, placementID, extraDetails);
        }

        public static void AdStart(string placementID, string provider)
        {
            Dictionary<string, object> adDetails = new Dictionary<string, object>()
            {
                {StandardEventNames.Parameters.AD_PLACEMENT_ID, placementID },
                {StandardEventNames.Parameters.AD_NETWORK_ID, provider }
            };

            ReportEvent(StandardEventNames.AD_START, adDetails);
            AnalyticsEvent.AdStart(true, provider, placementID);
        }

        public static void AdComplete(string placementID, string provider)
        {
            Dictionary<string, object> adDetails = new Dictionary<string, object>()
            {
                {StandardEventNames.Parameters.AD_PLACEMENT_ID, placementID },
                {StandardEventNames.Parameters.AD_NETWORK_ID, provider }
            };

            ReportEvent(StandardEventNames.AD_COMPLETE, adDetails);
            AnalyticsEvent.AdComplete(true, provider, placementID);
        }
        #endregion

        //Report the event to all analytics providers
        public static void ReportEvent(string eventKey)
        {
            foreach (MKAnalyticsModule module in analyticsModules)
            {
                module.ReportEvent(eventKey);
            }

            if (DebugMode)
                MKLog.Log("Event: " + eventKey);
        }

        //Report the event to all analytics providers with details
        public static void ReportEvent(string eventKey, Dictionary<string, object> eventDetails)
        {
            //Tried calling with null eventDetails dict
            if(eventDetails == null)
            {
                ReportEvent(eventKey);
                return;
            }

            foreach (MKAnalyticsModule module in analyticsModules)
            {
                module.ReportEvent(eventKey, eventDetails);
            }

            if (DebugMode)
            {
                string parameters = "";

                foreach(string key in eventDetails.Keys)
                {
                    parameters += string.Format("[{0}] {1}\n", key, eventDetails[key]);
                }

                MKLog.Log("Event: " + eventKey + "\n"+parameters);
            }
        }

    }
}