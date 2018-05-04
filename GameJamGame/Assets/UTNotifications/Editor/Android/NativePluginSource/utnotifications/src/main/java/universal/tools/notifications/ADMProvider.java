package universal.tools.notifications;

import android.content.Context;
import android.util.Log;

import com.amazon.device.messaging.ADM;

public class ADMProvider implements IPushNotificationsProvider {
    public static final String Name = "ADM";
    private final Context context;

    public ADMProvider(Context context) {
        this.context = context;
    }

    public static boolean isAvailable() {
        try {
            Class.forName("com.amazon.device.messaging.ADM");
            return true;
        } catch (ClassNotFoundException e) {
            return false;
        }
    }

    @Override
    public void enable() {
        try {
            final ADM adm = new ADM(context);

            String registrationId = adm.getRegistrationId();
            if (registrationId == null) {
                adm.startRegister();
            } else {
                Manager.onRegistered(Name, registrationId);
            }
        } catch (SecurityException ex) {
            Log.e(ADMProvider.class.getName(), "Unable to register ADM: " + ex.getMessage());
        }
    }

    @Override
    public void disable() {
        final ADM adm = new ADM(context);
        adm.startUnregister();
    }
}
