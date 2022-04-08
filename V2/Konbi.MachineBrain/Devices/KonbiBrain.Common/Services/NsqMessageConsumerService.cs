using KonbiBrain.Messages;
using NsqSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Common.Services
{

    public class NsqMessageConsumerService
    {
        public LogService LogService { get; set; }
        private readonly Consumer consumer;
        public NsqMessageConsumerService(string topic, INsqHandler handler)
        {
            consumer = new Consumer(topic, NsqConstants.NsqDefaultChannel);
            consumer.AddHandler(handler);
            consumer.ConnectToNsqLookupd(NsqConstants.NsqUrlConsumer);
           
        }

    }
    public interface INsqHandler : IHandler { }
}
