using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Common.Messages.Payment
{
    public class NsqPaymentACKResponseCommand : NsqPaymentCommandBase
    {

        public NsqPaymentACKResponseCommand(Guid commandId, string paymentDeviceType) : base(paymentDeviceType)
        {
            //Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            PaymentType = paymentDeviceType;
            Command = UniversalCommandConstants.PaymentACKCommand;
            CommandId = commandId;
        }
    }
}