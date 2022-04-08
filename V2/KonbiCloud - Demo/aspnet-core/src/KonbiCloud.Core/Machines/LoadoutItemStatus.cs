//using System;
//using System.Collections.Generic;
//using System.Text;
//using Abp.Domain.Entities;
//using KonbiCloud.Enums;

//namespace KonbiCloud.Machines
//{
//    public class LoadoutItemStatus:Entity<Guid>
//    {        
//        public string ItemLocation { get; set; }
//        public string ProductSKU { get; set; }
//        public int Quantity { get; set; }
//        public int? NumberExpiredItem { get; set; }
//        public HealthStatus HealthStatus { get; set; }
//        public int LocationCodeNumber => int.Parse(ItemLocation);
//        //public virtual CurrentMachineLoadout CurrentMachineLoadout { get; set; }
//    }
//}
