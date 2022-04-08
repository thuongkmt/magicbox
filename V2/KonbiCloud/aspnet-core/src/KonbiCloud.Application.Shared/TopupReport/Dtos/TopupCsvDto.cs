using Abp.Application.Services.Dto;
using KonbiCloud.Enums;
using System;

namespace KonbiCloud.TopupReport.Dtos
{
    public class TopupCsvDto 
    {
        public string MachineId { get; set; }

        public string ItemId { get; set; }

        public string RestockerId { get; set; }

        public DateTime DateTime { get; set; }

        public string SessionType { get; set; }

        public string ItemName { get; set; }

        public string  Category { get; set; }

        public string SKU { get; set; }

        public int Quantity { get; set; }

        public int QuantityBefore { get; set; }
        public int QuantityFinal { get; set; }

    }
}
