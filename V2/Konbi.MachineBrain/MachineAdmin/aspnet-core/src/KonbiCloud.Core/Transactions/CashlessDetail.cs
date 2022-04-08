using Abp.Domain.Entities.Auditing;

namespace KonbiCloud.Transactions
{
    public class CashlessDetail:CreationAuditedEntity
    {
        public decimal Amount { get; set; }
        public string Tid { get; set; }
        public string Mid { get; set; }
        public string Invoice { get; set; }
        public string Batch { get; set; }
        public string CardLabel { get; set; }
        public string CardNumber { get; set; }
        public string Rrn { get; set; }
        public string ApproveCode { get; set; }
        public string EntryMode { get; set; }
        public string AppLabel { get; set; }
        public string Aid { get; set; }
        public string Tc { get; set; }
    }
}