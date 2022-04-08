//using System;
//using System.Collections.Generic;
//using System.Text;
//using Abp.Domain.Entities;

//namespace KonbiCloud.Machines
//{
//    public class CurrentMachineLoadout:Entity<Guid>,IMustHaveTenant
//    {
//        public CurrentMachineLoadout()
//        {
//            ItemsStatus = new List<LoadoutItemStatus>();
//        }
//        public string MachineId { get; set; }
//        public string MachineLogicalId { get; set; }

//        public long? LeftOver { get; set; }
//        public int OutOfStock { get; set; }
//        public int DispenseErrors { get; set; }
//        public int Expired { get; set; }
//        public ICollection<LoadoutItemStatus> ItemsStatus { get; set; }
//        public int? TenantId { get; set; }
//        public DateTime Time { get; set; }
//    }
//}
