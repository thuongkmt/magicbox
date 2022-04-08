using System;
using Abp.Authorization;
using KonbiBrain.Messages;
using KonbiCloud.Common;
using Konbini.Messages.Commands;

namespace KonbiCloud.Payments
{
    [AbpAllowAnonymous]
    public class PaymentDeviceService : KonbiCloudAppServiceBase, IPaymentDeviceService
    {
        private readonly IMessageProducerService messageProducerService;

        public PaymentDeviceService(IMessageProducerService messageProducerService)
        {
            this.messageProducerService = messageProducerService;
        }
        
        public bool EnablePayments()
        {
            var tranCode = Guid.NewGuid();
            //todo: save current transcode to setting
            return messageProducerService.SendNsqCommand(NsqTopics.PAYMENT_REQUEST_TOPIC,new PaymentRequestCommand{CommandObject = new {IsEnabled = true,Value=0.01}});            
        }

        public void DisablePayments()
        {
            messageProducerService.SendNsqCommand(NsqTopics.PAYMENT_REQUEST_TOPIC, new PaymentRequestCommand { CommandObject = new { IsEnabled = false } });
        }
    }
}
