package universal.tools.notifications;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;

import org.json.JSONException;

public class ScheduledNotificationsRestorer extends BroadcastReceiver {
    private static final String RESTORE_SCHEDULED_NOTIFICATIONS = "RESTORE_SCHEDULED_NOTIFICATIONS";
    private static final String SCHEDULED_NOTIFICATIONS_STORED_PREFIX = "SCHEDULED_NOTIFICATIONS_STORED_";

    static void setRestoreScheduledOnReboot(Context context, boolean restoreScheduledOnReboot) {
        context = context.getApplicationContext();
        SharedPreferences prefs = context.getSharedPreferences(ScheduledNotificationsRestorer.class.getName(), Context.MODE_PRIVATE);
        SharedPreferences.Editor editor = prefs.edit();
        editor.putBoolean(RESTORE_SCHEDULED_NOTIFICATIONS, restoreScheduledOnReboot);
        editor.commit();
    }

    static boolean getRestoreScheduledOnReboot(final Context context) {
        return context.getApplicationContext().getSharedPreferences(ScheduledNotificationsRestorer.class.getName(), Context.MODE_PRIVATE).getBoolean(RESTORE_SCHEDULED_NOTIFICATIONS, false);
    }

    static void register(Context context, final UTNotification notification) {
        context = context.getApplicationContext();
        if (getRestoreScheduledOnReboot(context)) {
            final String notificationString = notification.toString();
            if (notificationString != null) {
                final SharedPreferences prefs = context.getSharedPreferences(ScheduledNotificationsRestorer.class.getName(), Context.MODE_PRIVATE);
                final SharedPreferences.Editor editor = prefs.edit();
                editor.putString(SCHEDULED_NOTIFICATIONS_STORED_PREFIX + notification.id, notificationString);
                editor.commit();
            }
        }
    }

    static void cancel(Context context, int id) {
        context = context.getApplicationContext();
        SharedPreferences prefs = context.getSharedPreferences(ScheduledNotificationsRestorer.class.getName(), Context.MODE_PRIVATE);
        SharedPreferences.Editor editor = prefs.edit();
        editor.remove(SCHEDULED_NOTIFICATIONS_STORED_PREFIX + id);
        editor.commit();
    }

    private static void restoreScheduledNotifications(Context context) {
        context = context.getApplicationContext();
        int[] ids = Manager.getStoredScheduledNotificationIds(context);

        if (ids != null && ids.length > 0) {
            SharedPreferences prefs = context.getSharedPreferences(ScheduledNotificationsRestorer.class.getName(), Context.MODE_PRIVATE);
            for (int id : ids) {
                String notificationJson = prefs.getString(SCHEDULED_NOTIFICATIONS_STORED_PREFIX + id, null);
                if (notificationJson != null && !notificationJson.isEmpty()) {
                    restoreScheduledNotification(context, notificationJson);
                }
            }
        }
    }

    private static void restoreScheduledNotification(final Context context, final String notificationJsonString) {
        try {
            Manager.scheduleNotificationCommon(context, new UTNotification(notificationJsonString));
        } catch (JSONException e) {
            e.printStackTrace();
        }
    }

    @Override
    public void onReceive(Context context, Intent intent) {
        if ("android.intent.action.BOOT_COMPLETED".equals(intent.getAction())) {
            restoreScheduledNotifications(context);
        }
    }
}
