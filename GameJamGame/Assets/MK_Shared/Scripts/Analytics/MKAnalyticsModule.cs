using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MightyKingdom
{
    public abstract class MKAnalyticsModule : ScriptableObject
    {
        public abstract void InitSession();
        public abstract void ReportEvent(string eventName);
        public abstract void ReportEvent(string eventName, Dictionary<string, object> eventDetails);
    }
}
