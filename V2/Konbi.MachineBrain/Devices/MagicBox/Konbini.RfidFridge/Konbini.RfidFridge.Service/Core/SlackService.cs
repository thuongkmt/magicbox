using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slack.Webhooks;
using Konbini.RfidFridge.Service.Helper;
using Microsoft.Extensions.Options;

namespace Konbini.RfidFridge.Service.Core
{
    public class SlackService
    {
        private SlackClient _slackClient = null;
        private readonly LogService _logger;

        private string InfoChannel;
        private string AlertChannel;
        private string Username;
        private string HookUrl;
        private string UnstableTagChannel;

        public SlackService(LogService logger)
        {
            _logger = logger;
        }

        public void Init(string infoChannel, string alertChannel, string unstableChannel, string username, string hookUrl)
        {
            this.InfoChannel = infoChannel;
            this.AlertChannel = alertChannel;
            this.Username = username;
            this.HookUrl = hookUrl;
            this.UnstableTagChannel = unstableChannel;
        }

        public void SendInfo(string machineName, string message)
        {
            
            Task.Run(() =>
            {
                if (string.IsNullOrEmpty(HookUrl))
                {
                    return;
                }

                if (_slackClient == null)
                {
                    _slackClient = new SlackClient(HookUrl);
                }
                try
                {
                    var slackMessage = new SlackMessage
                    {
                        Channel = this.InfoChannel,
                        Text = "[" + machineName + "] : " + message,
                        Username = Username,
                    };
                    _slackClient.Post(slackMessage);
                }
                catch (Exception e)
                {
                    _logger.LogError(e);
                }
            });
        }

        public void SendAlert(string machineName, string message)
        {

            Task.Run(() =>
            {
                if (string.IsNullOrEmpty(HookUrl))
                {
                    return;
                }
                if (_slackClient == null)
                {
                    _slackClient = new SlackClient(HookUrl);
                }
                try
                {
                    var slackMessage = new SlackMessage
                    {
                        Channel = this.AlertChannel,
                        Text = "[" + machineName + "] : " + message,
                        Username = Username,
                    };
                    _slackClient.Post(slackMessage);
                }
                catch (Exception e)
                {
                    _logger.LogError(e);
                }
            });
        }

        public void SendUnstableTagAlert(string machineName, string message)
        {

            Task.Run(() =>
            {
                if (string.IsNullOrEmpty(HookUrl))
                {
                    return;
                }
                if (_slackClient == null)
                {
                    _slackClient = new SlackClient(HookUrl);
                }
                try
                {
                    var slackMessage = new SlackMessage
                    {
                        Channel = this.UnstableTagChannel,
                        Text = "[" + machineName + "] : " + message,
                        Username = Username,
                    };
                    _slackClient.Post(slackMessage);
                }
                catch (Exception e)
                {
                    _logger.LogError(e);
                }
            });
        }
    }
}
