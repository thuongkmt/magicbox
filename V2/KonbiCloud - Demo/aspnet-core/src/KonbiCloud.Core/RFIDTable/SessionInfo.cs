using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.RFIDTable
{
    public class SessionInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string FromHrs { get; set; }

        public string ToHrs { get; set; }
    }
}
