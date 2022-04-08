using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Uow;
using Abp.Linq.Extensions;
using Abp.MultiTenancy;
using Abp.Runtime.Caching;
using Abp.UI;
using KonbiCloud.Authorization;
using KonbiCloud.CloudSync;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.EntityFrameworkCore;
using KonbiCloud.Features.Custom;
using KonbiCloud.Plate;
using KonbiCloud.PlateMenu.Dtos;
using KonbiCloud.PlateMenus;
using KonbiCloud.PlateMenus.Dtos;
using KonbiCloud.Prices;
using KonbiCloud.Products;
using KonbiCloud.RFIDTable.Cache;
using KonbiCloud.Sessions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.Transactions;

namespace KonbiCloud.PlateMenu
{
    [AbpAuthorize(AppPermissions.Pages_PlateMenus)]
    public class PlateMenusAppService : KonbiCloudAppServiceBase, IPlateMenusAppService
    {
        private readonly IRepository<MenuSchedule.PlateMenu, Guid> _plateMenuRepository;
        private readonly IRepository<Plate.Plate, Guid> _plateRepository;
        private readonly IRepository<Session, Guid> _sessionRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<PriceStrategyCode, int> _priceStrategyCodeRepository;
        private readonly IRepository<PriceStrategy, Guid> _priceStrategyRepository;
		private readonly IRfidTableFeatureChecker rfidTableFeatureChecker;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ICacheManager _cacheManager;
        private readonly IPlateMenuSyncService _plateMenuSyncService;
        private readonly IConfigurationRoot _appConfiguration;
        private IIocResolver _iocResolver;
        private readonly IDetailLogService detailLogService;
        private const string noImage = "assets/common/images";
        private string _serverUrl;
        public string ServerUrl
        {
            get
            {
                if (SettingManager != null && string.IsNullOrEmpty(_serverUrl))
                {
                    _serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                    if (_serverUrl == null)
                    {
                        _serverUrl = "";
                    }
                }
                return _serverUrl;
            }
        }

        public PlateMenusAppService(IRepository<MenuSchedule.PlateMenu, Guid> plateMenuRepository,
                                    IRepository<Plate.Plate, Guid> plateRepository,
                                    IRepository<Session, Guid> sessionRepository,
                                    IRepository<Product, Guid> productRepository,
                                    IRepository<PriceStrategyCode, int> priceStrategyCodeRepository,
                                    IRepository<PriceStrategy, Guid> priceStrategyRepository,
									IRfidTableFeatureChecker rfidTableFeatureChecker,
                                    IUnitOfWorkManager unitOfWorkManager, ICacheManager cacheManager,
                                    IPlateMenuSyncService plateMenuSyncService,
                                    IHostingEnvironment env,
                                    IIocResolver iocResolver,
                                    IDetailLogService detailLog)
        {
            _plateMenuRepository = plateMenuRepository;
            _plateRepository = plateRepository;
            _sessionRepository = sessionRepository;
            _productRepository = productRepository;
            _priceStrategyCodeRepository = priceStrategyCodeRepository;
            _priceStrategyRepository = priceStrategyRepository;
			this.rfidTableFeatureChecker = rfidTableFeatureChecker;
            _unitOfWorkManager = unitOfWorkManager;
            _cacheManager = cacheManager;
            _plateMenuSyncService = plateMenuSyncService;
            _appConfiguration = env.GetAppConfiguration();
            _iocResolver = iocResolver;
            this.detailLogService = detailLog;
        }

        public async Task<PagedResultDto<PlateMenuDto>> GetAllPlateMenus(PlateMenusInput input)
        {
            try
            {
                var plateMenus = _plateMenuRepository.GetAllIncluding()
                    .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter),
                        e => e.Plate.Name.ToLower().Contains(input.NameFilter.ToLower().Trim()))
                    .WhereIf(!string.IsNullOrWhiteSpace(input.CodeFilter),
                        e => e.Plate.Code.ToLower().Contains(input.CodeFilter.ToLower().Trim()))
                    .WhereIf(input.CategoryFilter > 0, e => e.Plate.PlateCategoryId == input.CategoryFilter)
                    .WhereIf(!string.IsNullOrWhiteSpace(input.ColorFilter),
                        e => e.Plate.Color.ToLower().Contains(input.ColorFilter.ToLower().Trim()))
                    .WhereIf(input.DateFilter != null, e => e.SelectedDate.Date == input.DateFilter.Value.Date)
                    .WhereIf(!string.IsNullOrWhiteSpace(input.SessionFilter) && !"0".Equals(input.SessionFilter),
                        e => e.SessionId.ToString().Equals(input.SessionFilter.Trim()))
                    .Include(x => x.Plate)
                    .Include(x => x.Plate.PlateCategory)
                    .Include(x => x.PriceStrategies); ;

                var totalCount = await plateMenus.CountAsync();
                var sortedList = await plateMenus
                    .OrderBy(input.Sorting ?? "plate.name asc, plate.code")
                    .PageBy(input)
                    .ToListAsync();

                var dto = sortedList.Select(x => new PlateMenuDto()
                {
                    Id = x.Id.ToString(),
                    Plate = new Plate.Plate
                    {
                        Name = x.Plate.Name,
                        ImageUrl = x.Plate.ImageUrl,
                        Desc = x.Plate.Desc,
                        Code = x.Plate.Code,
                        Color = x.Plate.Color
                    },
                    Price = x.Price,
                    CategoryName = x.Plate.PlateCategory == null ? null : x.Plate.PlateCategory.Name,
                    PriceStrategyId = (x.PriceStrategies == null || !x.PriceStrategies.Any()) ? "" : x.PriceStrategies.ToList()[0].Id.ToString(),
                    PriceStrategy = x.ContractorPrice.HasValue ? x.ContractorPrice : 0,
                    Session = x.Session
                }).ToList();

                foreach (var d in dto)
                {
                    if (d.Plate != null && d.Plate.ImageUrl != null)
                    {
                        var pathImage = Path.Combine(ServerUrl, Const.ImageFolder, _appConfiguration[AppSettingNames.PlateImageFolder]);

                        if (d.Plate.ImageUrl.Contains(noImage)) continue;

                        var arrImage = d.Plate.ImageUrl.Split("|");
                        var images = "";
                        for (int index = 0; index < arrImage.Length; index++)
                        {
                            if (index == arrImage.Length - 1)
                            {
                                images += Path.Combine(pathImage, arrImage[index]);
                            }
                            else
                            {
                                images += Path.Combine(pathImage, arrImage[index]) + '|';
                            }
                        }
                        d.Plate.ImageUrl = images;
                    }
                }
                detailLogService.Log($"Get all plate menus: {dto.Count}");
                return new PagedResultDto<PlateMenuDto>(totalCount, dto);
            }
            catch (Exception ex)
            {
                Logger.Error($"Get plate menu: {ex.Message}", ex);
                return new PagedResultDto<PlateMenuDto>(0, new List<PlateMenuDto>());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Edit)]
        public async Task<bool> UpdatePrice(PlateMenusInput input)
        {
            try
            {
                if (Guid.TryParse(input.Id, out Guid id))
                {
                    var plate = await _plateMenuRepository.FirstOrDefaultAsync(id);
                    plate.Price = input.Price;
                    await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return false;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Edit)]
        public async Task<bool> UpdatePriceStrategy(PlateMenusInput input)
        {
            try
            {
                if (Guid.TryParse(input.PriceStrategyId, out Guid id))
                {
                    var ps = await _priceStrategyRepository.FirstOrDefaultAsync(id);
                    await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    ps.Value = input.PriceStrategy;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return false;
            }
        }

        public async Task<ImportResult> ImportPlateMenu(List<ImportData> input)
        {
            try
            {
                var listError = new List<int>();
                var listSuccess = new List<ImportData>();
                var plates = await _plateRepository.GetAllListAsync();
                var sessions = await _sessionRepository.GetAllListAsync();

                for (int i = 0; i < input.Count; i++)
                {
                    var dto = input[i];

                    if(string.IsNullOrEmpty(dto.PlateCode) || string.IsNullOrEmpty(dto.SessionName) || string.IsNullOrEmpty(dto.SelectedDate))
                    {
                        listError.Add(i + 2);
                        continue;
                    }

                    //Check duplicate
                    if (listSuccess.Any(x => x.PlateCode.Equals(dto.PlateCode) && x.SessionName.Equals(dto.SessionName) && x.SelectedDate.Equals(dto.SelectedDate)))
                    {
                        listError.Add(i + 2);
                        continue;
                    }

                    //Check plate exist
                    var plate = plates.FirstOrDefault(x => x.Code.ToLower().Equals(dto.PlateCode.ToLower()));
                    if (plate == null)
                    {
                        listError.Add(i + 2);
                        continue;
                    }

                    //Check session exist
                    var session = sessions.FirstOrDefault(x => x.Name.ToLower().Equals(dto.SessionName.ToLower()));
                    if (session == null)
                    {
                        listError.Add(i + 2);
                        continue;
                    }

                    var date = DateTime.Now;
                    try
                    {
                        date = DateTime.Parse(dto.SelectedDate, new CultureInfo("en-SG"));
                    }
                    catch
                    {
                        listError.Add(i + 2);
                        continue;
                    }
                    //Check duplicate
                    if (_plateMenuRepository.FirstOrDefault(x => x.PlateId == plate.Id && x.SessionId == session.Id && x.SelectedDate.Date == date.Date) != null)
                    {
                        listError.Add(i + 2);
                        continue;
                    }

                    decimal price = 0;
                    decimal.TryParse(dto.Price, out price);

                    var plateMenu = new MenuSchedule.PlateMenu
                    {
                        Id = Guid.NewGuid(),
                        TenantId = AbpSession.TenantId,
                        PlateId = plate.Id,
                        SessionId = session.Id,
                        SelectedDate = date,
                        Price = price
                    };
                    listSuccess.Add(dto);
                    await _plateMenuRepository.InsertAsync(plateMenu);
                }
                if(!listError.Any())
                {
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                var result = new ImportResult
                {
                    ErrorCount = listError.Count,
                    SuccessCount = listSuccess.Count,
                    ErrorList = string.Join(", ", listError.Take(100).Select(x => x.ToString()).ToArray())
                };
                if(listError.Count > 100)
                {
                    result.ErrorList = string.Join(", ", listError.Take(100).Select(x => x.ToString()).ToArray()) + "...";
                }
                else
                {
                    result.ErrorList = string.Join(", ", listError.Take(100).Select(x => x.ToString()).ToArray());
                }
                await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"Import Plate Menu Error: {ex.Message}", ex);
                return new ImportResult
                {
                    ErrorCount = 0,
                    SuccessCount = 0,
                    ErrorList = "Error"
                };
            }
        }

        [AbpAllowAnonymous]
        public async Task<bool> SyncPlateMenuData()
        {
            var mId = Guid.Empty;
            Guid.TryParse(await SettingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);
            if (mId == Guid.Empty)
            {
                throw new UserFriendlyException("Machine configuration error");
            }

            var syncedList = new SyncedItemData<Guid> { MachineId = mId, SyncedItems = new List<Guid>() };
            var products = new List<Product>();
            var sessions = new List<Session>();
            var plates = new List<Plate.Plate>();
            var existPlateMenus = new List<MenuSchedule.PlateMenu>();
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
            {
                products = await _productRepository.GetAllListAsync();
                sessions = await _sessionRepository.GetAllListAsync();
                plates = await _plateRepository.GetAllListAsync();
                existPlateMenus = _plateMenuRepository.GetAllIncluding()
                                    .Where(x => x.SelectedDate.Date == DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc).Date)
                                    .Include(x => x.PriceStrategies).ToList();
            }
            var plateMenus = await _plateMenuSyncService.Sync(mId);
            if (plateMenus == null)
            {
                throw new UserFriendlyException("Cannot get Plate Menus from server");
            }
            try
            {
                foreach (var pm in plateMenus)
                {
                    if (!plates.Any(x => x.Id == pm.PlateId)) continue;
                    if (pm.Session != null && !sessions.Any(x => x.Id == pm.Session.Id))
                    {
                        pm.Session.TenantId = AbpSession.TenantId;
                        await _sessionRepository.InsertAsync(pm.Session);
                        pm.Session = null;
                    }
                    if (pm.Product != null && !products.Any(x => x.Id == pm.Product.Id))
                    {
                        if (AbpSession.TenantId.HasValue)
                        {
                            pm.Product.TenantId = AbpSession.TenantId.Value;
                        }
                        await _productRepository.InsertAsync(pm.Product);
                        pm.Product = null;
                    }

                    var existPM = existPlateMenus.FirstOrDefault(x => x.Id == pm.Id);
                    if (existPM != null)
                    {
                        existPM.IsDeleted = pm.IsDeleted;
                        existPM.PlateId = pm.PlateId;
                        existPM.ProductId = pm.ProductId;
                        existPM.Price = pm.Price;
                        existPM.SessionId = pm.SessionId;
                        existPM.ContractorPrice = pm.ContractorPrice;
                        if (existPM.PriceStrategies != null)
                        {
                            foreach (var ps in existPM.PriceStrategies)
                            {
                                ps.Value = pm.ContractorPrice;
                            }
                        }
                    }
                    else
                    {
                        pm.TenantId = AbpSession.TenantId;
                        if (pm.PriceStrategies != null && AbpSession.TenantId.HasValue)
                        {
                            foreach (var ps in pm.PriceStrategies)
                            {
                                ps.TenantId = AbpSession.TenantId.Value;
                            }
                        }
                        await _plateMenuRepository.InsertAsync(pm);
                    }

                    syncedList.SyncedItems.Add(pm.Id);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
                //Update sync status to server
                _plateMenuSyncService.UpdateSyncStatus(syncedList);
                await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Sync PM: {ex.Message}", ex);
                throw new UserFriendlyException("Sync Plate Menus failed");
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Replicate)]
        public async Task ReplicatePlateMenu(ReplicateInput input)
        {
            try
            {
                var pmToClone = await _plateMenuRepository.GetAllIncluding()
                                .Where(x => x.SelectedDate.Date == input.DateFilter.Date)
                                .Include(x => x.PriceStrategies).ToListAsync();
                if (!pmToClone.Any()) return;

                var futurePm = await _plateMenuRepository.GetAllIncluding()
                                .Where(x => x.SelectedDate.Date >= input.DateFilter.Date.AddDays(1)
                                            && x.SelectedDate.Date <= input.DateFilter.Date.AddDays(input.Days))
                                .Include(x => x.PriceStrategies).ToListAsync();

                var addList = new List<MenuSchedule.PlateMenu>();
                var updateList = new List<MenuSchedule.PlateMenu>();

                for (int i = 1; i <= input.Days; i++)
                {
                    var cloneDate = input.DateFilter.AddDays(i);
                    var futurePmAtCloneDate = futurePm.Where(x => x.SelectedDate.Date == cloneDate.Date);
                    var hasData = futurePmAtCloneDate.Any();
                    foreach (var pm in pmToClone)
                    {
                        if (!hasData)
                        {
                            var newPm = AddNewPlateMenu(pm, cloneDate);
                            addList.Add(newPm);
                            continue;
                        }

                        var futurePmAtCloneDateAndSession = futurePmAtCloneDate.Where(x => x.SessionId == pm.SessionId);
                        if (!futurePmAtCloneDateAndSession.Any())
                        {
                            var newPm = AddNewPlateMenu(pm, cloneDate);
                            addList.Add(newPm);
                            continue;
                        }

                        var existPm = futurePmAtCloneDateAndSession.FirstOrDefault(x => x.PlateId == pm.PlateId);
                        if (existPm == null)
                        {
                            var newPm = AddNewPlateMenu(pm, cloneDate);
                            addList.Add(newPm);
                        }
                        else
                        {
                            existPm.Price = pm.Price;
                            existPm.ContractorPrice = pm.ContractorPrice;
                            existPm.TenantId = pm.TenantId;
                            existPm.ProductId = pm.ProductId;
                            existPm.SessionId = pm.SessionId;

                            //PriceStrategy
                            if (pm.PriceStrategies != null)
                            {
                                if (existPm.PriceStrategies == null) existPm.PriceStrategies = new List<PriceStrategy>();
                                foreach (var ps in pm.PriceStrategies)
                                {
                                    var existPs = existPm.PriceStrategies.FirstOrDefault(x => x.PriceCodeId == ps.PriceCodeId);
                                    if (existPs != null)
                                    {
                                        existPs.Value = ps.Value;
                                    }
                                    else
                                    {
                                        existPm.PriceStrategies.Add(
                                        new PriceStrategy
                                        {
                                            Id = Guid.NewGuid(),
                                            TenantId = ps.TenantId,
                                            Value = ps.Value,
                                            PriceCodeId = ps.PriceCodeId
                                        });
                                    }
                                }
                            }
                            else
                            {
                                if (existPm.PriceStrategies != null)
                                {
                                    existPm.PriceStrategies.Clear();
                                }
                            }
                            updateList.Add(existPm);
                        }
                    }
                }

                using (var uowManager = _iocResolver.ResolveAsDisposable<IUnitOfWorkManager>())
                {
                    using (var uow = uowManager.Object.Begin(TransactionScopeOption.Suppress))
                    {
                        var dbContext = uowManager.Object.Current.GetDbContext<KonbiCloudDbContext>(MultiTenancySides.Tenant);

                        await dbContext.AddRangeAsync(addList);
                        dbContext.UpdateRange(updateList);
                        uow.Complete();
                        detailLogService.Log($"Replicate PM: Done");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Replicate PM: {ex.Message}", ex);
            }
        }
        private MenuSchedule.PlateMenu AddNewPlateMenu(MenuSchedule.PlateMenu pm, DateTime cloneDate)
        {
            var newPm = new MenuSchedule.PlateMenu
            {
                Id = Guid.NewGuid(),
                TenantId = pm.TenantId,
                PlateId = pm.PlateId,
                Price = pm.Price,
                SessionId = pm.SessionId,
                ContractorPrice = pm.ContractorPrice,
                ProductId = pm.ProductId,
                SelectedDate = cloneDate
            };
            if (pm.PriceStrategies != null)
            {
                newPm.PriceStrategies = new List<PriceStrategy>();
                foreach (var ps in pm.PriceStrategies)
                {
                    newPm.PriceStrategies.Add(
                        new PriceStrategy
                        {
                            Id = Guid.NewGuid(),
                            TenantId = ps.TenantId,
                            Value = ps.Value,
                            PriceCodeId = ps.PriceCodeId
                        });
                }
            }
            return newPm;
        }

        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Generate)]
        public async Task<bool> GeneratePlateMenu(ReplicateInput input)
        {
            try
            {
                var sessions = await _sessionRepository.GetAllListAsync();
                var plates = await _plateRepository.GetAllListAsync();
                var pmDate = await _plateMenuRepository.GetAllListAsync(x => x.SelectedDate.Date == input.DateFilter.Date);
                var priceStrategyContractor = new PriceStrategyCode();
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant))
                {
                    priceStrategyContractor = await _priceStrategyCodeRepository.FirstOrDefaultAsync(x => x.Code.Equals(PlateConsts.Contractor));
                }

                var priceStrategiesOfContractor = new List<PriceStrategy>();
                if (priceStrategyContractor != null && priceStrategyContractor.Id > 0)
                {
                    priceStrategiesOfContractor = await _priceStrategyRepository.GetAllListAsync(x => x.PriceCodeId == priceStrategyContractor.Id);
                }
                bool hasChange = false;
                foreach (var session in sessions)
                {
                    //Get platemenu by session and date
                    var pmSessionDate = pmDate.Where(x => x.SessionId == session.Id);

                    var priceStrategies = priceStrategiesOfContractor.Where(x => pmSessionDate.Any(y => y.Id == x.PlateMenuId));

                    foreach (var plate in plates)
                    {
                        //Check if menu schedule exist
                        var pm = pmSessionDate.FirstOrDefault(x => x.PlateId == plate.Id);
                        if (pm != null)
                        {
                            if (priceStrategies.Any())
                            {
                                //Check price strategy exist
                                var ps = priceStrategies.FirstOrDefault(x => x.PlateMenuId == pm.Id);
                                if (ps == null)
                                {
                                    //Insert new price strategy
                                    await _priceStrategyRepository.InsertAsync(new PriceStrategy
                                    {
                                        Id = Guid.NewGuid(),
                                        TenantId = AbpSession.TenantId == null ? 1 : AbpSession.TenantId.Value,
                                        Value = 0,
                                        PriceCodeId = priceStrategyContractor.Id,
                                        PlateMenuId = pm.Id
                                    });
                                    hasChange = true;
                                }
                            }
                            continue;
                        }

                        //Generate data
                        var newPm = new MenuSchedule.PlateMenu
                        {
                            Id = Guid.NewGuid(),
                            TenantId = AbpSession.TenantId,
                            PlateId = plate.Id,
                            Price = 0,
                            SessionId = session.Id,
                            SelectedDate = input.DateFilter
                        };
                        if (priceStrategyContractor != null && priceStrategyContractor.Id > 0)
                        {
                            newPm.PriceStrategies = new List<PriceStrategy>
                        {
                            new PriceStrategy
                            {
                                Id = Guid.NewGuid(),
                                TenantId = AbpSession.TenantId == null ? 1 : AbpSession.TenantId.Value,
                                Value = 0,
                                PriceCodeId = priceStrategyContractor.Id
                            }
                        };
                        }

                        await _plateMenuRepository.InsertAsync(newPm);
                        hasChange = true;
                    }
                }
                if (hasChange)
                {
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw new UserFriendlyException("Generate Plate Menus failed");
            }
        }


        public async Task<List<PlateMenuDayResult>> GetDayPlateMenusDetail(DateTime inputDate)
        {
            try
            {
                var plateMenus = await _plateMenuRepository.GetAllIncluding()
                    .Where(e => e.SelectedDate.Date == inputDate.Date)
                    .Include(x => x.Session)
                    .GroupBy(x => x.Session)
                    .ToListAsync();

                plateMenus = plateMenus.FindAll(x => x.Key != null).OrderBy(x => x.Key.FromHrs).ToList();

                var listResult = new List<PlateMenuDayResult>();

                for (int i = 0; i < plateMenus.Count; i++)
                {
                    var sessionMenu = plateMenus[i].ToList();
                    var plateMenu = new PlateMenuDayResult
                    {
                        SessionId = plateMenus[i].Key.Id.ToString(),
                        SessionName = plateMenus[i].Key.Name,
                        TotalSetPrice = sessionMenu.FindAll(x => x.Price != 0).Count,
                        TotalNoPrice = sessionMenu.FindAll(x => x.Price == 0).Count
                    };
                    listResult.Add(plateMenu);
                }
                return listResult;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new List<PlateMenuDayResult>();
            }
        }

        public async Task<List<string>> GetWeekPlateMenus()
        {
            var currentDay = DateTime.Now;
            var endNext7Day = currentDay.AddDays(7);
            try
            {
                var result = new List<string>();

                var plateMenus = await _plateMenuRepository.GetAllIncluding()
                    .Where(e => e.SelectedDate.Date >= currentDay.Date && e.SelectedDate.Date <= endNext7Day.Date)
                    .GroupBy(e => e.SelectedDate)
                    .ToListAsync();

                for (int i = 0; i < 7; i++)
                {
                    if(!plateMenus.Any(x => x.Key.Date == currentDay.Date))
                    {
                        result.Add(currentDay.Date.ToString("dd/MM/yyyy"));
                    }
                    currentDay = currentDay.AddDays(1);
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new List<string>();
            }
        }

        public async Task<List<PlateMenuMonthResult>> GetMonthPlateMenus(DateTime inputDate)
        {
           // DateTime inputDate = DateTime.Now;
            var firstDayOfMonth = new DateTime(inputDate.Year, inputDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            try
            {
                var plateMenus = await _plateMenuRepository.GetAllIncluding()
                    .Where(e => e.SelectedDate.Date >= firstDayOfMonth.Date && e.SelectedDate.Date <= lastDayOfMonth.Date)
                    .GroupBy(e => e.SelectedDate)
                    .ToListAsync();

                var listResult = new List<PlateMenuMonthResult>();

                for (int i = 0; i < plateMenus.Count; i++)
                {
                    var dailyMenu = plateMenus[i].ToList();
                    var plateMenu = new PlateMenuMonthResult
                    {
                        Day = plateMenus[i].Key.ToString("MM-dd-yyy"),
                        TotalSetPrice = dailyMenu.FindAll(x => x.Price != 0).Count,
                        TotalNoPrice = dailyMenu.FindAll(x => x.Price == 0).Count
                    };
                    listResult.Add(plateMenu);
                }
                return listResult;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new List<PlateMenuMonthResult>();
            }
        }
    }
}