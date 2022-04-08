using Abp.Domain.Entities.Auditing;
using KonbiCloud.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonbiCloud.Transactions
{
    [Table("BlackListCards")]
    public class BlackListCard : FullAuditedEntity<long>, ISyncEntity
    {
        public string CardLabel { get; set; }
        public string CardNumber { get; set; }
        public decimal UnpaidAmount { get; set; }
        public bool IsSynced { get; set; }
        public DateTime? SyncDate { get; set; }
    }
}
