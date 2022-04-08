using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using KonbiBrain.Messages;
using NsqSharp;

namespace KonbiCloud.BackgroundJobs
{
    public abstract class BaseNsqIncomingMessageService : PeriodicBackgroundWorkerBase, ISingletonDependency, IHandler, IDisposable
    {
        protected Consumer consumer;
        private readonly string nsqTopic;

        public BaseNsqIncomingMessageService(AbpTimer timer,string nsqTopic) : base(timer)
        {
            Timer.Period = 10000; //weekly
            Timer.RunOnStart = true;
            this.nsqTopic = nsqTopic;
            InitNsqConsumer();
        }


        private void InitNsqConsumer()
        {
            if (consumer == null)
            {
                consumer = new Consumer(nsqTopic, NsqConstants.NsqDefaultChannel);
                consumer.AddHandler(this);
                consumer.ConnectToNsqLookupd(NsqConstants.NsqUrlConsumer);
            }

        }
        protected override void DoWork()
        {
            if (consumer == null) InitNsqConsumer();
        }

        public async void HandleMessage(IMessage message)
        {
            //var msg = Encoding.UTF8.GetString(message.Body);
            //var obj = JsonConvert.DeserializeObject<UniversalCommands>(msg);
            await ProcessIncomingMessage(message);
        }

        protected abstract Task ProcessIncomingMessage(IMessage message);
       
        public void LogFailedMessage(IMessage message)
        {
            
        }

        public void Dispose()
        {
            consumer?.Stop();
        }
    }
}
