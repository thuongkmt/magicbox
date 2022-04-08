using Abp.Domain.Entities.Auditing;

namespace KonbiCloud.Transactions
{
    public class CashDetail : CreationAuditedEntity
    {
        public decimal ChangeIssued { get; set; }

        public decimal Amount { get; set; }

        /// <summary>
        /// Json string
        /// </summary>
        public string CashInfo { get; set; }
    }
}