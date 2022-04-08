using System.Text;
using KonbiBrain.Common.Messages;
using KonbiBrain.Messages;
using Newtonsoft.Json;
using TemplateBrain.ViewModels;
using NsqSharp;

namespace TemplateBrain
{
    public class MessageHandler : IHandler
    {
        private readonly ShellViewModel shellViewModel;
        public MessageHandler(ShellViewModel hander)
        {
            shellViewModel = hander;
        }

        /// <summary>Handles a message.</summary>
        public void HandleMessage(IMessage message)
        {
            string msg = Encoding.UTF8.GetString(message.Body);
            //shellViewModel.AppendNotification(msg);
            var obj = JsonConvert.DeserializeObject<BaseCommand>(msg);
            if (obj.IsTimeout()) return;
            if (obj.Command == CommunicationCommands.Temperature_SetTemperature)
            {
                var data = JsonConvert.DeserializeObject<TemperatureCommands.SetTemperature>(msg);
                shellViewModel.SetTemperature(data.Temperature);            
            }else if(obj.Command == CommunicationCommands.Temperature_GetTemperature)
            {
                shellViewModel.GetCurrentTemperature();
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