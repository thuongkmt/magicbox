using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Plate
{
    public class Tray : FullAuditedEntity<Guid>, IMayHaveTenant
    {
	    public int? TenantId { get; set; }

		[Required]
		public virtual string Name { get; set; }
		
		[Required]
		public virtual string Code { get; set; }
		
    }
}