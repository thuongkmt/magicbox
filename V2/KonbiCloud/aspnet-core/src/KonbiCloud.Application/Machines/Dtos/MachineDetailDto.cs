using System;
using System.Collections.Generic;
using System.Text;
using Abp.AutoMapper;

namespace KonbiCloud.Machines.Dtos
{
    [AutoMapFrom(typeof(Machine))]
    public class MachineDetailDto
    {
        public string Name { get; set; }
       
        public int StopSalesAfter { get; set; }
       
        public int TemperatureStopSales { get; set; }
        public DateTime? LastClientSyncDate { get; set; }
       
        public List<Session> Sessions { get; set; }
    }

    //[AutoMapFrom(typeof(CurrentMachineLoadout))]
    public class CurrentMachineLoadoutDto
    {
        public string MachineId { get; set; }
        public string MachineLogicalId { get; set; }

        public long? LeftOver { get; set; }
        public int OutOfStock { get; set; }
        public int DispenseErrors { get; set; }

        public ICollection<LoadoutItemStatusDto> ItemsStatusDtos { get; set; }
        public int TenantId { get; set; }
    }

    //[AutoMapFrom(typeof(LoadoutItemStatus))]
    public class LoadoutItemStatusDto 
    {
        public string MachineId { get; set; }
        public string MachineName { get; set; }

        public string ItemLocation { get; set; }
        public int Quantity { get; set; }
        public string ProductSKU { get; set; }
        public int HealthStatus { get; set; }
        public int? NumberExpiredItem { get; set; }
        public long RestockSessionID { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public DateTime Time { get; set; }
    }

    public class LoadoutItemStatusAPDto
    {
        public string MachineId { get; set; }
        public string MachineName { get; set; }

        public string ItemLocation { get; set; }
        public int Quantity { get; set; }
        public string ProductSKU { get; set; }
       
        public int? AmoutExpired { get; set; }
      //  public long RestockSessionID { get; set; }
        public string ProductName { get; set; }
      
        public DateTime Time { get; set; }
    }
}
