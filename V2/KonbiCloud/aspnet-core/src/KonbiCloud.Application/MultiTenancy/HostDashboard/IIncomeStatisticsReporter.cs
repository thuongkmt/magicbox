using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KonbiCloud.MultiTenancy.HostDashboard.Dto;

namespace KonbiCloud.MultiTenancy.HostDashboard
{
    public interface IIncomeStatisticsService
    {
        Task<List<IncomeStastistic>> GetIncomeStatisticsData(DateTime startDate, DateTime endDate,
            ChartDateInterval dateInterval);
    }
}