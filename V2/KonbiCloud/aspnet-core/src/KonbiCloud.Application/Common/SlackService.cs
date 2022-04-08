using System;
using System.Threading.Tasks;
using Castle.Core.Logging;
using KonbiCloud.Configuration;
using Microsoft.Extensions.Options;
using Slack.Webhooks;

namespace KonbiCloud.Common
{
    public interface ISlackService
    {
        Task SendAlert(string machineName, string message, string channelName);
    }

    public class SlackService : KonbiCloudAppServiceBase, ISlackService
    {
        private SlackClient _slackClient = null;
        private readonly ILogger _logger;
        private readonly SlackOption _slackOption;


        public SlackService(ILogger logger, IOptions<SlackOption> option)
        {
            _logger = logger;
            _slackOption = option.Value;
        }

        public async Task SendAlert(string machineName, string message, string channelName)
        {
            if (_slackClient == null)
            {
                string hooUrlConfig = await SettingManager.GetSettingValueAsync(AppSettings.Slack.HookUrl);
                var hookUrl = hooUrlConfig == "" ? _slackOption.HookUrl : hooUrlConfig;
                _slackClient = new SlackClient(hookUrl);
            }

            try
            {
                var serverNameConfig = await SettingManager.GetSettingValueAsync(AppSettings.Slack.ServerName);
                var userNameConfig = await SettingManager.GetSettingValueAsync(AppSettings.Slack.UserName);

                var slackMessage = new SlackMessage
                {
                    Channel = channelName,
                    Text = "[" + machineName + "] : " + message,
                    Username = userNameConfig=="" ? _slackOption.UserName: userNameConfig
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
