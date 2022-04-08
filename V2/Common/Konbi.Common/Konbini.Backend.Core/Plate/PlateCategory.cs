using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using System.Collections.Generic;
using KonbiCloud.Common;

namespace KonbiCloud.Plate
{
	[Table("PlateCategories")]
    public class PlateCategory : FullAuditedEntity , IMayHaveTenant, ISyncEntity
    {
		public int? TenantId { get; set; }
			

		[Required]
		public virtual string Name { get; set; }
		
		public virtual string Desc { get; set; }

        public ICollection<Plate> Plates { get; set; }
        public bool IsSynced { get; set; }
        public DateTime? SyncDate { get; set; }
    }
}