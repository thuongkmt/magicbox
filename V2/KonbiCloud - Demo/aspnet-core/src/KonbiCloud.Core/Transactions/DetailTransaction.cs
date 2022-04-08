using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using KonbiCloud.Employees;
using KonbiCloud.Enums;

namespace KonbiCloud.Transactions
{
    [Table("Transactions")]
    public class DetailTransaction: FullAuditedEntity<long>, IMayHaveTenant
    {
        public DetailTransaction()
        {
            Dishes = new List<DishTransaction>();
            Products = new List<ProductTransaction>();
            TranCode = Guid.NewGuid();//TranCode to use in machine
        }

        public Guid TranCode { get; set; }
        public int? TenantId { get; set; }
        public DateTime StartTime { get; set; }
        
        public DateTime PaymentTime { get; set; }

        //public Guid? EmployeeId { get; set; }
        //[ForeignKey("EmployeeId")]
        //public virtual Employee Employee { get; set; }

        public ICollection<DishTransaction> Dishes { get; set; }
        public ICollection<ProductTransaction> Products { get; set; }
        public PaymentState PaymentState { get; set; }
        public PaymentType PaymentType { get; set; }
        public TransactionStatus Status { get; set; }

        public decimal Amount { get; set; }

        public virtual CashlessDetail CashlessDetail { get; set; }
        public virtual CashDetail CashDetail { get; set; }
        public string TransactionId { get; set; }
        public Guid? MachineId { get; set; }
        [ForeignKey("MachineId")]
        public virtual Machines.Machine Machine { get; set; }
        public string MachineName { get; set; }
        public Guid? SessionId { get; set; }
        [ForeignKey("SessionId")]
        public virtual Machines.Session Session { get; set; }
        public string Buyer { get; set; }

        public string BeginTranImage { get; set; }
        public string EndTranImage { get; set; }
        public string TranVideo { get; set; }

        [NotMapped]
        public byte[] BeginTranImageByte { get; set; }
        [NotMapped]
        public byte[] EndTranImageByte { get; set; }
    }
}
