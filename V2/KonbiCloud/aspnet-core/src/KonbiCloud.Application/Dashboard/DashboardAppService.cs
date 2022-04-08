using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using Abp.Timing;
using KonbiCloud.Common;
using KonbiCloud.Dashboard.Dto;
using KonbiCloud.Dashboard.Dtos;
using KonbiCloud.Enums;
using KonbiCloud.Machines;
using KonbiCloud.Machines.Dtos;
using KonbiCloud.Sessions;
using KonbiCloud.Transactions;
using KonbiCloud.Products;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KonbiCloud.TemperatureLogs;

namespace KonbiCloud.Dashboard
{
    public class DashboardAppService : KonbiCloudAppServiceBase, IDashboardAppService
    {
        private readonly IRepository<Transactions.DetailTransaction, long> _transactionRepository;
        private readonly IRepository<Session, Guid> _sessionRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IDetailLogService _detailLogService;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IRepository<TemperatureLog> _temperatureLogsRepository;

        public DashboardAppService(
            IRepository<Transactions.DetailTransaction, long> transactionRepository, 
            IRepository<Session, Guid> sessionRepository, ICacheManager cacheManager, 
            IDetailLogService detailLog, IRepository<Product, Guid> productRepository, 
            IRepository<Machine, Guid> machineRepository,
            IRepository<TemperatureLog> temperatureLogsRepository
            )
        {
            _transactionRepository = transactionRepository;
            _sessionRepository = sessionRepository;
            _cacheManager = cacheManager;
            _detailLogService = detailLog;
            _productRepository = productRepository;
            _machineRepository = machineRepository;
            _temperatureLogsRepository = temperatureLogsRepository;
        }

        public async Task<DashboardData> GetDashboardData(SalesDatePeriod salesDatePeriod)
        {
            return new DashboardData
            {
                TotalTransSale = await GetTotalTransSale(),
                TotalTransToday = GetTotalTransToday(),
                TotalTransCurrentSession = 0, //await GetFromCache("TotalTransCurrentSession"),
                TotalTransCurrentSessionSale = 0, //await GetFromCache("TotalTransCurrentSessionSale"),
                SalesSummary = (await GetSalesData(salesDatePeriod)).SalesSummary,
                TransactionForToday = await GetTransactionForToday()
                //SessionStat = (await GenerateSessionStatData()).SessionStat
            };
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
               Logger.Error(ex.Message);
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
                Logger.Error(ex.Message);
                return 0;
            }
        }
        
        public async Task<MachineStatDataOutput> GetSaleByMachineData()
        {
            try
            {
                List<MachineStatData> returnData = new List<MachineStatData>();

                var allMachines = await _machineRepository.GetAll()
                    .Where(x => x.IsOffline == false)
                    .OrderBy(x => x.CreationTime)
                    .ToListAsync();

                var trans = await _transactionRepository.GetAllIncluding()
                    .Where(item => item.Status == TransactionStatus.Success)
                    .Include(x => x.Machine).Where(x => x.Machine.IsOffline == false)
                    .GroupBy(x => x.Machine)
                    .ToListAsync();

                trans = trans.FindAll(x => x.Key != null).ToList();

                for (int i = 0; i < allMachines.Count; i++)
                {
                    var transOfMachine = trans.FindAll(x => x.Key.Id == allMachines[i].Id).ToList();
                    if(transOfMachine.Count > 0)
                    {
                        var item = new MachineStatData(allMachines[i].Name, transOfMachine[0].ToList().Count, transOfMachine[0].Sum(x => x.Amount));
                        returnData.Add(item);
                    }
                    else
                    {
                        var item = new MachineStatData(allMachines[i].Name, 0, 0);
                        returnData.Add(item);
                    }
                }

                return (new MachineStatDataOutput(returnData));
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return new MachineStatDataOutput(new List<MachineStatData>());
            }
        }

        /// <summary>
        /// Get transactions for today.
        /// </summary>
        /// <returns></returns>
        private async Task<List<TransactionViewDto>> GetTransactionForToday()
        {
            try
            {
                // Get all transactions.
                var transactions = await _transactionRepository.GetAll()
                       .Where(item => item.Status == TransactionStatus.Success)
                       .Where(e => e.PaymentTime.Date == Clock.Now.Date)
                       .Include(e => e.Inventories)
                       .Include(e=> e.Machine)
                       .Include("Inventories.Product")
                       .OrderByDescending(x => x.PaymentTime)
                       .Take(20)
                       .ToListAsync();

                // Declare list return.
                List<TransactionViewDto> returnData = new List<TransactionViewDto>();

                // Map data for return.
                for (int i = 0; i < transactions.Count; i++)
                {
                    List<string> lstProductName = new List<string>();

                    // Get all product name by transaction.
                    foreach (var inventory in transactions[i].Inventories)
                    {
                        lstProductName.Add(inventory.Product.Name);
                    }

                    var item = new TransactionViewDto();
                    item.MachineLogicalID = transactions[i].Machine.Name;
                    item.ProductName = lstProductName;
                    item.LocationCode = transactions[i].TranCode.ToString();
                    item.TotalValue = transactions[i].Amount;
                    item.DateTime = transactions[i].PaymentTime;
                    returnData.Add(item);
                }

                // Return list transactions.
                return returnData;
            }
            catch (Exception ex)
            {
                // Add message error to log.
                Logger.Error(ex.Message);
                // Return empty log.
                return new List<TransactionViewDto>();
            }
        }

        public async Task<SalesDataOuput> GetSalesData(SalesDatePeriod salesDatePeriod)
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

                        for (int i = 5; i >= 0; i--)
                        {
                            var day = Clock.Now.AddDays(-i);
                            var transDay = transDaily.FindAll(x => x.PaymentTime.Date == day.Date);
                            if (transDay.Count == 0) data.Add(new SalesData(day.ToShortDateString(), 0, 0));
                            else
                            {
                                var item = new SalesData(day.ToShortDateString(), transDay.Sum(x => x.Amount), transDay.Count);
                                data.Add(item);
                            }
                        }
                        break;
                    case SalesDatePeriod.Weekly:

                        var EndDateWeekly = DateTime.Now.AddDays(DayOfWeek.Monday - Clock.Now.DayOfWeek).AddDays(-1); //get sunday
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

                            var transWeek = transWeekly.FindAll(x => x.PaymentTime.Date > weekStart && x.PaymentTime.Date <= weekEnd);
                            data.Add(new SalesData(weekStart.ToShortDateString() + " W" + i, transWeek.Sum(x => x.Amount), transWeek.Count));
                        }
                        //add this week
                        //var transThisWeek = transWeekly.FindAll(x => x.PaymentTime.Date > EndDateWeekly && x.PaymentTime.Date <= Clock.Now);
                        //data.Add(new SalesData(DateTime.Now.ToString("yyyy-MM-dd") + " W", transThisWeek.Sum(x => x.Amount), transThisWeek.Count));

                        break;
                    case SalesDatePeriod.Monthly:

                        var EndDateMonthly = Clock.Now;
                        //get 5 Month days = 150 days data ago 
                        var transMonthly = await _transactionRepository.GetAll()
                          .Where(item => item.Status == TransactionStatus.Success)
                          .Where(t => t.PaymentTime.Date >= EndDateMonthly.AddDays(-150).Date && t.PaymentTime.Date <= EndDateMonthly.Date)
                          .OrderByDescending(t => t.PaymentTime)
                          .ToListAsync();

                        for (int i = 4; i > 0; i--)
                        {
                            var month = Clock.Now.AddMonths(-i);
                            var transMonth = transMonthly.FindAll(x => x.PaymentTime.Month == month.Month);
                            if (transMonth.Count == 0) data.Add(new SalesData(month.ToShortDateString(), 0, 0));
                            else
                            {
                                var item = new SalesData(month.ToShortDateString(), transMonth.Sum(x => x.Amount), transMonth.Count);
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
    }
}
