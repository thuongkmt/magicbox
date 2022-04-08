using Abp.Domain.Entities.Auditing;
using KonbiCloud.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KonbiCloud.Machines
{
    public class UserMachine : FullAuditedEntity<Guid>
    {
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public long UserId { get; set; }

        [ForeignKey("MachineId")]
        public virtual Machine Machine { get; set; }
        public Guid MachineId { get; set ; }
    }
}
