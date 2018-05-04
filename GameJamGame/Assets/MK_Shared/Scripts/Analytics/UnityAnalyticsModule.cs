using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Analytics.Experimental;

namespace MightyKingdom
{
    [CreateAssetMenu(fileName = "UnityModule", menuName = "MightyKingdom/Analytics/Unity")]
    public class UnityAnalyticsModule : MKAnalyticsModule
    {
        public override void InitSession()
        {
            //Configured in editor
        }

        public override void ReportEvent(string eventName)
        {
            //Doesn't double up with standard events
            if(StandardEventNames.IsStandardEvent(eventName) == false)
                Analytics.CustomEvent(eventName);
        }

        public override void ReportEvent(string eventName, Dictionary<string, object> eventDetails)
        {
            //Doesn't double up with standard events
            if (StandardEventNames.IsStandardEvent(eventName) == false)
                Analytics.CustomEvent(eventName, eventDetails);
        }
    }
}