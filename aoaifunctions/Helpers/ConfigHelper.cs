using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aoaifunctions.Helpers
{
    public class ConfigHelper
    {
        public static string GetConfigSetting(string key, string defaultSetting = null)
        {
            var settingValue = Environment.GetEnvironmentVariable(key);
            if (String.IsNullOrEmpty(settingValue))
            {
                return defaultSetting;
            }
            else
            {
                return settingValue;
            }
        }
    }
}
