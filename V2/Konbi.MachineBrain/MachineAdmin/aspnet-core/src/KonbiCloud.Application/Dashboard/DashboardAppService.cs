using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using Abp.Timing;
using KonbiCloud.Common;
using KonbiCloud.Dashboard.Dto;
using KonbiCloud.Dashboard.Dtos;
using KonbiCloud.Enums;
using KonbiCloud.Machines.Dtos;
using KonbiCloud.Sessions;
using KonbiCloud.Transactions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.Dashboard
{
    public class DashboardAppService : KonbiCloudAppServiceBase, IDashboardAppService
    {
        private readonly IRepository<DetailTransaction, long> _transactionRepository;
        private readonly IRepository<Session, Guid> _sessionRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IDetailLogService _detailLogService;

        public DashboardAppService(IRepository<DetailTransaction, long> transactionRepository, IRepository<Session, Guid> sessionRepository, ICacheManager cacheManager, IDetailLogService detailLog)
        {
            _transactionRepository = transactionRepository;
            _sessionRepository = sessionRepository;
            _cacheManager = cacheManager;
            _detailLogService = detailLog;
        }

        public async Task<DashboardData> GetDashboardData(SalesDatePeriod salesDatePeriod)
        {
            return new DashboardData
            {
                TotalTransSale = await GetFromCache("TotalTransSale"),
                TotalTransToday = await GetFromCache("TotalTransToday"),
                TotalTransCurrentSession = await GetFromCache("TotalTransCurrentSession"),
                TotalTransCurrentSessionSale = await GetFromCache("TotalTransCurrentSessionSale"),
                SalesSummary = (await GetSalesData(salesDatePeriod)).SalesSummary
                //SessionStat = (await GenerateSessionStatData()).SessionStat
            };
        }

        private async Task<decimal> GetFromCache(string type)
        {
            if(type == "TotalTransSale")
            {
                return await _cacheManager
                    .GetCache("DashboardCache1_TotalTransSale")
                    .Get(type, () => GetTotalTransSale());
            }
            else if(type == "TotalTransToday")
            {
                return _cacheManager
                    .GetCache("DashboardCache1_TotalTransToday")
                    .Get(type, () => GetTotalTransToday());
            }
            else if (type == "TotalTransCurrentSession")
            {
                return await _cacheManager
                    .GetCache("DashboardCache1_TotalTransCurrentSession")
                    .Get(type, () => GetTotalTransCurrentSession());
            }
            else if (type == "TotalTransCurrentSessionSale")
            {
                return await _cacheManager
                    .GetCache("DashboardCache1_GetTotalTransCurrentSessionSale")
                    .Get(type, () => GetTotalTransCurrentSessionSale());
            }
            else
            {
                return 0;
            }
        }

        public async Task<SalesDataOuput> GetSalesData(SalesDatePeriod salesDatePeriod)
        {
            if(salesDatePeriod == SalesDatePeriod.Daily)
            {
                return await _cacheManager
                 .GetCache("DashboardCache2_Daily")
                 .Get("SalesData", () => GenerateSalesData(salesDatePeriod));
            }
            else if(salesDatePeriod == SalesDatePeriod.Weekly)
            {
                return await _cacheManager
                 .GetCache("DashboardCache2_Weekly")
                 .Get("SalesData", () => GenerateSalesData(salesDatePeriod));
            }
            else
            {
                return await _cacheManager
                 .GetCache("DashboardCache2_Monthly")
                 .Get("SalesData", () => GenerateSalesData(salesDatePeriod));
            }
        }

        public async Task<SessionStatDataOutput> GetSessionStatData()
        {
            return await _cacheManager
                  .GetCache("DashboardCache3")
                  .Get("SessionStatData", () => GenerateSessionStatData());
        }

        private async Task<decimal> GetTotalTransSale()
        {
            try
            {
                var transactions = await _transactionRepository.GetAll()
                    .Where(item => item.Status == TransactionStatus.Success)
                    .ToListAsync();
                var total = transactions.Sum(item => item.Amount);
                return total;
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return 0;
            }
        }

        private decimal GetTotalTransToday()
        {
            try
            {
                var transactions = _transactionRepository.GetAll()
                       .Where(item => item.Status == TransactionStatus.Success)
                       .Where(e => e.PaymentTime.Date == Clock.Now.Date);
                var totalCount = transactions.Count();
                return totalCount;
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return 0;
            }
        }

        private async Task<decimal> GetTotalTransCurrentSession()
        {
            try
            {
                var allSessions = await _sessionRepository
                    .GetAll()
                    .OrderBy(e => e.FromHrs)
                    .ToListAsync();

                if (allSessions.Count == 0) return 0;

                var currentTime = Clock.Now;
                var currentTimeInMinute = currentTime.Hour * 60 + currentTime.Minute;

                Session currentSession = null;
                if (allSessions.Count == 1)
                {
                    currentSession = allSessions[0];
                }
                else
                {
                    for (int i = 0; i < allSessions.Count; i++)
                    {
                        var fromInMin = 0;
                        if (i > 0)
                        {
                            var fromHour = allSessions[i].FromHrs.Substring(0, 2);
                            var fromMin = allSessions[i].FromHrs.Substring(2, 2);
                            fromInMin = Convert.ToInt32(fromHour) * 60 + Convert.ToInt32(fromMin);
                        }

                        var toInMin = 24 * 60;
                        if (i < allSessions.Count - 1)
                        {
                            var toHour = allSessions[i + 1].FromHrs.Substring(0, 2);
                            var toMin = allSessions[i + 1].FromHrs.Substring(2, 2);
                            toInMin = Convert.ToInt32(toHour) * 60 + Convert.ToInt32(toMin);
                        }

                        if (currentTimeInMinute >= fromInMin && currentTimeInMinute < toInMin)
                        {
                            currentSession = allSessions[i];
                            break;
                        }
                    }
                }

                var transactions = _transactionRepository.GetAll()
                    .Where(item => item.Status == TransactionStatus.Success)
                    .Where(e => e.PaymentTime.Date == Clock.Now.Date)
                    .Where(e => e.Session.Id == currentSession.Id);
                var totalCount = transactions.Count();
                return totalCount;
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return 0;
            }
        }

        private async Task<decimal> GetTotalTransCurrentSessionSale()
        {
            try
            {
                var allSessions = await _sessionRepository
                .GetAll()
                .OrderBy(e => e.FromHrs)
                .ToListAsync();

                if (allSessions.Count == 0) return 0;

                var currentTime = Clock.Now;
                var currentTimeInMinute = currentTime.Hour * 60 + currentTime.Minute;

                Session currentSession = null;
                if (allSessions.Count == 1)
                {
                    currentSession = allSessions[0];
                }
                else
                {
                    for (int i = 0; i < allSessions.Count; i++)
                    {
                        var fromInMin = 0;
                        if (i > 0)
                        {
                            var fromHour = allSessions[i].FromHrs.Substring(0, 2);
                            var fromMin = allSessions[i].FromHrs.Substring(2, 2);
                            fromInMin = Convert.ToInt32(fromHour) * 60 + Convert.ToInt32(fromMin);
                        }

                        var toInMin = 24 * 60;
                        if (i < allSessions.Count - 1)
                        {
                            var toHour = allSessions[i + 1].FromHrs.Substring(0, 2);
                            var toMin = allSessions[i + 1].FromHrs.Substring(2, 2);
                            toInMin = Convert.ToInt32(toHour) * 60 + Convert.ToInt32(toMin);
                        }

                        if (currentTimeInMinute >= fromInMin && currentTimeInMinute < toInMin)
                        {
                            currentSession = allSessions[i];
                            break;
                        }
                    }
                }

                var transactions = _transactionRepository.GetAll()
                    .Where(item => item.Status == TransactionStatus.Success)
                    .Where(e => e.PaymentTime.Date == Clock.Now.Date)
                    .Where(e => e.Session.Id == currentSession.Id);

                var total = transactions.Sum(item => item.Amount);
                return total;
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return 0;
            }
        }

        private async Task<SalesDataOuput> GenerateSalesData(SalesDatePeriod salesDatePeriod)
        {
            List<SalesData> data = new List<SalesData>();

            try
            {
                switch (salesDatePeriod)
                {
                    case SalesDatePeriod.Daily:

                        var EndDateDaily = Clock.Now;
                        var transDaily = await _transactionRepository.GetAll()
                           .Where(item => item.Status == TransactionStatus.Success)
                           .Where(t => t.PaymentTime.Date >= EndDateDaily.AddDays(-5).Date && t.PaymentTime.Date <= EndDateDaily.Date)
                           .OrderByDescending(t => t.PaymentTime)
                           .ToListAsync();

                        for (int i = 5; i > 0; i--)
                        {
                            var day = DateTime.Now.AddDays(-i);
                            var transDay = transDaily.FindAll(x => x.PaymentTime.Date == day.Date);
                            if (transDay.Count == 0) data.Add(new SalesData(day.ToString("yyyy-MM-dd"), 0, 0));
                            else
                            {
                                var item = new SalesData(day.ToString("yyyy-MM-dd"), transDay.Sum(x => x.Amount), transDay.Count);
                                data.Add(item);
                            }
                        }
                        break;
                    case SalesDatePeriod.Weekly:

                        var EndDateWeekly = DateTime.Now.AddDays(DayOfWeek.Monday - DateTime.Now.DayOfWeek).AddDays(-1); //get sunday
                                                                                                                         //get 3 weeks * 7 days = 21 day data ago 
                        var transWeekly = await _transactionRepository.GetAll()
                          .Where(item => item.Status == TransactionStatus.Success)
                          .Where(t => t.PaymentTime.Date >= EndDateWeekly.AddDays(-21).Date && t.PaymentTime.Date <= EndDateWeekly.Date)
                          .OrderByDescending(t => t.PaymentTime)
                          .ToListAsync();

                        for (int i = 4; i > 0; i--)
                        {
                            var weekStart = EndDateWeekly.AddDays(-((i - 1) * 7));
                            var weekEnd = weekStart.AddDays(7);

                            var transWeek = transWeekly.FindAll(x => x.PaymentTime.Date >= weekStart && x.PaymentTime.Date <= weekEnd);

                            if (transWeek.Count == 0) data.Add(new SalesData(weekStart.ToString("yyyy-MM-dd") + " W" + i, 0, 0));
                            else
                            {
                                var item = new SalesData(weekStart.ToString("yyyy-MM-dd") + " W" + i, transWeek.Sum(x => x.Amount), transWeek.Count);
                                data.Add(item);
                            }
                        }
                        break;
                    case SalesDatePeriod.Monthly:

                        var EndDateMonthly = DateTime.Now;
                        //get 5 Month days = 150 days data ago 
                        var transMonthly = await _transactionRepository.GetAll()
                          .Where(item => item.Status == TransactionStatus.Success)
                          .Where(t => t.PaymentTime.Date >= EndDateMonthly.AddDays(-150).Date && t.PaymentTime.Date <= EndDateMonthly.Date)
                          .OrderByDescending(t => t.PaymentTime)
                          .ToListAsync();

                        for (int i = 4; i > 0; i--)
                        {
                            var month = DateTime.Now.AddMonths(-i);
                            var transMonth = transMonthly.FindAll(x => x.PaymentTime.Month == month.Month);
                            if (transMonth.Count == 0) data.Add(new SalesData(month.ToString("yyyy-MM"), 0, 0));
                            else
                            {
                                var item = new SalesData(month.ToString("yyyy-MM"), transMonth.Sum(x => x.Amount), transMonth.Count);
                                data.Add(item);
                            }
                        }
                        break;
                }

                return (new SalesDataOuput(data));
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new SalesDataOuput(new List<SalesData>());
            }
        }

        private async Task<SessionStatDataOutput> GenerateSessionStatData()
        {
            try
            {
                var trans = await _transactionRepository.GetAllIncluding()
                    .Where(item => item.Status == TransactionStatus.Success)
                    .Where(e => e.PaymentTime.Date == Clock.Now.Date)
                    .Include(x => x.Session)
                    .GroupBy(x => x.Session)
                    .ToListAsync();
                trans = trans.FindAll(x => x.Key != null).OrderBy(x => x.Key.FromHrs).ToList();

                List<SessionStatData> returnData = new List<SessionStatData>();
                for (int i = 0; i < trans.Count; i++)
                {
                    var sessionName = trans[i].Key.Name;
                    var transOfSession = trans[i].ToList();
                    var item = new SessionStatData(sessionName, transOfSession.Count, transOfSession.Sum(x => x.Amount));
                    returnData.Add(item);
                }

                return (new SessionStatDataOutput(returnData));
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new SessionStatDataOutput(new List<SessionStatData>());
            }
        }

    }
}
