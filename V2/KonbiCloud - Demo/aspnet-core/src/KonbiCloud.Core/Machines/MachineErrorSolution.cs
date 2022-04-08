using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Machines
{
    public class MachineErrorSolution : FullAuditedEntity<int>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Solution { get; set; }
    }
}
