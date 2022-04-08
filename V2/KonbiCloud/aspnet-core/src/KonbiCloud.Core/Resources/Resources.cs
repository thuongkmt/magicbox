using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KonbiCloud.Resources
{
    [Table("Resources")]
    public class Resource : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        public string FileName { get; set; }

        public string Thumbnail { get; set; }

        public long Length { get; set; }

        public MediaType MediaType { get; set; }
    }
}
