using Konbini.Messages.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Common.Messages.Payment
{
    public class NsqPaymentCallbackResponseCommand : NsqPaymentCommandBase
    {
        public NsqPaymentCallbackResponseCommand()
        {
            Response = new PaymentResponseData();
        }
        
        public PaymentResponseData Response { get; set; }
        public NsqPaymentCallbackResponseCommand(string paymentType) : base(paymentType)
        {
            Command = UniversalCommandConstants.PaymentDeviceResponse;
            Response = new PaymentResponseData();
        }
    }
    public class PaymentResponseData
    {
        public string Message { get; set; }
        public PaymentState State { get; set; }
        public dynamic ResponseObject { get; set; }
        public dynamic OtherInfo { get; set; }

    }
}
