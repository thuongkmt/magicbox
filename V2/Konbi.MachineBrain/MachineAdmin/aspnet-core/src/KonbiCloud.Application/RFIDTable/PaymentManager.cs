using KonbiBrain.Common.Messages;
using KonbiBrain.Common.Messages.Payment;
using KonbiBrain.Messages;
using KonbiCloud.Common;
using KonbiCloud.Enums;
using Konbini.Messages.Enums;
using Newtonsoft.Json;
using NsqSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KonbiCloud.RFIDTable
{
    public class PaymentManager : IPaymentManager, IHandler
    {
        private readonly IMessageProducerService nsqProducerService;
        public  PaymentType PaymentType => PaymentType.MdbCashless; //TODO: remove hard code for payment type
        private readonly Consumer consumer;
        private CancellationTokenSource ctx;
        
        private NsqPaymentCommandBase Command { get; set; }
        private object commandLock = new object();

        public event EventHandler<CommandEventArgs> DeviceFeedBack;

        public PaymentManager(IMessageProducerService nsqProducerService)
        {
            this.nsqProducerService = nsqProducerService;
            //consumer = new Consumer(NsqTopics.PAYMENT_RESPONSE_TOPIC, NsqConstants.NsqDefaultChannel);
            //consumer.AddHandler(this);
            //consumer.ConnectToNsqLookupd(NsqConstants.NsqUrlConsumer);
        }
        public async Task<CommandState> ActivatePaymentAsync(TransactionInfo preTransaction)
        {
            ctx?.Cancel();
            lock (commandLock)
            {
                Command = new NsqEnablePaymentCommand(PaymentType.ToString(),

                 preTransaction.Id,
                 preTransaction.Amount
                 );
                // Command.CommandId = Guid.NewGuid();
                ///var sendingSuccess = await Task.Run(() => { return nsqProducerService.SendPaymentCommand(cmd); });
                var result = nsqProducerService.SendPaymentCommand(Command);
                if (result)
                {
                    Command.CommandState = CommandState.SendSuccess;
                }
                else
                {
                    Command.CommandState = CommandState.Failed;

                }

            }
            ctx = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {

                while (Command.CommandState == CommandState.SendSuccess && Command.Command == UniversalCommandConstants.EnablePaymentCommand && !ctx.Token.IsCancellationRequested)
                {
                    await Task.Delay(50, ctx.Token);
                }
                if (ctx.Token.IsCancellationRequested)
                    Command.CommandState = CommandState.Cancelled;

            }
            catch (OperationCanceledException)
            {
                Command.CommandState = CommandState.TimeOut;

            }

            return Command.CommandState;

        }

        public void HandleMessage(IMessage message)
        {
            try
            {

           
            // process for ACK message where  telling that the request  has been sent to device successfully.
            var msg = Encoding.UTF8.GetString(message.Body);
            var obj = JsonConvert.DeserializeObject<UniversalCommands>(msg);
            if (obj.Command == UniversalCommandConstants.PaymentACKCommand)
            {
                if (Command != null && Command.CommandId == obj.CommandId)
                {
                    Command.CommandState = CommandState.Received;
                }
            }
            else if (obj.Command == UniversalCommandConstants.EnablePaymentCommand)
            {
                OnDeviceFeedBack(new CommandEventArgs() { Command = JsonConvert.DeserializeObject<NsqEnablePaymentResponseCommand>(msg) });
            }
            else if (obj.Command == UniversalCommandConstants.PaymentDeviceResponse)
            {
                OnDeviceFeedBack(new CommandEventArgs() { Command = JsonConvert.DeserializeObject<NsqPaymentCallbackResponseCommand>(msg) });
            }
            }
            catch 
            {
                // dont catch unexpected message
            }



        }

        public void LogFailedMessage(IMessage message)
        {
            // throw new NotImplementedException();
        }
        protected void OnDeviceFeedBack(CommandEventArgs e)
        {
            DeviceFeedBack?.Invoke(this, e);
        }

        public Task CancelPaymentAsync(TransactionInfo transaction)
        {
            //TODO: 
            throw new NotImplementedException();
        }
    }
    public class CommandEventArgs : EventArgs
    {
        public IUniversalCommands Command { get; set; }
        public string CommandStr { get; set; }
    }
}
