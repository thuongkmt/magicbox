using System;
using System.Text;
using KonbiBrain.Common.Messages;
using Newtonsoft.Json;
using NsqSharp;
using KonbiBrain.Common.Messages.Payment;

namespace MdbCashlessBrain
{
    public class PaymentRequestMessageHandler : IHandler
    {
        private readonly MdbProcessingService mdbProcessingService;
        public PaymentRequestMessageHandler(MdbProcessingService _mdbProcessingService)
        {
            this.mdbProcessingService = _mdbProcessingService;
        }

        /// <summary>Handles a message.</summary>
        public void HandleMessage(IMessage message)
        {
            string msg = Encoding.UTF8.GetString(message.Body);
            Console.WriteLine(msg);
            var obj = JsonConvert.DeserializeObject<UniversalCommands>(msg);
            if (obj.IsTimeout()) return;

            if (obj.Command== UniversalCommandConstants.EnablePaymentCommand)
            {
                //if ((bool) obj.CommandObject.IsEnabled)
                //{
                //    mdbProcessingService.PaymentAmount = (double) obj.CommandObject.Value;
                //    mdbProcessingService.EnableReader();
                //}
                //else
                //{
                //    mdbProcessingService.PaymentAmount = null;
                //    mdbProcessingService.DisableReader();
                //}
                var data = JsonConvert.DeserializeObject<NsqEnablePaymentCommand>(msg);

                mdbProcessingService.PaymentAmount = data.Amount;
                mdbProcessingService.TransactionId = data.TransactionId;
                mdbProcessingService.CommandId = data.CommandId;
                mdbProcessingService.EnableReader();
            }
            else if(obj.Command==UniversalCommandConstants.DisablePaymentCommand)
            {
                mdbProcessingService.PaymentAmount = null;
                mdbProcessingService.TransactionId = null;
                mdbProcessingService.CommandId = null;
                mdbProcessingService.DisableReader();
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