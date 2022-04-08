using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KonbiBrain.Common.Messages;

namespace KonbiBrain.Messages
{
    public class WebApiCommand
    {
        public string CommandObjectJson { get; set; }
        public dynamic CommandObject { get; set; }
        public CommunicationCommands Command { get; set; }
        
    }
}
