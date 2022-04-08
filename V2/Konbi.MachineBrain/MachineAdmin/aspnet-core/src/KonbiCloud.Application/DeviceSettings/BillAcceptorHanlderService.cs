using KonbiCloud.Common;
using System;
using System.Collections.Generic;
using System.Text;
using KonbiBrain.Messages;

namespace KonbiCloud.DeviceSettings
{
    public class BillAcceptorHanlderService : IBillAcceptorHanlderService
    {
        private readonly IMessageProducerService _messageProducerService;
        public BillAcceptorHanlderService(IMessageProducerService messageProducerService)
        {
            _messageProducerService = messageProducerService;
        }
        public void Disable()
        {
            _messageProducerService.SendNsqCommand(NsqTopics.BILL_ACCEPTOR_TOPIC, "disable");
        }

        public void Enable()
        {
            _messageProducerService.SendNsqCommand(NsqTopics.BILL_ACCEPTOR_TOPIC, "enable");
        }

        public void Reset()
        {
            _messageProducerService.SendNsqCommand(NsqTopics.BILL_ACCEPTOR_TOPIC, "reset");
        }
    }
}
