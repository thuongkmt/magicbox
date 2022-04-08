using Konbini.RfidFridge.Domain.DTO;
using Konbini.RfidFridge.Domain.Enums;
using System;

namespace Konbini.RfidFridge.Service.Devices
{
    public interface IFridgePayment
    {
        bool Connect(string comport);
        void Start();
        void End(bool openLatch = true);
        void Charge(int amount, Action<TransactionStatus, object, CardPaymentType, string> callback = null);
        Action<PaymentType> OnValidateCardSuccess { get; set; }
        Action OnValidateCardFailed { get; set; }
        bool IsChargeFinished { get; set; }
        void OpenLatch();
        DateTime GetLastPolling();

        bool CheckDevice();

        object SendCommand(TerminalCommand cmd);
        void DisconnectHardware();
        void ReconnectHardware();

        void CustomerAction(CustomerAction action);

        void Refund(Action<bool> isSuccess);
    }
}
