using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.MachineSessions.Dtos
{
    public class MachineSessionListDto
    {
        public int MaxResultCount { get; set; }
        public int SkipCount { get; set; }
        public string Sorting { get; set; }
    }
}
