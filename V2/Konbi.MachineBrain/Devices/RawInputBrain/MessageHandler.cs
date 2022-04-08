using System.Configuration;
using System.Text;
using KonbiBrain.Messages;
using RawInputBrain.ViewModels;
using Newtonsoft.Json;
using NsqSharp;

namespace RawInputBrain
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
            //string msg = Encoding.UTF8.GetString(message.Body);
            //shellViewModel.AppendNotification(msg);
            //var obj = JsonConvert.DeserializeObject<BaseCommand>(msg);
            //if (obj.IsTimeout()) return;
           
            //if (obj.Command == CommunicationCommands.IUC_EndTransaction)
            //{
            //    shellViewModel.IucDevice.EndTransaction();
            //}
            //if (obj.Command == CommunicationCommands.IUC_CancelTransaction)
            //{
            //    shellViewModel.IucDevice.CancelCommand();
            //}
            //if (obj.Command == CommunicationCommands.IUC_SendPurchase)
            //{
            //    var data = JsonConvert.DeserializeObject<MdbCommands.SendPurchase>(msg);
            //    shellViewModel.IucDevice.SendPurchase(data.Amount);
            //}
            //if (obj.Command == CommunicationCommands.IUC_Reset)
            //{
            //    var resetOnNewTran = false;
            //    if(bool.TryParse(ConfigurationManager.AppSettings["ResetOnNewTran"],out  resetOnNewTran))
            //        shellViewModel.IucDevice.InitIuc();
            //}
            ////if (obj.Command == CommunicationCommands.IUC_DisableReader)
            ////{
            ////    shellViewModel.IucDevice.DisableReader();
            ////}

            ////if (obj.Command == CommunicationCommands.IUC_EnableReader)
            ////{
            ////    shellViewModel.IucDevice.EnableReader();
            ////}
            //if (obj.Command == CommunicationCommands.IUC_SendCancellation) shellViewModel.IucDevice.DisableReader();

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