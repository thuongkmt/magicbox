using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Common.Messages.Payment
{
    public class NsqEnablePaymentCommand : NsqPaymentCommandBase
    {
        public NsqEnablePaymentCommand()
        {
            Command = UniversalCommandConstants.EnablePaymentCommand;
           
        }
    }
}
