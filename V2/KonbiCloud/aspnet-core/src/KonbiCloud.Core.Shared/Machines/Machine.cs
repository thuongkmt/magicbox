using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using KonbiCloud.Inventories;
using System;
using System.Collections.Generic;

namespace KonbiCloud.Machines
{
    public class Machine : FullAuditedEntity<Guid>, IMayHaveTenant
    {

        public Machine()
        {
            Devices = new List<Device>();
        }
        public string Name { get; set; }
        public int? TenantId { get; set; }
        public bool IsOffline { get; set; }

        public string MachineType { get; set; }//Vending,Locker,MagicBox,MagicPlate

        public ICollection<Device> Devices { get; set; }
        public ICollection<CurrentInventory> CurrentInventory { get; set; }
        public DateTime? StockLastUpdated { get; set; }

    }

}
