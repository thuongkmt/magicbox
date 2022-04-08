using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonbiCloud.Plate
{
    [Table("Discs")]
    public class Disc : FullAuditedEntity<Guid>, IMayHaveTenant
    {
		public int? TenantId { get; set; }

		[Required]
		public virtual string Uid { get; set; }
		
		[Required]
		public virtual string Code { get; set; }

		public virtual Guid PlateId { get; set; }
		public Plate Plate { get; set; }
        public ICollection<DishMachineSyncStatus> DishMachineSyncStatus { get; set; }
    }
}