using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace KonbiCloud.Sessions
{
    [Table("Sessions")]
    public class Session : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public virtual string Name { get; set; }

        public virtual string FromHrs { get; set; }

        public virtual string ToHrs { get; set; }

    }
}