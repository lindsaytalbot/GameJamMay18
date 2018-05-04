package universal.tools.notifications;

import android.os.Bundle;

import com.google.android.gms.gcm.GcmListenerService;

public class GcmIntentService extends GcmListenerService {
    @Override
    public void onMessageReceived(String from, Bundle data) {
        Manager.postPushNotification(getApplicationContext(), data);
    }
}
