using KonbiBrain.Common.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Konbini.Common.Messages
{
    public class UniversalACKResponseCommand: UniversalCommands
    {
        public UniversalACKResponseCommand(Guid commandId) : base(UniversalCommandConstants.ACKResponse) {
            CommandId = commandId;
        }
    }
}
