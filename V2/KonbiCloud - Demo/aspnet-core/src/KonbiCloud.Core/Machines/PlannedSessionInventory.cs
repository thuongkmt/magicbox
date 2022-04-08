//using System;
//using System.Collections.Generic;
//using System.Text;
//using Abp.Domain.Entities;
//using KonbiCloud.Enums;
//using Newtonsoft.Json;

//namespace KonbiCloud.Machines
//{
//    public class PlannedSessionInventory : Entity<Guid>, IMustHaveTenant
//    {
//        public PlannedSessionInventory()
//        {
//            Items = new HashSet<ItemInventory>();
//        }
//        public virtual Session Session { get; set; }
//        public virtual ICollection<ItemInventory> Items { get; set; }
//        public int TenantId { get; set; }
//        [JsonIgnore]
//        public virtual Machine Machine { get; set; }
//    }
//}
