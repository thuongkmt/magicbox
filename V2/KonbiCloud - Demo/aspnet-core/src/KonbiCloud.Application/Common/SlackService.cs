using System;
using Castle.Core.Logging;
using KonbiCloud.Configuration;
using Microsoft.Extensions.Options;
using Slack.Webhooks;

namespace KonbiCloud.Common
{
    public interface ISlackService
    {
        void SendLenovoAlert(string machineName, string message);
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

        public void SendLenovoAlert(string machineName, string message)
        {
            if (_slackClient == null)
            {
                _slackClient = new SlackClient(_slackOption.HookUrl);
                //_slackClient = new SlackClient("https://hooks.slack.com/services/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");
            }

            try
            {

                var slackMessage = new SlackMessage
                {
                    Channel = _slackOption.ChannelName,
                    Text = "[" + machineName + "-" + _slackOption.ServerName + "] : " + message,
                    Username = _slackOption.UserName,
                    Mrkdwn = true
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
