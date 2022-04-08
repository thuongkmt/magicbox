using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using KonbiCloud.Common;

namespace KonbiCloud.Machines
{
	[Table("Sessions")]
    public class Session : FullAuditedEntity<Guid> , IMayHaveTenant, ISyncEntity
    {
		public int? TenantId { get; set; }

		[Required]
		public virtual string Name { get; set; }
		
		public virtual string FromHrs { get; set; }
		
		public virtual string ToHrs { get; set; }
        public bool IsSynced { get; set; }
        public DateTime? SyncDate { get; set; }


    }
}