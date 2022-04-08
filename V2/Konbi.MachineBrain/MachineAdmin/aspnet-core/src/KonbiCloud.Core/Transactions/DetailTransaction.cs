using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using KonbiCloud.Common;
using KonbiCloud.Employees;
using KonbiCloud.Enums;
using KonbiCloud.Inventories;
using KonbiCloud.Sessions;
using Konbini.Messages.Enums;
using Newtonsoft.Json;

namespace KonbiCloud.Transactions
{
    [Table("Transactions")]
    public class DetailTransaction: FullAuditedEntity<long>, IMayHaveTenant ,ISyncEntity
    {
        public DetailTransaction()
        {
            Dishes = new List<DishTransaction>();
            Products = new List<ProductTransaction>();
            Inventories = new List<InventoryItem>();

            TranCode = Guid.NewGuid();//TranCode to use in machine
        }

        public Guid TranCode { get; set; }
        public int? TenantId { get; set; }
        public DateTime StartTime { get; set; }
        
        public DateTime PaymentTime { get; set; }
        public Guid? SessionId { get; set; }
        [ForeignKey("SessionId")]
        public virtual Session Session { get; set; }

        public Guid? TopupId { get; set; }
        [ForeignKey("TopupId")]
        public virtual Topup Topup { get; set; }


        public ICollection<DishTransaction> Dishes { get; set; }
        public ICollection<ProductTransaction> Products { get; set; }

        public ICollection<InventoryItem> Inventories { get; set; }

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
        public bool IsSynced { get; set; }
        public DateTime? SyncDate { get; set; }

        [NotMapped]
        public Guid? MachineId { get; set; }
        [NotMapped]
        public string MachineName { get; set; }
        public string Buyer { get; set; }

        public string BeginTranImage { get; set; }
        public string EndTranImage { get; set; }
        public string TranVideo { get; set; }

        [NotMapped]
        public byte[] BeginTranImageByte { get; set; }
        [NotMapped]
        public byte[] EndTranImageByte { get; set; }

        public string OtherInfo1 { get; set; }
        public string OtherInfo2 { get; set; }
        public string OtherInfo3 { get; set; }
    }
}
