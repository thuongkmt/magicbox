using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Common.Messages.Payment
{
    public class NsqEnablePaymentResponseCommand :NsqPaymentCommandBase
    {
        public int Code { get; set; }
        public NsqEnablePaymentResponseCommand()
        {
            Command = UniversalCommandConstants.EnablePaymentCommand;
            
        }
    }
}
