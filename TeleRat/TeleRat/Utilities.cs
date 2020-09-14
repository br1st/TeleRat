using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Botnet
{
    class Utilities
    {
        private static Dictionary<string, string> alerts;

        static Utilities()
        {
            string json = File.ReadAllText(config.appData + "\\Bot\\Config.json");                       // config.appData + "\\Bot\\Config.json"
            var data = JsonConvert.DeserializeObject<dynamic>(json);
            alerts = data.ToObject<Dictionary<string, string>>();
        }

        public static string GetData(string key)
        {
            if (alerts.ContainsKey(key))
                return alerts[key];

            return null;
        }

        public static string GetFormattedData(string key, object parameter)
        {
            return GetFormattedData(key, new object[] { parameter });
        }

        public static string GetFormattedData(string key, params object[] parameter)
        {
            if (alerts.ContainsKey(key))
                return string.Format(alerts[key], parameter);

            return null;
        }
    }
}
