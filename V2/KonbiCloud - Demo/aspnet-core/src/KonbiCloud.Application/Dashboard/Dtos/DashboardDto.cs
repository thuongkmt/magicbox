using KonbiCloud.Machines;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Dashboard.Dtos
{
    public class OverviewDashboardDto
    {        
        public OverViewDtoItem[] Items {get;set;}
        public TransactionViewDto[] Top20Transactions { get; set; }
        //public CurrentMachineLoadout[] CurrentMachineLoadouts { get; set; }
        public ChartDtoItem SaleByMachineData { get; set; }
        public ChartDtoItem SaleByProductData { get; set; }
    }

    public class OverViewDtoItem
    {
        public string NameOverview { get; set; }
        public string ExtInfo1 { get; set; }
        public string ExtInfo2 { get; set; }
        public string ExtInfo3 { get; set; }
    }

    public class TransactionViewDto
    {
        public string MachineLogicalID { get; set; }
        public string ProductName { get; set; }
        public string LocationCode { get; set; }
        public string TotalValue { get; set; }
        public string DateTime { get; set; }
    }

    public class MachineInventoryOverviewDto
    {
        public MachineInventoryOverviewItem[] Items { get; set; }
    }

    public class MachineInventoryOverviewItem
    {
        public string MachineId { get; set; }
        public string Name{ get; set; }
        public int LeftOverNum { get; set; }
        public int OutOfStockNum { get; set; }
        public int DispenseErrorNum { get; set; }
    }

    public class ChartDto
    {
        public ChartDtoItem[] ChartsItems { get; set; }
    }

    public class ChartDtoItem
    {
        public string Name { get; set; }
        public ChartDataItem[] Data { get; set; }        
    }

    public class ChartDataItem
    {
        public string X { get; set; }
        public string Y { get; set; }
    }

}
