using System.Text;
using KonbiBrain.Common.Messages;
using MdbCashlessBrain.ViewModels;
using Newtonsoft.Json;
using NsqSharp;

namespace MdbCashlessBrain
{
    public class PaymentRequestMessageHandler : IHandler
    {
        private readonly ShellViewModel shellViewModel;
        public PaymentRequestMessageHandler(ShellViewModel hander)
        {
            shellViewModel = hander;
        }

        /// <summary>Handles a message.</summary>
        public void HandleMessage(IMessage message)
        {
            string msg = Encoding.UTF8.GetString(message.Body);
            shellViewModel.AppendNotification(msg);
            var obj = JsonConvert.DeserializeObject<UniversalCommands>(msg);
            if (obj.IsTimeout()) return;

            if (obj.Command== UniversalCommandConstants.PaymentRequest)
            {
                if ((bool) obj.CommandObject.IsEnabled)
                {
                    shellViewModel.MdbDevice.PaymentAmount = (double) obj.CommandObject.Value;
                    shellViewModel.MdbDevice.EnableReader();
                }
                else
                {
                    shellViewModel.MdbDevice.PaymentAmount = null;
                    shellViewModel.MdbDevice.DisableReader();
                }
            }
        }

        /// <summary>
        /// Called when a message has exceeded the specified <see cref="Config.MaxAttempts"/>.
        /// </summary>
        /// <param name="message">The failed message.</param>
        public void LogFailedMessage(IMessage message)
        {
            // Log failed messages
        }
    }
}