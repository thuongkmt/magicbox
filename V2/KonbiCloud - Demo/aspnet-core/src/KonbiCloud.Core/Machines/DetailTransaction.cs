//using System;
//using Abp.Domain.Entities;
//using Abp.Domain.Entities.Auditing;
//using KonbiCloud;
//using KonbiCloud.Enums;

//namespace KonbiCloud.Machines
//{
//    public class DetailTransaction : AuditedEntity<Guid>,IMustHaveTenant
//    {
//        public long LocalId { get; set; }
//        public string MachineId { get; set; }
//        public string MachineLogicalId { get; set; }
//        public SessionType SessionType { get; set; }
//        public double TotalValue { get; set; }
//        public string PaymentMethod { get; set; }
//        public long TransactionId { get; set; }
//        public long RestockSessionId { set; get; }
//        public long ProductId { get; set; }
//        public string ProductSKU { get; set; }
//        public string ProductDescription { get; set; }
//        public long Quantity { get; set; }
//        public double UnitPrice { get; set; }
//        public string Unit { get; set; }
//        public double LinePrice { get; set; }
//        public string LocationCode { get; set; }
//        public string ProductName { get; set; }
//        public DateTime DateTime { get; set; }

//        public TransactionStatus State { get; set; }

//        public bool IsNormalTransaction {
//            get {
//                return State == TransactionStatus.Success || State == TransactionStatus.Timeout;
//            }
//        }
//        private DateTime? paymentTime;
//        public DateTime? PaymentTime
//        {
//            get { return paymentTime; }
//            set
//            {
//                PaymentEpochTime = value?.ToUnixTime() ?? 0;
//                paymentTime = value;
//            }
//        }
//        public long PaymentEpochTime { get; set; }
//        public int Collect5c { get; set; }
//        public int Collect10c { get; set; }
//        public int Collect20c { get; set; }
//        public int Collect50c { get; set; }
//        public int Collect100c { get; set; }
//        public int Collect2d { get; set; }
//        public int Collect5d { get; set; }
//        public int Collect10d { get; set; }
//        public int Collect50d { get; set; }

//        //
//        public long? ChangeFiveCents { get; set; }

//        public long? ChangeTenCents { get; set; }

//        public long? ChangeTwentyCents { get; set; }

//        public long? ChangeFiftyCents { get; set; }

//        public long? ChangeOneDollar { get; set; }

//        public long? ChangeTwoDollars { get; set; }

//        public long? ChangeFiveDollars { get; set; }

//        public long? ChangeTenDollars { get; set; }

//        public long? ChangeFiftyDollars { get; set; }

//        public double? SumChanged => ChangeFiveCents * 0.05 + ChangeTenCents * 0.1 + ChangeTwentyCents * 0.2 + ChangeFiftyCents * 0.5 + ChangeOneDollar * 1 +
//                                      ChangeTwoDollars * 2 + ChangeFiveDollars * 5 + ChangeTenDollars * 10 + ChangeFiftyDollars * 50;

//        public double ChangeIssued { get {
//                return State != TransactionStatus.Cancelled ? this.SumCollected - this.TotalValue : this.SumCollected;
//            } }

//        //public double SumCollected { get; set; }
//        public double AmountDue => SumCollected - Qty * UnitPrice;

//        public double TotalCollected { get; set; }

//        public double SumCollected => TotalCollected;
//        public int Qty { get; set; }
//        public virtual RestockSession RestockSession { get; set; }

//        public int TenantId { get; set; }
//        public int RemainProduct { get; set; }
//        public string EmployeeId { get; set; }

//        public string Tid { get; set; }
//        public string Mid { get; set; }
//        public string Invoice { get; set; }
//        public string CardLabel { get; set; }
//        public string CardNumber { get; set; }
//        public string ApproveCode { get; set; }
//        public string CustomerEmail { get; set; }
//    }
//}
