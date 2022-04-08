using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Common.Messages.Payment
{
    public class NsqPaymentCommandBase: UniversalCommands
    {
        public string TransactionCode { get; set; } 
        public long Time { get; set; }
        public NsqPaymentCommandBase()
        {
            Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        
    }
}
