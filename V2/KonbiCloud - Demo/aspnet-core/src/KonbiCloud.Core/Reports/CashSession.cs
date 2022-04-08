using System;
using Abp.Domain.Entities.Auditing;
using KonbiCloud;

namespace KonbiCloud.Reports
{
    public class CashSession : AuditedEntity<Guid>
    {
        public long LocalId { get; set; }
        public string MachineId { get; set; }
        public string MachineLogicalId { get; set; }
        private DateTime? _startTime { set; get; }
        public DateTime? StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
                StartEpochTime = value?.ToUnixTime() ?? 00;
            }
        }
        public long StartEpochTime { set; get; }

        private DateTime? _endTime { set; get; }
        public DateTime? EndTime
        {
            set
            {
                _endTime = value;
                EndEpochTime = value?.ToUnixTime() ?? 0;
            }
            get { return _endTime; }
        }
        public long EndEpochTime { set; get; }

        public double DefaultAmount { get; set; }
        public double ExcessAmtCollected { get; set; }
        public double ToppedUpAmount { get; set; }
        public double CashlessErrorRefunded { get; set; }

        public double BeginAmount { get; set; }
        public double CashSalesAmount { get; set; }
        public double CashlessSalesAmount { get; set; }
        public double CashCollected { get; set; }
        public double CoinCollected { get; set; }
        public double TotalSalesAmount { get; set; }
        public double EndAmount { get; set; }
        public bool IsClosed { get; set; }
        public int Preset10c { get; set; }
        public int Preset20c { get; set; }
        public int Preset50c { get; set; }
        public int Preset100c { get; set; }
        public int Preset2d { get; set; }
        public int Preset5d { get; set; }
        public int Preset10d { get; set; }
        public int Preset50d { get; set; }

        public int Collected10c { get; set; }
        public int Collected20c { get; set; }
        public int Collected50c { get; set; }
        public int Collected100c { get; set; }
        public int Collected2d { get; set; }
        public int Collected5d { get; set; }
        public int Collected10d { get; set; }
        public int Collected50d { get; set; }

        public int ToppedUp10c { get; set; }
        public int ToppedUp20c { get; set; }
        public int ToppedUp50c { get; set; }
        public int ToppedUp100c { get; set; }
        public int ToppedUp2d { get; set; }
        public int ToppedUp50d { get; set; }
        public int ToppedUp5d { get; set; }
        public int ToppedUp10d { get; set; }
        public double DefaultFloatAmount { get; set; }
        public double ToppedUpAmountCalulated => (ToppedUp50d > 0 ? 50 * ToppedUp50d : 0)
                             + (ToppedUp10d > 0 ? 10 * ToppedUp10d : 0)
                             + (ToppedUp5d > 0 ? 5 * ToppedUp5d : 0)
                             + (ToppedUp2d > 0 ? 2 * ToppedUp2d : 0)

                             + (ToppedUp100c > 0 ? (100 * ToppedUp100c * 0.01) : 0)
                             + (ToppedUp50c > 0 ? (50 * ToppedUp50c * 0.01) : 0)
                             + (ToppedUp20c > 0 ? (20 * ToppedUp20c * 0.01) : 0)
                             + (ToppedUp10c > 0 ? (10 * ToppedUp10c * 0.01) : 0);
        public void CalculateToppedUpAmount()
        {
            ToppedUpAmount = (ToppedUp50d > 0 ? 50 * ToppedUp50d : 0)
                             + (ToppedUp10d > 0 ? 10 * ToppedUp10d : 0)
                             + (ToppedUp5d > 0 ? 5 * ToppedUp5d : 0)
                             + (ToppedUp2d > 0 ? 2 * ToppedUp2d : 0)

                             + (ToppedUp100c > 0 ? (100 * ToppedUp100c * 0.01) : 0)
                             + (ToppedUp50c > 0 ? (50 * ToppedUp50c * 0.01) : 0)
                             + (ToppedUp20c > 0 ? (20 * ToppedUp20c * 0.01) : 0)
                             + (ToppedUp10c > 0 ? (10 * ToppedUp10c * 0.01) : 0);
        }

        //public double DefaultFloatAmount => Preset100c + Preset10c * 0.1 + Preset10d * 10 + Preset20c * 0.2 +
        //                                    Preset2d * 2 + Preset50c * 0.5 + Preset50d * 50 + Preset5d * 5;

    }
}
