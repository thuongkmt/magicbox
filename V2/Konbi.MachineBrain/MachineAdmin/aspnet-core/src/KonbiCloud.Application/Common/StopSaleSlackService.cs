using Abp.Configuration;
using Castle.Core.Logging;
using KonbiCloud.Configuration;
using Microsoft.Extensions.Options;
using Slack.Webhooks;
using System;

namespace KonbiCloud.Common
{
    public interface IStopSaleSlackService
    {
        void SendAlert(string machineName, string message);
    }

    public class StopSaleSlackService : KonbiCloudAppServiceBase, IStopSaleSlackService
    {
        private SlackClient _slackClient = null;
        private readonly ILogger _logger;
        private readonly SlackOption _slackOption;

        public StopSaleSlackService(ILogger logger, IOptions<SlackOption> option)
        {
            _logger = logger;
            _slackOption = option.Value;
        }

        public void SendAlert(string machineName, string message)
        {
            if (_slackClient == null)
            {
                var hookUrl = String.IsNullOrEmpty(SettingManager.GetSettingValue(AppSettingNames.SlackUrl)) ? _slackOption.HookUrl : SettingManager.GetSettingValue(AppSettingNames.SlackUrl);
                _slackClient = new SlackClient(hookUrl);
            }
            try
            {
                var channel = String.IsNullOrEmpty(SettingManager.GetSettingValue(AppSettingNames.AlertChannel)) ? _slackOption.ChannelName : SettingManager.GetSettingValue(AppSettingNames.AlertChannel);
                var serverName = String.IsNullOrEmpty(SettingManager.GetSettingValue(AppSettingNames.ServerName)) ? _slackOption.ServerName : SettingManager.GetSettingValue(AppSettingNames.ServerName); 
                var userName = String.IsNullOrEmpty(SettingManager.GetSettingValue(AppSettingNames.Username)) ? _slackOption.UserName : SettingManager.GetSettingValue(AppSettingNames.Username); 

                var slackMessage = new SlackMessage
                {
                    Channel = channel,
                    Text = "[" + machineName + "-" + serverName + "] : " + message,
                    Username = userName,
                    Markdown = true
                };

                _slackClient.Post(slackMessage);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
            }
        }
    }
}
