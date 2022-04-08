using KonbiCloud.Plate;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using KonbiCloud.Common;

namespace KonbiCloud.Plate
{
    [Table("Discs")]
    public class Disc : FullAuditedEntity<Guid> , IMayHaveTenant, ISyncEntity
    {
	    public int? TenantId { get; set; }

		[Required]
		public virtual string Uid { get; set; }
		
		[Required]
		public virtual string Code { get; set; }

		public virtual Guid PlateId { get; set; }
		public Plate Plate { get; set; }
        public bool IsSynced { get; set; }
        public DateTime? SyncDate { get; set; }
    }
}