using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using KonbiCloud.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Plate
{
    public class Tray : FullAuditedEntity<Guid>, IMayHaveTenant, ISyncEntity
    {
	    public int? TenantId { get; set; }

		[Required]
		public virtual string Name { get; set; }
		
		[Required]
		public virtual string Code { get; set; }
        public bool IsSynced { get; set; }
        public DateTime? SyncDate { get; set; }

    }
}