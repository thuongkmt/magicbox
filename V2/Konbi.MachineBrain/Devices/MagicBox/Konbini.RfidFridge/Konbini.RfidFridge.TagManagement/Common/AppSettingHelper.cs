using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Konbini.RfidFridge.TagManagement.Common
{
   public class AppSettingHelper
    {
        public static string GetAppSetting(string key)
        {
            var value = string.Empty;
            try
            {
                value = System.Configuration.ConfigurationManager.AppSettings[key];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return value;
        }

        public static void SetAppSetting(string key, string value)
        {
            try
            {
                var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                configuration.AppSettings.Settings[key].Value = value;
                configuration.Save();
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
