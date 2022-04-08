using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Common.Messages.Payment
{
    public class NsqFeedBackResponseCommand : NsqPaymentCommandBase
    {
        public int Code { get; set; }
        public NsqFeedBackResponseCommand()
        {
            Command = UniversalCommandConstants.EnablePaymentCommand;
            
        }
    }
}
