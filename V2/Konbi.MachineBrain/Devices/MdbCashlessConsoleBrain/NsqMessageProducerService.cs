using System;
using System.Text;
using KonbiBrain.Common.Messages;
using KonbiBrain.Interfaces;
using KonbiBrain.Messages;
using NsqSharp;

namespace KonbiBrain.Common.Services
{
    public class NsqMessageProducerService:IMessageProducerService
    {
        public LogService LogService { get; set; }
        private readonly Producer producer;

        public NsqMessageProducerService()
        {
            producer = new Producer(NsqConstants.NsqUrlProducer);
        }
        public void SendEzLinkCommand(object command)
        {
            SendNsqCommand(NsqTopics.EZLINK_MESSAGE_TOPIC,command);
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
                Console.WriteLine(ex);
               // MessageBox.Show(ex.Message);
                return false;
            }
        }

        public void Dispose()
        {
            producer?.Stop();
        }

       

        public bool SendPaymentResponseCommand(IUniversalCommands command)
        {
            if (command.CommandId == Guid.Empty)
                command.CommandId = Guid.NewGuid();
            return SendNsqCommand(NsqTopics.PAYMENT_RESPONSE_TOPIC, command);
        }
    }
}
