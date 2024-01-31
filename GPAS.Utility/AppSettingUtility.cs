using System.Configuration;

namespace GPAS.Utility
{
    public class AppSettingUtility
    {
        public static string GetValueFromAppSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}


