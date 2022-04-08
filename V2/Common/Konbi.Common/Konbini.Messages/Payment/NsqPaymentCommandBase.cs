using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Common.Messages.Payment
{
    public class NsqPaymentCommandBase: UniversalCommands
    {
        public Guid TransactionId { get; set; } 
        public long Time { get; set; }
        public string PaymentType { get; set; }
        public NsqPaymentCommandBase()
        {
            //Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            CommandId = Guid.NewGuid();
        }
        public NsqPaymentCommandBase(string paymentDeviceType):base()
        {
            //Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            PaymentType = paymentDeviceType;
            CommandId = Guid.NewGuid();
           
        }
    }
 
}
