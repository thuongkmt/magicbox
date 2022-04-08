using KonbiBrain.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Interfaces
{
    public interface IMessageProducerService : IDisposable
    {
        //void SendNsqCommand(string nsqTopic, object command);
        //void SendEzLinkCommand(object command);
        void SendToKonbiBrainCommand(object command);

        void SendVMCCommand(object command);
        void SendHopperPayoutCommand(object command);
        void SendMDBCommand(object command);
        void SendTemperatureCommand(object command);
        void SendNfcCommand(object command);
        void SendToCustomerUICommand(object command);
        void SendCloudSyncCommand(object command);
        bool SendRfidTableCommand(object command);
        bool SendRfidTableResponseCommand(object command);
        bool SendNsqCommand(string nsqTopic, object command);
        bool SendPaymentResponseCommand(IUniversalCommands command);
    }
}
