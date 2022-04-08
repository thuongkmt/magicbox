using KonbiCloud.Dashboard.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Dashboard.Dtos
{
    public class DashboardData
    {
        public decimal TotalTransSale { get; set; }
        public decimal TotalTransToday { get; set; }
        public decimal TotalTransCurrentSession { get; set; }
        public decimal TotalTransCurrentSessionSale { get; set; }
        public List<SalesData> SalesSummary { get; set; }
        public List<SessionStatData> SessionStat { get; set; }

    }
}
