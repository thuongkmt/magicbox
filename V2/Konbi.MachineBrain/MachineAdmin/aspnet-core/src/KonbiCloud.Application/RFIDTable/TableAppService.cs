using Abp.Application.Services;
using Abp.Domain.Repositories;
using KonbiCloud.Machines;
using KonbiCloud.MenuSchedule;
using KonbiCloud.RFIDTable.Cache;
using KonbiCloud.Sessions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KonbiCloud.Sessions;
using KonbiCloud.Transactions;
using System.Threading.Tasks;
using KonbiCloud.CloudSync;
using KonbiCloud.Plate;
using KonbiCloud.Configuration;
using KonbiCloud.Common;
using Abp.Configuration;
using System.IO;
using Abp;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace KonbiCloud.RFIDTable
{
    public class TableAppService : AbpServiceBase, ITableAppService
    {
        private readonly IRepository<Session, Guid> sessionRepository;
        private readonly IRepository<MenuSchedule.PlateMenu, Guid> plateMenuRepository;

        private readonly IRepository<DetailTransaction, long> transactionRepository;
        private readonly IRepository<Disc, Guid> discRepository;
        private readonly IRepository<Plate.Plate, Guid> plateRepository;
        private readonly IRepository<Plate.Tray, Guid> trayRepository;
        private readonly ISettingManager _settingManager;        
        private readonly ITransactionSyncService transactionSyncService;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IDetailLogService detailLogService;

        protected bool PreventSellingSamePlate => _settingManager.GetSettingValue<bool>(AppSettingNames.PreventSellingSamePlateInASession);

        public TableAppService(ISettingManager settingManager,
                               IRepository<Session, Guid> sessionRepository,
                               IRepository<MenuSchedule.PlateMenu, Guid> plateMenuRepository,
                               IRepository<DetailTransaction, long> transactionRepository,
                               IRepository<Disc, Guid> discRepository,
                               IRepository<Plate.Plate, Guid> plateRepository,
                               ITransactionSyncService transactionSyncService,
                               IRepository<Plate.Tray, Guid> trayRepository,
                               IHostingEnvironment env,
                               IDetailLogService detailLog)
        {
            this.sessionRepository = sessionRepository;
            this.plateMenuRepository = plateMenuRepository;
            this.transactionRepository = transactionRepository;
            this.discRepository = discRepository;
            this.plateRepository = plateRepository;
            this.transactionSyncService = transactionSyncService;
            this.trayRepository = trayRepository;
            _settingManager = settingManager;
            _appConfiguration = env.GetAppConfiguration();
            this.detailLogService = detailLog;
        }

        public async Task<long> GenerateTransactionAsync(TransactionInfo transactionInfo)
        {
            try
            {
                var newTran = new DetailTransaction()
                {
                    TranCode = transactionInfo.Id,
                    Amount = transactionInfo.Amount,
                    CashlessDetail = new CashlessDetail(),
                    PaymentState = transactionInfo.PaymentState,
                    PaymentTime = DateTime.Now,
                    PaymentType = transactionInfo.PaymentType,
                    StartTime = DateTime.Now,
                    Status = transactionInfo.PaymentState == Konbini.Messages.Enums.PaymentState.Success ? Enums.TransactionStatus.Success : Enums.TransactionStatus.Error,
                    Buyer = transactionInfo.Buyer,
                    BeginTranImage = transactionInfo.BeginTranImage,
                    EndTranImage = transactionInfo.EndTranImage
                };
                var session = await sessionRepository.FirstOrDefaultAsync(el => el.Id == transactionInfo.SessionId);
                newTran.SessionId = session?.Id;

                var mId = Guid.Empty;
                Guid.TryParse(await SettingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);
                if (mId != Guid.Empty)
                {
                    newTran.MachineId = mId;
                }
                newTran.MachineName = await SettingManager.GetSettingValueAsync(AppSettingNames.MachineName);

                newTran.Dishes = new List<DishTransaction>();
                if (transactionInfo.MenuItems != null && transactionInfo.MenuItems.Any())
                {
                    var dishes = await discRepository.GetAllListAsync();
                    foreach (var mn in transactionInfo.MenuItems)
                    {
                        var disc = dishes.FirstOrDefault(x => x.Code.Equals(mn.Plate.Code) && x.Uid.Equals(mn.Plate.Uid));
                        newTran.Dishes.Add(new DishTransaction
                        {
                            Amount = mn.Price,
                            DiscId = disc?.Id
                        });
                    }
                }

                //Handle transaction image
                var tranImgFolderPath = Path.Combine(Directory.GetCurrentDirectory(), Const.ImageFolder, _appConfiguration[AppSettingNames.TransactionImageFolder]);

                if(File.Exists(newTran.BeginTranImage))
                {
                    var beginImgName = $"{newTran.TranCode}.{Const.Begin}.jpg";
                    var beginImgPath = Path.Combine(tranImgFolderPath, beginImgName);
                    File.Copy(newTran.BeginTranImage, beginImgPath);
                    if(File.Exists(beginImgPath))
                    {
                        File.Delete(newTran.BeginTranImage);
                        newTran.BeginTranImage = beginImgName;
                    }
                }
                if (File.Exists(newTran.EndTranImage))
                {
                    var endImgName = $"{newTran.TranCode}.{Const.End}.jpg";
                    var endImgPath = Path.Combine(tranImgFolderPath, endImgName);
                    File.Copy(newTran.EndTranImage, endImgPath);
                    if (File.Exists(endImgPath))
                    {
                        File.Delete(newTran.EndTranImage);
                        newTran.EndTranImage = endImgName;
                    }
                }

                var localId = await transactionRepository.InsertAndGetIdAsync(newTran);
                var tranSync = new DetailTransaction()
                {
                    Id = localId,
                    TranCode = newTran.TranCode,
                    Amount = newTran.Amount,
                    CashlessDetail = new CashlessDetail(),
                    PaymentState = newTran.PaymentState,
                    PaymentTime = DateTime.Now,
                    PaymentType = newTran.PaymentType,
                    StartTime = DateTime.Now,
                    Status = newTran.PaymentState == Konbini.Messages.Enums.PaymentState.Success ? Enums.TransactionStatus.Success : Enums.TransactionStatus.Error,
                    Buyer = newTran.Buyer,
                    SessionId = newTran.SessionId,
                    MachineId = newTran.MachineId,
                    MachineName = newTran.MachineName,
                    Dishes = newTran.Dishes?.Select(x => new DishTransaction
                    {
                        Amount = x.Amount,
                        DiscId = x.DiscId
                    }).ToList(),
                    BeginTranImage = newTran.BeginTranImage,
                    EndTranImage = newTran.EndTranImage
                };
                var beginOk = Common.Common.CompressImage(tranSync, tranImgFolderPath, detailLogService);
                if (!beginOk)
                {
                    return localId;
                }
                var endOk = Common.Common.CompressImage(tranSync, tranImgFolderPath, detailLogService, false);
                if (!endOk)
                {
                    return localId;
                }

                var result = await transactionSyncService.PushTransactionsToServer(new List<DetailTransaction> { tranSync });
                if (result.result != null && result.result.Any())
                {
                    var syncedTran = await transactionRepository.FirstOrDefaultAsync(localId);
                    if (syncedTran != null)
                    {
                        syncedTran.IsSynced = true;
                        syncedTran.SyncDate = DateTime.Now;
                    }
                }
                return localId;
            }
            catch (Exception ex)
            {
                Logger?.Error(ex.Message, ex);
                return 0;
            }
        }

        public async Task<SaleSessionCacheItem> GetSaleSessionInternalAsync()
        {
            SaleSessionCacheItem cacheItem = new SaleSessionCacheItem();
            var currentTime = Convert.ToInt32(string.Format("{0:00}{1:00}", DateTime.Now.Hour, DateTime.Now.Minute));
            var s = await sessionRepository.GetAll().Where(session => session.IsDeleted == false && currentTime > Convert.ToInt32(session.FromHrs) && currentTime <= Convert.ToInt32(session.ToHrs)).OrderBy(session => session.FromHrs)
              .FirstOrDefaultAsync();
            if (s != null)
            {

                cacheItem.SessionInfo = new SessionInfo() { Id = s.Id, Name = s.Name, FromHrs = s.FromHrs, ToHrs = s.ToHrs };

                cacheItem.MenuItems = await plateMenuRepository.GetAll().Where(el => el.IsDeleted == false && el.SelectedDate == DateTime.Today && el.SessionId == s.Id).Include(el => el.Plate).OrderBy(el => el.Plate.Name)
                    .Select(el =>
                    new MenuItemInfo()
                    {
                        Name = el.Plate.Name,
                        Code = el.Plate.Code,
                        PlateId = el.PlateId,
                        Color = el.Plate.Color,
                        Desc = el.Plate.Desc,
                        ImageUrl = el.Plate.Desc,
                        Price = el.Price.HasValue ? el.Price.Value : 0,
                        PriceContractor = el.PriceStrategies.FirstOrDefault().Value ?? 0


                    }).ToListAsync();
                cacheItem.Plates = await discRepository.GetAll().Where(el => el.IsDeleted == false && cacheItem.MenuItems.Any(item => item.PlateId == el.PlateId)).Select(el =>
                   new PlateInfo()
                   {
                       Code = el.Code,
                       PlateId = el.PlateId,
                       Uid = el.Uid
                   }
                ).ToListAsync();

                cacheItem.Trays = await trayRepository.GetAll().Where(el => el.IsDeleted == false).OrderBy(el => el.Name).Select(el => new TrayInfo()
                {
                    Name = el.Name,
                    Code = el.Code

                }).ToListAsync();

            }
            return cacheItem;
        }


        public string Validate(List<PlateReadingInput> plates, SaleSessionCacheItem cacheItem)
        {
            if (cacheItem.SessionInfo == null)
                return "Cannot find appropriate session";
            if (cacheItem.Plates == null || cacheItem.Plates.Count == 0)
                return "There is no plate registered in the system. Sales not ready.";
            if (cacheItem.MenuItems == null || cacheItem.MenuItems.Count == 0)
                return "There is no menu schedule defined for the session";
            // validate to ensure the plates have not sold for the session
            var foundSoldPlate = transactionRepository.GetAll().Any(el =>
            el.Status == Enums.TransactionStatus.Success
            && el.PaymentTime.Year == DateTime.Today.Year 
            && el.PaymentTime.Month == DateTime.Today.Month
            && el.PaymentTime.Day == DateTime.Today.Day
            && el.Session.Id == cacheItem.SessionInfo.Id && el.Dishes.Any(d => plates.Any(p => p.UID == d.Disc.Uid)));

            if (foundSoldPlate && PreventSellingSamePlate)
            {
                return "One of your plates has been sold earlier. Please pickup again";
            }
            return string.Empty;
        }
    }
}
