using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StandardEventNames
{
    //Application
    public const string SCREEN_VISIT = "screen_visit";
    public const string CUTSCENE_SKIP = "cutscene_skip";

    //Progression
    public const string GAME_START = "game_start";
    public const string GAME_OVER = "game_over";
    public const string LEVEL_START = "level_start";
    public const string LEVEL_COMPLETE = "level_complete";
    public const string LEVEL_QUIT = "level_quit";
    public const string LEVEL_SKIP = "level_skip";
    public const string LEVEL_UP = "level_up";
    public const string LEVEL_FAIL = "level_fail";

    //Onboarding
    public const string FIRST_INTERACTION = "first_interaction";
    public const string TUTORIAL_START = "tutorial_start";
    public const string TUTORIAL_STEP = "tutorial_step";
    public const string TUTORIAL_COMPLETE = "tutorial_complete";
    public const string TUTORIAL_SKIP = "tutorial_skip";

    //Engagement and Social
    public const string PUSH_NOTIFICATION_ENABLE = "push_notifcation_enable";
    public const string PUSH_NOTIFICATION_CLICK = "push_notification_click";
    public const string CHAT_MESSAGE_SENT = "chat_msg_snet";
    public const string ACHIEVEMENT_UNLOCK = "achievement_unlock";
    public const string ACHIEVEMENT_STEP = "achievement_step";
    public const string USER_SIGNUP = "user_signup";
    public const string SOCIAL_SHARE = "social_share";

    //Monetization
    public const string STORE_OPENED = "store_opened";
    public const string STORE_ITEM_CLICK = "store_item_click";
    public const string CURRENCY_ACQUIRED = "currency_acquired";
    public const string CONSUMABLE_ACQUIRED = "consumable_acquired";
    public const string CONSUMABLE_SPENT = "consumable_spent";
    public const string ITEM_ACQUIRED = "item_acquired";
    public const string AD_OFFER = "ad_offer";
    public const string AD_START = "ad_start";
    public const string AD_COMPLETE = "ad_complete";
    public const string AD_SKIP = "ad_skip";
    public const string POST_AD_ACTION = "post_ad_action";

    public static bool IsStandardEvent(string eventName)
    {
        List<string> events = new List<string>
        {
            SCREEN_VISIT,
            CUTSCENE_SKIP,
            GAME_START,
            GAME_OVER,
            LEVEL_START,
            LEVEL_COMPLETE,
            LEVEL_QUIT,
            LEVEL_SKIP,
            LEVEL_UP,
            LEVEL_FAIL,
            FIRST_INTERACTION,
            TUTORIAL_START,
            TUTORIAL_STEP,
            TUTORIAL_COMPLETE,
            TUTORIAL_SKIP,
            PUSH_NOTIFICATION_ENABLE,
            PUSH_NOTIFICATION_CLICK,
            CHAT_MESSAGE_SENT,
            ACHIEVEMENT_UNLOCK,
            ACHIEVEMENT_STEP,
            USER_SIGNUP,
            SOCIAL_SHARE,
            STORE_OPENED,
            STORE_ITEM_CLICK,
            CURRENCY_ACQUIRED,
            CONSUMABLE_ACQUIRED,
            CONSUMABLE_SPENT,
            ITEM_ACQUIRED,
            AD_OFFER,
            AD_START,
            AD_COMPLETE,
            AD_SKIP,
            POST_AD_ACTION,
        };

        return events.Contains(eventName);
    }

    public static class Parameters
    {
        public const string SCENE_NAME = "scene_name";
        public const string SCREEN_NAME = "screen_name";

        public const string LEVEL_NAME = "level_name";
        public const string LEVEL_INDEX = "level_index";

        public const string TYPE = "type";
        public const string SOURCE = "source";

        public const string ITEM_NAME = "item_name";
        public const string ITEM_ID = "item_id";
        public const string RESOURCE_TYPE = "resource_type";

        public const string CONSUMABLE_NAME = "consumable_name";
        public const string CONSUMABLE_AMOUNT = "consumable_amount";
        public const string CONSUMABLE_BALANCE = "consumable_balance";
        public const string CONSUMABLE_ITEM_PURCHASED = "item_purchased";

        public const string CURRENCY_NAME = "currency_name";
        public const string CURRENCY_AMOUNT = "currency_amount";
        public const string CURRENCY_BALANCE = "currency_balance";

        public const string PURCHASE_ID = "purchase_id"; // Optional parameter
        public const string PURCHASE_NAME = "purchase_name"; // Optional parameter
        public const string PURCHASE_QUANTITY = "puchase_qty"; // Optional parameter

        public const string TUTORIAL_ID = "tutorial_id";
        public const string ACHIEVEMENT_ID = "achievement_id";
        public const string STEP_INDEX = "step_index";

        public const string OLD_LEVEL_INDEX = "old_level_index";
        public const string NEW_LEVEL_INDEX = "new_level_index";
        public const string OLD_LEVEL_NAME = "old_level_name";
        public const string NEW_LEVEL_NAME = "new_level_name";

        public const string AD_PLACEMENT_ID = "placement_id";
        public const string AD_NETWORK_ID = "network";
        public const string AD_REWARDED = "rewarded";

        public const string SHARE_TYPE = "share_type";
        public const string SOCIAL_NETWORK = "social_network";
        public const string AUTHORIZATION_NETWORK = "authorization_network";

        public const string ACTION_ID = "action_id";
        public const string MESSAGE_ID = "message_id";

    }
}
