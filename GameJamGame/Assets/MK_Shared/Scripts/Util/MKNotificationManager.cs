using MightyKingdom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MKNotificationManager : Singleton<MKNotificationManager>
{
    private const string NOTIFICATIONS_SAVE_KEY = "MK_NotificationsEnabled";
    private const string NOTIFICATIONS_PROMPT_KEY = "MK_NotificationsPrompt";
    private static bool notificationsEnabled;
    private static bool initializedNotifications;
    private static bool hasShownPermissionPrompt;
    private static Dictionary<int, PendingNotification> scheduledNotifications = new Dictionary<int, PendingNotification>();

    public static bool NotificationsEnabled
    {
        get
        {
            return notificationsEnabled;
        }

        set
        {
            MKPlayerPrefs.SetBool(NOTIFICATIONS_SAVE_KEY, value);
            notificationsEnabled = value;

            if (initializedNotifications)
            {
                if (value)
                    RescheduleNotifications(); //Reschedule all notifications
                else
                    UTNotifications.Manager.Instance.CancelAllNotifications(); //Clear all pending notifications
            }
            else
            {
                Init();
            }
        }
    }

    //Always true on Android. True on iOS after calling ShowNotificationPrompt()
    public static bool HasShownPermissionPrompt
    {
        get
        {
            return hasShownPermissionPrompt;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        MKPlayerPrefs.Init();
        notificationsEnabled = MKPlayerPrefs.GetBool(NOTIFICATIONS_SAVE_KEY, true);
        hasShownPermissionPrompt = MKPlayerPrefs.GetBool(NOTIFICATIONS_PROMPT_KEY, Application.platform == RuntimePlatform.Android);

        //Android doesn't need permission
        if (hasShownPermissionPrompt)
            InitNotifications();
    }

    //Initialises notifications. This causes a permission prompt on iOS and should be shown after a dialog
    private static void InitNotifications()
    {
        if (!NotificationsEnabled || initializedNotifications)
            return;

        UTNotifications.Manager.Instance.Initialize(false);
        initializedNotifications = true;

        RescheduleNotifications();
    }


    //Called after user has been shown a message explaning that notifications are about to be enabled
    public static void ShowNotificationPrompt()
    {
        MKPlayerPrefs.SetBool(NOTIFICATIONS_PROMPT_KEY, true);
        hasShownPermissionPrompt = true;
        InitNotifications();
    }

    //Reschedules any notifications that were cancelled or were called before initialisation was completed 
    private static void RescheduleNotifications()
    {
        foreach (int key in scheduledNotifications.Keys.ToList())
        {
            PendingNotification notification = scheduledNotifications[key];

            if (notification.isRepeating)
                ScheduleNotificationRepeating(notification.date, notification.repeatRate, notification.title, notification.body, notification.index);
            else
                ScheduleNotification(notification.date, notification.title, notification.body, notification.index);
        }
    }

    /// <summary>
    /// Schedules a notification. If notifications are not enabled or not initialized the notification will be saved for later
    /// </summary>
    /// <param name="date">The date to schedule the notification</param>
    /// <param name="title">The title of the notification</param>
    /// <param name="body">The body text of the notification</param>
    /// <param name="index">The reference ID of this notification. A duplicate ID will override the previous entry</param>
    /// <param name="badgeNumber">The number that will appear next to the notification</param>
    /// <param name="notificationProfile">The profile to use. Configure in UTNotification settings</param>
    public static void ScheduleNotification(DateTime date, string title, string body, int index, int badgeNumber = -1, string notificationProfile = null)
    {
        //Records the notification in case notifications are Disabled and then Renabled
        scheduledNotifications[index] =
            new PendingNotification
            {
                date = date,
                title = title,
                body = body,
                index = index,
                isRepeating = false
            };

        if (!notificationsEnabled)
            return;

        if (!initializedNotifications)
        {
            MKLog.Log("InitNotifications() not called yet. " + title + ": " + body + "\n()");
            return;
        }

        UTNotifications.Manager.Instance.ScheduleNotification(date, title, body, index, badgeNumber: badgeNumber, notificationProfile: notificationProfile);
    }

    /// <summary>
    /// Schedules a notification. If notifications are not enabled or not initialized the notification will be saved for later
    /// </summary>
    /// <param name="date">The date to schedule the notification</param>
    /// <param name="repeatRate">How often to repeat this notification after the initial date</param>
    /// <param name="title">The title of the notification</param>
    /// <param name="body">The body text of the notification</param>
    /// <param name="index">The reference ID of this notification. A duplicate ID will override the previous entry</param>
    /// <param name="badgeNumber">The number that will appear next to the notification</param>
    /// <param name="notificationProfile">The profile to use. Configure in UTNotification settings</param>
    public static void ScheduleNotificationRepeating(DateTime date, TimeSpan repeatRate, string title, string body, int index, int badgeNumber = -1, string notificationProfile = null)
    {
        //Records the notification in case notifications are Disabled and then Renabled
        scheduledNotifications[index] =
            new PendingNotification
            {
                date = date,
                title = title,
                body = body,
                index = index,
                isRepeating = false,
                repeatRate = repeatRate
            };

        if (!notificationsEnabled)
            return;

        if (!initializedNotifications)
        {
            MKLog.Log("InitNotifications() not called yet. " + title + ": " + body + "\n()");
            return;
        }

        UTNotifications.Manager.Instance.ScheduleNotificationRepeating(date, (int)repeatRate.TotalSeconds, title, body, index, badgeNumber: badgeNumber, notificationProfile: notificationProfile);
    }

    public static void CancelNotification(int index)
    {
        if (scheduledNotifications.ContainsKey(index))
            scheduledNotifications.Remove(index);

        if (initializedNotifications)
            UTNotifications.Manager.Instance.CancelNotification(index);
    }

    private struct PendingNotification
    {
        public DateTime date;
        public string title;
        public string body;
        public int index;
        public bool isRepeating;
        public TimeSpan repeatRate;
    }
}
