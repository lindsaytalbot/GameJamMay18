package universal.tools.notifications;

import android.content.Context;
import android.content.Intent;

import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.GoogleApiAvailability;
import com.unity3d.player.UnityPlayer;

public class FCMProvider implements IPushNotificationsProvider {
    public static final String Name = "FCM";
    private final Context context;
    private final String firebaseSenderID;

    // Provide Application Context here
    public FCMProvider(Context context, String firebaseSenderID) {
        this.context = context;
        this.firebaseSenderID = firebaseSenderID;
    }

    public static boolean isAvailable(Context context, boolean allowUpdatingGooglePlayIfRequired) {
        try {
            final GoogleApiAvailability googleApiAvailability = GoogleApiAvailability.getInstance();

            final int result = googleApiAvailability.isGooglePlayServicesAvailable(context);
            if (result != ConnectionResult.SUCCESS && allowUpdatingGooglePlayIfRequired) {
                if (googleApiAvailability.isUserResolvableError(result) && result != ConnectionResult.SERVICE_MISSING && result != ConnectionResult.SERVICE_INVALID) {
                    googleApiAvailability.getErrorDialog(UnityPlayer.currentActivity, result, 0).show();
                }

                // May get available on next run as a result.
                return false;
            }

            return true;
        } catch (Throwable e) {
            return false;
        }
    }

    @Override
    public void enable() {
        startGcmInstanceIDListenerService(firebaseSenderID);
    }

    @Override
    public void disable() {
        startGcmInstanceIDListenerService(GcmInstanceIDListenerService.UNREGISTER);
    }

    private void startGcmInstanceIDListenerService(String value) {
        Intent intent = new Intent(context, GcmInstanceIDListenerService.class);
        intent.putExtra(GcmInstanceIDListenerService.SENDER_ID, value);
        context.startService(intent);
    }
}
