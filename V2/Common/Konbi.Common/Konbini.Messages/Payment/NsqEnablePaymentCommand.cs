using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Common.Messages.Payment
{
    public class NsqEnablePaymentCommand : NsqPaymentCommandBase
    {
        public decimal Amount { get; set; }
        public NsqEnablePaymentCommand()
        {
            Command = UniversalCommandConstants.EnablePaymentCommand;
        }
        public NsqEnablePaymentCommand(string paymentType):base(paymentType)
        {
            Command = UniversalCommandConstants.EnablePaymentCommand;
        }
        public NsqEnablePaymentCommand(string paymentType, Guid transactionId,decimal amount):base(paymentType)
        {
            Command = UniversalCommandConstants.EnablePaymentCommand;
            TransactionId = transactionId;
            Amount = amount;            
        }
    }
}
