using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillAcceptorBrain
{
    public class CommandInfo
    {
        public bool NeedAck { get; set; }
        public bool IsSent { get; set; }
        public string HexCommand { get; set; }
        public DateTime SendingTime { get; set; }
        public DateTime? ResponseTime { get; set; }
        public bool Acked { get; set; }
        public bool NeedWaitResponse { get; set; }
        public string WaitResponsePrefix { get; set; }
    }
}
