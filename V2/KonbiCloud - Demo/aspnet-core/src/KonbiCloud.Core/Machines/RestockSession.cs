//using System;
//using System.Collections.Generic;
//using System.Text;
//using Abp.Domain.Entities;
//using Abp.Domain.Entities.Auditing;
//using KonbiCloud;
//using KonbiCloud.Enums;

//namespace KonbiCloud.Machines
//{
//    public class RestockSession : FullAuditedEntity<Guid>, IMustHaveTenant
//    {
//        public long LocalId { get; set; }
//        public string MachineId { get; set; }
//        public SessionType SessionType { get; set; }
//        private DateTime _startTime { set; get; }
//        public DateTime StartTime { get { return _startTime; } set { StartEpochTime = value.ToUnixTime(); _startTime = value; } }
//        public long StartEpochTime { set; get; }
//        private DateTime _endTime { set; get; }
//        public DateTime EndTime { get { return _endTime; } set { EndEpochTime = value.ToUnixTime(); _endTime = value; } }
//        public long EndEpochTime { set; get; }
//        public List<LoadoutItem> LoadoutItems { get; set; }
//        //public List<Transaction> Transactions { get; set; }
//        //public List<ItemInventory> PlannedItems { get; set; }
//        public int EditStock { get; set; }
//        public bool IsClosed { get; set; }
//        public int DispenseErrors { get; set; }
//        public RestockSession()
//        {
//            LoadoutItems = new List<LoadoutItem>();
//            //Transactions = new List<Transaction>();
//            PlannedItems = new List<ItemInventory>();
//        }

//        // report attributes
//        public int StockedQuantity { set; get; }
//        public long? SoldQuantity { set; get; }
//        public int WastageQuantity { set; get; }
//        public double SaleAmount { set; get; }
//        public int TenantId { get; set; }
//    }
//}
