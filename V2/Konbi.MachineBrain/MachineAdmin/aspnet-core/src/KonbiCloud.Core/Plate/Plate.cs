using KonbiCloud.Plate;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using System.Collections.Generic;

namespace KonbiCloud.Plate
{
	[Table("Plates")]
    public class Plate : FullAuditedEntity<Guid> , IMayHaveTenant
    {
		public int? TenantId { get; set; }

		public virtual string Name { get; set; }
		
		public virtual string ImageUrl { get; set; }
		
		public virtual string Desc { get; set; }
		
		[Required]
		public virtual string Code { get; set; }
		
		public virtual int? Avaiable { get; set; }
		
		public virtual string Color { get; set; }
		

		public virtual int? PlateCategoryId { get; set; }
		public PlateCategory PlateCategory { get; set; }

        public ICollection<Disc> Discs { get; set; }

    }
}