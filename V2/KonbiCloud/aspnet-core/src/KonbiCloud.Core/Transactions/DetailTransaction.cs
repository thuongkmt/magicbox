using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using KonbiCloud.Enums;
using KonbiCloud.Inventories;

namespace KonbiCloud.Transactions
{
    [Table("Transactions")]
    public class DetailTransaction: FullAuditedEntity<long>, IMayHaveTenant
    {
        public DetailTransaction()
        {
            Products = new List<ProductTransaction>();
            Inventories = new List<InventoryItem>();
            TransactionDetails = new List<TransactionDetail>();
            TranCode = Guid.NewGuid();//TranCode to use in machine
        }

        public Guid TranCode { get; set; }
        public int? TenantId { get; set; }
        public DateTime StartTime { get; set; }
        
        public DateTime PaymentTime { get; set; }

        public ICollection<ProductTransaction> Products { get; set; }

        public ICollection<InventoryItem> Inventories { get; set; }

        public ICollection<TransactionDetail> TransactionDetails { get; set; }
        public PaymentState PaymentState { get; set; }
        public PaymentType PaymentType { get; set; }
        public TransactionStatus Status { get; set; }

        public decimal Amount { get; set; }

        public int? CashlessDetailId { get; set; }

        [ForeignKey("CashlessDetailId")]
        public virtual CashlessDetail CashlessDetail { get; set; }

        public int? CashDetailId { get; set; }

        [ForeignKey("CashDetailId")]
        public virtual CashDetail CashDetail { get; set; }

        public long LocalTranId { get; set; }

        public Guid? MachineId { get; set; }

        [ForeignKey("MachineId")]
        public virtual Machines.Machine Machine { get; set; }

        public string MachineName { get; set; }

        public Guid? SessionId { get; set; }

        [ForeignKey("SessionId")]
        public virtual Machines.Session Session { get; set; }

        public Guid? TopupId { get; set; }

        [ForeignKey("TopupId")]
        public virtual Topup Topup { get; set; }

        public string Buyer { get; set; }
    }
}
