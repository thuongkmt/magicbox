using System;
using System.Text;
using Abp.Dependency;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using KonbiCloud.Messages;
using KonbiCloud.SignalR;
using Newtonsoft.Json;
using NsqSharp;

namespace KonbiCloud.BackgroundJobs
{
    public class RfidTableNsqIncomingMessageService : PeriodicBackgroundWorkerBase, ISingletonDependency ,IHandler,IDisposable
    {
        private Consumer consumer;
        private readonly IMessageCommunicator messageCommunicator;

        public RfidTableNsqIncomingMessageService(AbpTimer timer, IMessageCommunicator messageCommunicator) : base(timer)
        {
            Timer.Period = 1000; //weekly
            Timer.RunOnStart = true;
            InitNsqConsumer();
            this.messageCommunicator = messageCommunicator;
        }

        protected override void DoWork()
        {
             if(consumer==null) InitNsqConsumer();
        }

        private void InitNsqConsumer()
        {
            consumer = new Consumer(NsqTopics.RFID_TABLE_TOPIC, NsqConstants.NsqDefaultChannel);
            consumer.AddHandler(this);
            consumer.ConnectToNsqLookupd(NsqConstants.NsqUrlConsumer);
        }

        public async void HandleMessage(IMessage message)
        {
            var msg = Encoding.UTF8.GetString(message.Body);
            //LogService.NsqCommandLog.Information("KonbiMain incoming: " + msg);
            var obj = JsonConvert.DeserializeObject<UniversalCommands>(msg);
            if (obj.CommandId == UniversalCommands.RFIDTable_DetectedDisc)
            {
                var mess = "{\"type\":\"RFIDTable_DetectedDisc\",\"data\":[";
                mess += obj.CommandObject;
                mess += "]}";
                await messageCommunicator.SendTestMessageToAllClient(new GeneralMessage { Message = mess});
            }
            else if (obj.CommandId == UniversalCommands.RFIDTable_RemovedDisc)
            {
                //send signalr message
            }
        }

        public void LogFailedMessage(IMessage message)
        {
           
        }

        public void Dispose()
        {
            consumer?.Stop();
        }
    }
}
