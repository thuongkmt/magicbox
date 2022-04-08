using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using Abp.Timing;
using KonbiCloud.Dashboard.Dto;
using KonbiCloud.Dashboard.Dtos;
using KonbiCloud.Machines;
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
        private readonly IRepository<Transactions.DetailTransaction, long> _transactionRepository;
        private readonly IRepository<Session, Guid> _sessionRepository;
        private readonly ICacheManager _cacheManager;

        public DashboardAppService(IRepository<Transactions.DetailTransaction, long> transactionRepository, IRepository<Session, Guid> sessionRepository, ICacheManager cacheManager)
        {
            _transactionRepository = transactionRepository;
            _sessionRepository = sessionRepository;
            _cacheManager = cacheManager;
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
            if (type == "TotalTransSale")
            {
                return await _cacheManager
                    .GetCache("DashboardCache1_TotalTransSale")
                    .Get(type, () => GetTotalTransSale());
            }
            else if (type == "TotalTransToday")
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
            if (salesDatePeriod == SalesDatePeriod.Daily)
            {
                return await _cacheManager
                 .GetCache("DashboardCache2_Daily")
                 .Get("SalesData", () => GenerateSalesData(salesDatePeriod));
            }
            else if (salesDatePeriod == SalesDatePeriod.Weekly)
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
            var transactions = await _transactionRepository.GetAllListAsync();
            var total = transactions.Sum(item => item.Amount);
            return total;
        }

        private decimal GetTotalTransToday()
        {
            var transactions = _transactionRepository.GetAll()
                   .Where(e => e.PaymentTime.Date == Clock.Now.Date);
            var totalCount = transactions.Count();
            return totalCount;
        }

        private async Task<decimal> GetTotalTransCurrentSession()
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
                .Where(e => e.PaymentTime.Date == Clock.Now.Date)
                .Where(e => e.Session.Id == currentSession.Id);
            var totalCount = transactions.Count();
            return totalCount;
        }

        private async Task<decimal> GetTotalTransCurrentSessionSale()
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
                .Where(e => e.PaymentTime.Date == Clock.Now.Date)
                .Where(e => e.Session.Id == currentSession.Id);

            var total = transactions.Sum(item => item.Amount);
            return total;
        }

        private async Task<SalesDataOuput> GenerateSalesData(SalesDatePeriod salesDatePeriod)
        {
            List<SalesData> data = new List<SalesData>();

            switch (salesDatePeriod)
            {
                case SalesDatePeriod.Daily:

                    var EndDateDaily = Clock.Now;
                    var transDaily = await _transactionRepository.GetAll()
                       .Where(t => t.PaymentTime.Date >= EndDateDaily.AddDays(-5) && t.PaymentTime.Date <= EndDateDaily)
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
                      .Where(t => t.PaymentTime.Date >= EndDateWeekly.AddDays(-21) && t.PaymentTime.Date <= EndDateWeekly)
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
                      .Where(t => t.PaymentTime.Date >= EndDateMonthly.AddDays(-150) && t.PaymentTime.Date <= EndDateMonthly)
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

        private async Task<SessionStatDataOutput> GenerateSessionStatData()
        {
            var trans = await _transactionRepository.GetAllIncluding()
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

    }
}



//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Abp.Domain.Repositories;
//using Castle.Core.Logging;
//using KonbiCloud.Dashboard.Dtos;
//using KonbiCloud.Enums;
//using KonbiCloud.Machines;
//using KonbiCloud.Products;
//using KonbiCloud.Utils;

//namespace KonbiCloud.Dashboard
//{
//    public class DashboardAppService : KonbiCloudAppServiceBase, IDashboardAppService
//    {
//        private readonly IRepository<Machine, Guid> machineRepository;
//        private readonly IRepository<Product, Guid> productRepository;
//        private readonly IRepository<DetailTransaction, Guid> detailTransactionRepository;
//        private readonly IRepository<CurrentMachineLoadout, Guid> currentLoadoutRepository;
//        private readonly IRepository<LoadoutItemStatus, Guid> loadoutItemStatusRepository;
//        private readonly ILogger _logger;

//        public DashboardAppService(IRepository<Machine, Guid> machineRepository,
//            IRepository<Product, Guid> productRepository,
//            IRepository<DetailTransaction, Guid> detailTransactionRepository,
//            IRepository<CurrentMachineLoadout, Guid> currentLoadoutRepository,
//            IRepository<LoadoutItemStatus, Guid> loadoutItemStatusRepository,
//            ILogger logger)
//        {
//            this.machineRepository = machineRepository;
//            this.productRepository = productRepository;
//            this.detailTransactionRepository = detailTransactionRepository;
//            this.currentLoadoutRepository = currentLoadoutRepository;
//            this.loadoutItemStatusRepository = loadoutItemStatusRepository;
//            this._logger = logger;
//        }


//        public Task<ChartDto> GetChartDataInfomations()
//        {
//            throw new NotImplementedException();
//        }

//        public Task<MachineInventoryOverviewDto> GetDashboardMachineInventoryInfomations()
//        {
//            return Task.Run(() =>
//            {
//                MachineInventoryOverviewDto result = new MachineInventoryOverviewDto();
//                List<MachineInventoryOverviewItem> items = new List<MachineInventoryOverviewItem>();
//                MachineInventoryOverviewItem item = new MachineInventoryOverviewItem
//                {
//                    MachineId = Guid.NewGuid().ToString(),
//                    Name = "SOLAR V!",
//                    LeftOverNum = 1,
//                    OutOfStockNum = 2,
//                    DispenseErrorNum = 3
//                };
//                items.Add(item);

//                item = new MachineInventoryOverviewItem
//                {
//                    MachineId = Guid.NewGuid().ToString(),
//                    Name = "SOLAR V2",
//                    LeftOverNum = 1,
//                    OutOfStockNum = 2,
//                    DispenseErrorNum = 3
//                };
//                items.Add(item);


//                item = new MachineInventoryOverviewItem
//                {
//                    MachineId = Guid.NewGuid().ToString(),
//                    Name = "SOLAR V2",
//                    LeftOverNum = 1,
//                    OutOfStockNum = 2,
//                    DispenseErrorNum = 3
//                };
//                items.Add(item);

//                item = new MachineInventoryOverviewItem
//                {
//                    MachineId = Guid.NewGuid().ToString(),
//                    Name = "SOLAR V3",
//                    LeftOverNum = 1,
//                    OutOfStockNum = 2,
//                    DispenseErrorNum = 3
//                };
//                items.Add(item);

//                item = new MachineInventoryOverviewItem
//                {
//                    MachineId = Guid.NewGuid().ToString(),
//                    Name = "SOLAR V4",
//                    LeftOverNum = 1,
//                    OutOfStockNum = 2,
//                    DispenseErrorNum = 3
//                };
//                items.Add(item);


//                item = new MachineInventoryOverviewItem
//                {
//                    MachineId = Guid.NewGuid().ToString(),
//                    Name = "SOLAR V5",
//                    LeftOverNum = 1,
//                    OutOfStockNum = 2,
//                    DispenseErrorNum = 3
//                };
//                items.Add(item);


//                item = new MachineInventoryOverviewItem
//                {
//                    MachineId = Guid.NewGuid().ToString(),
//                    Name = "SOLAR V6",
//                    LeftOverNum = 1,
//                    OutOfStockNum = 2,
//                    DispenseErrorNum = 3
//                };
//                items.Add(item);


//                item = new MachineInventoryOverviewItem
//                {
//                    MachineId = Guid.NewGuid().ToString(),
//                    Name = "SOLAR V7",
//                    LeftOverNum = 1,
//                    OutOfStockNum = 2,
//                    DispenseErrorNum = 3
//                };
//                items.Add(item);

//                result.Items = items.ToArray();



//                return result;
//            });
//        }

//        private List<TransactionViewDto> createFakeTransactions()
//        {
//            List<TransactionViewDto> result = new List<TransactionViewDto>();
//            result.Add(new TransactionViewDto
//            {
//                MachineLogicalID = "VMH-MDC",
//                ProductName = "Chicken Pie",
//                LocationCode = "101",
//                TotalValue = "10",
//                DateTime = DateTime.Now.ToString()
//            });
//            result.Add(new TransactionViewDto
//            {
//                MachineLogicalID = "VMH-MDC",
//                ProductName = "Chicken Pie",
//                LocationCode = "101",
//                TotalValue = "10",
//                DateTime = DateTime.Now.ToString()
//            });
//            result.Add(new TransactionViewDto
//            {
//                MachineLogicalID = "VMH-MDC",
//                ProductName = "Chicken Pie",
//                LocationCode = "101",
//                TotalValue = "10",
//                DateTime = DateTime.Now.ToString()
//            });
//            result.Add(new TransactionViewDto
//            {
//                MachineLogicalID = "VMH-MDC",
//                ProductName = "Chicken Pie",
//                LocationCode = "101",
//                TotalValue = "10",
//                DateTime = DateTime.Now.ToString()

//            });
//            result.Add(new TransactionViewDto
//            {
//                MachineLogicalID = "VMH-MDC",
//                ProductName = "Chicken Pie",
//                LocationCode = "101",
//                TotalValue = "10",
//                DateTime = DateTime.Now.ToString()

//            });

//            result.Add(new TransactionViewDto
//            {
//                MachineLogicalID = "VMH-MDC",
//                ProductName = "Chicken Pie",
//                LocationCode = "101",
//                TotalValue = "10",
//                DateTime = DateTime.Now.ToString()
//            });

//            return result;
//        }
//        private ChartDtoItem createFakeSaleByMachineChartDto()
//        {
//            ChartDtoItem result = new ChartDtoItem();
//            result.Name = "Sale By Machine";
//            result.Data = new ChartDataItem[] { };

//            List<ChartDataItem> items = new List<ChartDataItem>();
//            items.Add(new ChartDataItem
//            {
//                X = "VMH-MDC",
//                Y = "100"
//            });

//            items.Add(new ChartDataItem
//            {
//                X = "VMC-CACC",
//                Y = "90"
//            });

//            items.Add(new ChartDataItem
//            {
//                X = "VMH-JTCSMT",
//                Y = "80"
//            });

//            items.Add(new ChartDataItem
//            {
//                X = "VMH-SAESL",
//                Y = "70"
//            });

//            items.Add(new ChartDataItem
//            {
//                X = "VMH-TMP2",
//                Y = "60"
//            });

//            items.Add(new ChartDataItem
//            {
//                X = "VMH-JTCSMT",
//                Y = "50"
//            });

//            items.Add(new ChartDataItem
//            {
//                X = "VMH-SITSP",
//                Y = "40"
//            });

//            items.Add(new ChartDataItem
//            {
//                X = "VMH-SITSP",
//                Y = "30"
//            });

//            result.Data = items.ToArray();

//            return result;
//        }

//        private ChartDtoItem createFakeSaleByProductChartDto()
//        {
//            ChartDtoItem result = new ChartDtoItem();
//            result.Name = "Sale By Product";
//            result.Data = new ChartDataItem[] { };

//            List<ChartDataItem> items = new List<ChartDataItem>();
//            items.Add(new ChartDataItem
//            {
//                X = "Coffee Peanut (3pcs/box) ",
//                Y = "100"
//            });

//            items.Add(new ChartDataItem
//            {
//                X = "Chocolate Rice (3pcs/box) ",
//                Y = "90"
//            });

//            items.Add(new ChartDataItem
//            {
//                X = "Strawberry Roll (2pcs/box) ",
//                Y = "80"
//            });

//            items.Add(new ChartDataItem
//            {
//                X = "Swiss Roll Slice (Coffee) 3pcs/box ",
//                Y = "70"
//            });

//            items.Add(new ChartDataItem
//            {
//                X = "Swiss Roll Slice (Strawberry)",
//                Y = "60"
//            });

//            items.Add(new ChartDataItem
//            {
//                X = "Swiss Roll Slices (Pandan) 3pcs/box",
//                Y = "50"
//            });

//            items.Add(new ChartDataItem
//            {
//                X = "Swiss Roll Slices (Chocolate) 3pcs/box ",
//                Y = "40"
//            });


//            result.Data = items.ToArray();

//            return result;
//        }

//        public async Task<OverviewDashboardDto> GetDashboardOverviewInfomations()
//        {
//            LocaTimeUtils locaTimeUtils = new LocaTimeUtils();
//            OverviewDashboardDto result = new OverviewDashboardDto();
//            List<OverViewDtoItem> items = new List<OverViewDtoItem>();
//            int numberMachine = await machineRepository.CountAsync();
//            OverViewDtoItem item = new OverViewDtoItem
//            {
//                NameOverview = "machine",
//                ExtInfo1 = numberMachine.ToString(),
//                ExtInfo2 = ""
//            };
//            items.Add(item);

//            int numberProduct = await productRepository.CountAsync();

//            item = new OverViewDtoItem
//            {
//                NameOverview = "product",
//                ExtInfo1 = numberProduct.ToString(),
//                ExtInfo2 = ""
//            };
//            items.Add(item);
//            DateTime todayFirstTime = locaTimeUtils.getStartTodayLocalTime();// temporal 
//            // reconstruct all items
//            //var allTransactionQuerry = detailTransactionRepository.GetAllList().Where(t => t.PaymentTime >= todayFirstTime).GroupBy(a => a.LocalId).Select(g => g.OrderByDescending(t => t.PaymentTime).FirstOrDefault());
//            var allTransactionQuerry = detailTransactionRepository.GetAllList().Where(t => t.PaymentTime >= todayFirstTime);

//            int numberTodayTransaction = allTransactionQuerry.Count();
//            int numberSuccessTransactionToday = allTransactionQuerry.Where(t => t.State != TransactionStatus.Error).Count();
//            int numberErrorTransactionToday = allTransactionQuerry.Where(t => t.State == TransactionStatus.Error).Count();

//            List<DetailTransaction> allTransactions = allTransactionQuerry.ToList();

//            List<DetailTransaction> listTop20Transactions = allTransactions.Where(t=>t.State != TransactionStatus.Error).OrderByDescending(t => t.CreationTime).Take(20).ToList();
//            result.Top20Transactions = listTop20Transactions.Select(t=>new TransactionViewDto {
//                MachineLogicalID = t.MachineLogicalId,
//                ProductName = t.ProductName,
//                LocationCode = t.LocationCode,
//                TotalValue = t.TotalValue.ToString(),
//                DateTime = t.PaymentTime != null ? t.PaymentTime.ToString():""
//            }).ToArray();


//            // transaction my machine
//            List<Machine> machines = await machineRepository.GetAllListAsync();
//            allTransactions = allTransactions.Where(t => t.State != TransactionStatus.Error).ToList();
//            foreach (var tran in allTransactions)
//            {
//                Machine machine = machines.Where(m => m.Id.ToString() == tran.MachineId).First();
//                tran.MachineLogicalId = machine != null ? machine.Name : "";
//            }

//            var sumaryByMachines = allTransactions.GroupBy(t => t.MachineLogicalId).Select(g => new { MachineLogicalID = g.Key, Amount = g.Count() }).ToList();
//            var sumaryByProductID = allTransactions.GroupBy(t => t.ProductName).Select(g => new { ProductName = g.Key, Amount = g.Count() }).ToList();


//            item = new OverViewDtoItem
//            {
//                NameOverview = "transaction",
//                ExtInfo1 = numberTodayTransaction.ToString(),
//                ExtInfo2 = numberSuccessTransactionToday.ToString(),
//                ExtInfo3 = numberErrorTransactionToday.ToString()
//            };
//            items.Add(item);

//            var allMachineLoadoutStatus = await currentLoadoutRepository.GetAllListAsync();
//            var activeMachineLoadout = new List<CurrentMachineLoadout>();

//            try
//            {
//                foreach (var ml in allMachineLoadoutStatus)
//                {
//                    var m = machines.FirstOrDefault(x => !x.IsDeleted && x.Id.ToString() == ml.MachineId);
//                    if (m == null)
//                    {
//                        var loadOutItems = await loadoutItemStatusRepository.GetAllListAsync(x => x.CurrentMachineLoadout.Id == ml.Id);
//                        foreach (var loadOutItem in loadOutItems)
//                        {
//                            await loadoutItemStatusRepository.DeleteAsync(loadOutItem.Id);
//                        }
//                        await currentLoadoutRepository.DeleteAsync(ml.Id);
//                        await CurrentUnitOfWork.SaveChangesAsync();
//                    }
//                    else
//                    {
//                        ml.MachineLogicalId = m.Name;
//                        activeMachineLoadout.Add(ml);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.Error("Error when delete MachineLoadoutStatus", ex);
//            }

//            long numLeftOver = activeMachineLoadout.Sum(m => m.LeftOver).Value;
//            int numOutOfStock = activeMachineLoadout.Sum(m => m.OutOfStock);
//            int numDispenseError = activeMachineLoadout.Sum(m => m.DispenseErrors);           
//            result.CurrentMachineLoadouts = activeMachineLoadout.ToArray();

//            item = new OverViewDtoItem
//            {
//                NameOverview = "stock",
//                ExtInfo1 = numLeftOver.ToString(),
//                ExtInfo2 = numOutOfStock.ToString(),
//                ExtInfo3 = numDispenseError.ToString()
//            };
//            items.Add(item);

//            result.Items = items.ToArray();

//            //fake 
//            //result.SaleByMachineData = createFakeSaleByMachineChartDto();
//            //result.SaleByProductData = createFakeSaleByProductChartDto();
//            //result.Top20Transactions = createFakeTransactions().ToArray();

//            result.SaleByMachineData = new ChartDtoItem
//            {
//                Name = "Sale by Machine",
//                Data = sumaryByMachines.Select(s=>new ChartDataItem { X=s.MachineLogicalID, Y=s.Amount.ToString()}).ToArray()
//            };

//            result.SaleByProductData = new ChartDtoItem
//            {
//                Name = "Sale by Product",
//                Data = sumaryByProductID.OrderByDescending(s=>s.Amount).Take(10).Select(s => new ChartDataItem { X = s.ProductName, Y = s.Amount.ToString() }).ToArray()
//            };

//            return result;
//        }
//    }
//}
