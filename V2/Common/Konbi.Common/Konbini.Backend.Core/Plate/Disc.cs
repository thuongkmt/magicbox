using KonbiCloud.Plate;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace KonbiCloud.Plate
{
	[Table("Discs")]
    public class Disc : FullAuditedEntity , IMayHaveTenant
    {
		public int? TenantId { get; set; }

		[Required]
		public virtual string Uid { get; set; }
		
		[Required]
		public virtual string Code { get; set; }
		

		public virtual Guid PlateId { get; set; }
		public Plate Plate { get; set; }
		
    }
}