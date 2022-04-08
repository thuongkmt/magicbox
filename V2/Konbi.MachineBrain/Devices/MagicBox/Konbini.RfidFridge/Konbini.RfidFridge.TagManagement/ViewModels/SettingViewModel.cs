using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using System.IO.Ports;
using Konbini.RfidFridge.TagManagement.Common;
using Konbini.RfidFridge.TagManagement.Data;
using Konbini.RfidFridge.TagManagement.Enums;

namespace Konbini.RfidFridge.TagManagement.ViewModels
{
    public class SettingViewModel : StateViewModel
    {
        private string cloudUrl;
        private string userName;
        private string password;
        private string rbmqServer;
        private string rbmqUser;
        private string rbmqPass;
        private string tenantId;

        public string CloudUrl
        {
            get => cloudUrl;
            set
            {
                cloudUrl = value;
                SaveConfig(SettingKey.CloudUrl, value);
            }
        }
        public string UserName
        {
            get => userName;
            set
            {
                userName = value;
                SaveConfig(SettingKey.UserName, value);
            }
        }
        public string Password
        {
            get => password;
            set
            {
                password = value;
                SaveConfig(SettingKey.Password, value);
            }
        }

        public string RbmqServer
        {
            get => rbmqServer; set
            {
                rbmqServer = value;
                SaveConfig(SettingKey.RbmqServer, value);
            }
        }
        public string RbmqUser
        {
            get => rbmqUser; set
            {
                rbmqUser = value;
                SaveConfig(SettingKey.RbmqUser, value);
            }
        }
        public string RbmqPass
        {
            get => rbmqPass; set
            {
                rbmqPass = value;
                SaveConfig(SettingKey.RbmqPass, value);
            }
        }
        public string TenantId
        {
            get => tenantId; set
            {
                tenantId = value;
                SaveConfig(SettingKey.TenantId, value);
            }
        }
        public SettingViewModel(IEventAggregator events) : base(events)
        {
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            LoadConfig();
        }

        private void LoadConfig()
        {
            using (var context = new KDbContext())
            {
                cloudUrl = context.Settings.SingleOrDefault(x => x.Key == SettingKey.CloudUrl)?.Value;
                password = context.Settings.SingleOrDefault(x => x.Key == SettingKey.Password)?.Value;
                userName = context.Settings.SingleOrDefault(x => x.Key == SettingKey.UserName)?.Value;
                rbmqServer = context.Settings.SingleOrDefault(x => x.Key == SettingKey.RbmqServer)?.Value;
                rbmqUser = context.Settings.SingleOrDefault(x => x.Key == SettingKey.RbmqUser)?.Value;
                rbmqPass = context.Settings.SingleOrDefault(x => x.Key == SettingKey.RbmqPass)?.Value;
                tenantId = context.Settings.SingleOrDefault(x => x.Key == SettingKey.TenantId)?.Value;
            }
        }

        private void SaveConfig(SettingKey key, string value)
        {
            using (var context = new KDbContext())
            {
                var config = context.Settings.SingleOrDefault(x => x.Key == key);
                if (config == null)
                {
                    context.Settings.Add(new Entities.Settings
                    {
                        Key = key,
                        Value = value
                    });
                }
                else
                {
                    config.Value = value;
                }

                context.SaveChanges();
            }
        }
    }
}

