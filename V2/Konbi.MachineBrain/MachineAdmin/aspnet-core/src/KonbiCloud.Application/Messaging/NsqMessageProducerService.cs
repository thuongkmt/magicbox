using System;
using System.Text;
using Castle.Core.Logging;
using KonbiBrain.Common.Messages;
using KonbiBrain.Messages;
using KonbiCloud.Common;
using NsqSharp;

namespace KonbiCloud.Messaging
{

    public class NsqMessageProducerService : IMessageProducerService
    {
        private readonly Producer producer;
        private readonly ILogger logger;

        public NsqMessageProducerService(ILogger logger)
        {
            producer = new Producer(NsqConstants.NsqUrlProducer);
            this.logger = logger;
        }
        public void SendEzLinkCommand(object command)
        {
            SendNsqCommand(NsqTopics.EZLINK_MESSAGE_TOPIC, command);
        }

        public void SendHopperPayoutCommand(object command)
        {
            SendNsqCommand(NsqTopics.HOPPERPAYOUT_MESSAGE_TOPIC, command);
        }

        public void SendMDBCommand(object command)
        {
            SendNsqCommand(NsqTopics.MDB_MESSAGE_TOPIC, command);
        }

        public void SendNfcCommand(object command)
        {
            SendNsqCommand(NsqTopics.NFC_MESSAGE_TOPIC, command);
        }

        public void SendTemperatureCommand(object command)
        {
            SendNsqCommand(NsqTopics.TEMPERATURE_MESSAGE_TOPIC, command);
        }

        public void SendToKonbiBrainCommand(object command)
        {
            SendNsqCommand(NsqTopics.KONBI_MAIN_TOPIC, command);
        }

        public void SendToCustomerUICommand(object command)
        {
            SendNsqCommand(NsqTopics.KONBI_CUSTOMER_UI_TOPIC, command);
        }

        public void SendVMCCommand(object command)
        {
            SendNsqCommand(NsqTopics.VMC_MESSAGE_TOPIC, command);
        }
        public void SendCloudSyncCommand(object command)
        {
            SendNsqCommand(NsqTopics.KONBI_CLOUDSYNC_TOPIC, command);
        }

        public bool SendRfidTableCommand(object command)
        {
            return SendNsqCommand(NsqTopics.RFID_TABLE_REQUEST_TOPIC, command);
        }
        public bool SendRfidTableResponseCommand(object command)
        {
            return SendNsqCommand(NsqTopics.RFID_TABLE_RESPONSE_TOPIC, command);
        }
        public bool SendNsqCommand(string nsqTopic, object command)
        {
            try
            {
                var commandStr = Newtonsoft.Json.JsonConvert.SerializeObject(command);
                producer.Publish(nsqTopic, Encoding.UTF8.GetBytes(commandStr));
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("SendNsqCommand Error",ex);
                return false;
            }
        }

        public void Dispose()
        {
            producer?.Stop();
        }
        /// <summary>
        /// send request to payment devices. 
        /// </summary>
        /// <param name="command"></param>
        public bool SendPaymentCommand(IUniversalCommands command)
        {
            if (command.CommandId == Guid.Empty)
                command.CommandId = Guid.NewGuid();
            return SendNsqCommand(NsqTopics.PAYMENT_REQUEST_TOPIC, command);
        }
    }
}
