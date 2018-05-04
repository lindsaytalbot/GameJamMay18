using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AnalyticsUtils
{
    public static string AppInfoJsonString
    {
        get
        {
            var appInfo = new Dictionary<string, string>()
            {
                {"app_version", Application.version},
                {"install_mode", Application.installMode.ToString()},
                {"installer_name", Application.installerName},
                {"sandbox_type", Application.sandboxType.ToString()}
            };

            return DictionaryToJsonString(appInfo);
        }
    }

    private static string DictionaryToJsonString(Dictionary<string, string> dictionary)
    {
        var kvs = dictionary.Select(kvp => string.Format("\"{0}\":\"{1}\"", kvp.Key, kvp.Value)).ToArray();
        return string.Concat("{", string.Join(",", kvs), "}");
    }
}