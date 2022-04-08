//using Abp.Application.Services;
//using KonbiCloud.Dashboard.Dtos;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;

//namespace KonbiCloud.Dashboard
//{
//    public interface IDashboardAppService : IApplicationService
//    {        
//        Task<OverviewDashboardDto> GetDashboardOverviewInfomations();
//        Task<MachineInventoryOverviewDto> GetDashboardMachineInventoryInfomations();
//        Task<ChartDto> GetChartDataInfomations();
//    }
//}

using Abp.Application.Services;
using KonbiCloud.Dashboard.Dto;
using KonbiCloud.Dashboard.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.Dashboard
{
    public interface IDashboardAppService : IApplicationService
    {
        Task<DashboardData> GetDashboardData(SalesDatePeriod input);
        Task<SalesDataOuput> GetSalesData(SalesDatePeriod salesDatePeriod);
        Task<SessionStatDataOutput> GetSessionStatData();
    }
}
