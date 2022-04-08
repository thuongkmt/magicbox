using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Runtime.Caching;
using Abp.UI;
using KonbiCloud.Authorization;
using KonbiCloud.CloudSync;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Features.Custom;
using KonbiCloud.Machines;
using KonbiCloud.MenuSchedule;
using KonbiCloud.Plate;
using KonbiCloud.PlateMenu.Dtos;
using KonbiCloud.PlateMenus;
using KonbiCloud.PlateMenus.Dtos;
using KonbiCloud.Prices;
using KonbiCloud.Products;
using KonbiCloud.RFIDTable.Cache;
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
        private readonly ICacheManager _cacheManager;
        private const string noImage = "assets/common/images";
        private readonly string serverUrl;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IRepository<Machine, Guid> _machineRepository;

        public PlateMenusAppService(IRepository<MenuSchedule.PlateMenu, Guid> plateMenuRepository,
                                    IRepository<Plate.Plate, Guid> plateRepository,
                                    IRepository<Session, Guid> sessionRepository,
                                    IRepository<Product, Guid> productRepository,
                                    IRepository<PriceStrategyCode, int> priceStrategyCodeRepository,
                                    IRepository<PriceStrategy, Guid> priceStrategyRepository,
                                    IRfidTableFeatureChecker rfidTableFeatureChecker,
                                    ICacheManager cacheManager,
                                    IHostingEnvironment env,
                                    IRepository<Machine, Guid> machineRepository)
        {
            _plateMenuRepository = plateMenuRepository;
            _plateRepository = plateRepository;
            _sessionRepository = sessionRepository;
            _productRepository = productRepository;
            _priceStrategyCodeRepository = priceStrategyCodeRepository;
            _priceStrategyRepository = priceStrategyRepository;
            this.rfidTableFeatureChecker = rfidTableFeatureChecker;
            _cacheManager = cacheManager;
            _appConfiguration = env.GetAppConfiguration();
            serverUrl = _appConfiguration["App:ServerRootAddress"];
            if (serverUrl == null)
            {
                serverUrl = "";
            }
            _machineRepository = machineRepository;
        }

        public async Task<PagedResultDto<PlateMenuDto>> GetAllPlateMenus(PlateMenusInput input)
        {
            try
            {
                //rfidTableFeatureChecker.CheckRfidTableFeatures(AbpSession.TenantId);

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
                    .Include(x => x.Plate).Include(x => x.Plate.PlateCategory);

                var dto = plateMenus.Select(x => new PlateMenuDto()
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
                    PriceStrategy = (x.PriceStrategies == null || !x.PriceStrategies.Any()) ? 0 : x.PriceStrategies.ToList()[0].Value,
                    Session = x.Session
                });

                var totalCount = await dto.CountAsync();
                var results = await dto
                    .OrderBy(input.Sorting ?? "plate.name asc, plate.code")
                    .PageBy(input)
                    .ToListAsync();

                foreach (var d in results)
                {
                    if (d.Plate != null && d.Plate.ImageUrl != null)
                    {
                        var pathImage = Path.Combine(serverUrl, Const.ImageFolder, _appConfiguration[AppSettingNames.PlateImageFolder]);

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

                return new PagedResultDto<PlateMenuDto>(totalCount, results);
            }
            catch (UserFriendlyException ue)
            {
                throw ue;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new PagedResultDto<PlateMenuDto>(0, new List<PlateMenuDto>());
            }
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

        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Edit)]
        public async Task<bool> UpdatePrice(PlateMenusInput input)
        {
            try
            {
                if (Guid.TryParse(input.Id, out Guid id))
                {
                    var plate = await _plateMenuRepository.FirstOrDefaultAsync(id);
                    if(plate != null)
                    {
                        plate.Price = input.Price;
                        await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    }
                    
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
                    if(ps != null)
                    {
                        ps.Value = input.PriceStrategy;
                    }
                }
                if (Guid.TryParse(input.Id, out Guid pmId))
                {
                    var plate = await _plateMenuRepository.FirstOrDefaultAsync(pmId);
                    if (plate != null)
                    {
                        plate.ContractorPrice = input.PriceStrategy;
                        await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return false;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Import)]
        public async Task<ImportResult> ImportPlateMenu(List<ImportData> input)
        {
            try
            {
                var listError = new List<string>();
                var listSuccess = new List<ImportData>();
                var plates = await _plateRepository.GetAllListAsync();
                var sessions = await _sessionRepository.GetAllListAsync();

                var priceStrategyContractor = new PriceStrategyCode();
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant))
                {
                    priceStrategyContractor = await _priceStrategyCodeRepository.FirstOrDefaultAsync(x => x.Code.Equals(PlateConsts.Contractor));
                }

                for (int i = 0; i < input.Count; i++)
                {
                    var dto = input[i];

                    if (string.IsNullOrEmpty(dto.PlateCode) || string.IsNullOrEmpty(dto.SessionName) || string.IsNullOrEmpty(dto.SelectedDate))
                    {
                        listError.Add($"&#8226; Row {i + 2}- Date, Session and Plate Code are mandatory");
                        continue;
                    }

                    //Check duplicate
                    if (listSuccess.Any(x => x.PlateCode.Equals(dto.PlateCode) && x.SessionName.Equals(dto.SessionName) && x.SelectedDate.Equals(dto.SelectedDate)))
                    {
                        listError.Add($"&#8226; Row {i + 2}- Duplicate Date, Session and Plate Code");
                        continue;
                    }

                    //Check plate exist
                    var plate = plates.FirstOrDefault(x => x.Code.ToLower().Equals(dto.PlateCode.ToLower()));
                    if (plate == null)
                    {
                        listError.Add($"&#8226; Row {i + 2}- Cannot find Plate with Code: {dto.PlateCode}");
                        continue;
                    }

                    //Check session exist
                    var session = sessions.FirstOrDefault(x => x.Name.ToLower().Equals(dto.SessionName.ToLower()));
                    if (session == null)
                    {
                        listError.Add($"&#8226; Row {i + 2}- Cannot find Session with Name: {dto.SessionName}");
                        continue;
                    }

                    var date = DateTime.Now;
                    try
                    {
                        date = DateTime.Parse(dto.SelectedDate, new CultureInfo("en-SG"));
                    }
                    catch
                    {
                        listError.Add($"&#8226; Row {i + 2}- Wrong datetime");
                        continue;
                    }

                    //Check exist
                    var test = _plateMenuRepository.GetAllIncluding()
                                                        .Where(x => x.PlateId == plate.Id && x.SessionId == session.Id && x.SelectedDate.Date == date.Date)
                                                        .Include(x => x.PriceStrategies).ToList();

                    MenuSchedule.PlateMenu existPm = null;
                    if(test != null && test.Any())
                    {
                        existPm = test[0];
                    }

                    decimal price = 0;
                    decimal.TryParse(dto.Price, out price);
                    decimal contractorPrice = 0;
                    decimal.TryParse(dto.ContractorPrice, out contractorPrice);

                    if (existPm == null)
                    {
                        var plateMenu = new MenuSchedule.PlateMenu
                        {
                            Id = Guid.NewGuid(),
                            TenantId = AbpSession.TenantId,
                            PlateId = plate.Id,
                            SessionId = session.Id,
                            SelectedDate = date,
                            Price = price,
                            ContractorPrice = contractorPrice
                        };

                        if (priceStrategyContractor != null && priceStrategyContractor.Id > 0)
                        {
                            plateMenu.PriceStrategies = new List<PriceStrategy>
                            {
                                new PriceStrategy
                                {
                                    Id = Guid.NewGuid(),
                                    TenantId = AbpSession.TenantId == null ? 1 : AbpSession.TenantId.Value,
                                    Value = contractorPrice,
                                    PriceCodeId = priceStrategyContractor.Id
                                }
                            };
                        }

                        await _plateMenuRepository.InsertAsync(plateMenu);
                    }
                    else
                    {
                        existPm.Price = price;
                        existPm.ContractorPrice = contractorPrice;
                        foreach (var ps in existPm.PriceStrategies)
                        {
                            ps.Value = contractorPrice;
                        }
                    }
                    listSuccess.Add(dto);
                }
                if (listSuccess.Any())
                {
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                var result = new ImportResult
                {
                    ErrorCount = listError.Count,
                    SuccessCount = listSuccess.Count
                };
                if (listError.Count > 100)
                {
                    result.ErrorList = string.Join("<br/>", listError.Take(100).Select(x => x.ToString()).ToArray()) + "...";
                }
                else
                {
                    result.ErrorList = string.Join("<br/>", listError.Take(100).Select(x => x.ToString()).ToArray());
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Import Plate Menu Error", ex);
                return new ImportResult
                {
                    ErrorCount = 0,
                    SuccessCount = 0,
                    ErrorList = "Error"
                };
            }
        }

        [AbpAllowAnonymous]
        public async Task<IList<MenuSchedule.PlateMenu>> GetPlateMenus(EntityDto<Guid> machineInput)
        {
            var plateMenus = new List<MenuSchedule.PlateMenu>();
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
            {
                var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == machineInput.Id);
                if (machine == null)
                {
                    Logger.Error($"Sync PlateMenu: MachineId: {machineInput.Id} does not exist");
                    return null;
                }
                else if (machine.IsDeleted)
                {
                    Logger.Error($"Sync PlateMenu: Machine with id: {machineInput.Id} is deleted");
                    return null;
                }

                var allPlateMenus = _plateMenuRepository.GetAllIncluding()
                                                        .Where(x => x.TenantId == machine.TenantId &&
                                                               x.SelectedDate.Date == DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc).Date)
                                                        .Include(x => x.PriceStrategies)
                                                        .Include(x => x.Product)
                                                        .Include(x => x.PlateMenuMachineSyncStatus);

                if (allPlateMenus == null) return plateMenus;

                foreach (var p in allPlateMenus)
                {
                    var pm = p.PlateMenuMachineSyncStatus.FirstOrDefault(x => x.MachineId.Equals(machineInput.Id));
                    if (pm == null || p.LastModificationTime > pm.SyncDate || p.DeletionTime > pm.SyncDate)
                    {
                        plateMenus.Add(p);
                    }
                }

                return plateMenus;
            }
        }

        [AbpAllowAnonymous]
        public async Task<bool> UpdateSyncStatus(SyncedItemData<Guid> syncData)
        {
            bool hasUpdate = false;
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
            {
                var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == syncData.MachineId);
                if (machine == null)
                {
                    Logger.Error($"Sync PlateMenu: MachineId: {syncData.MachineId} does not exist");
                    return false;
                }
                else if (machine.IsDeleted)
                {
                    Logger.Error($"Sync PlateMenu: Machine with id: {syncData.MachineId} is deleted");
                    return false;
                }

                var allPlateMenus = _plateMenuRepository.GetAllIncluding()
                                                        .Where(x => x.TenantId == machine.TenantId &&
                                                               x.SelectedDate.Date == DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc).Date)
                                                        .Include(x => x.PlateMenuMachineSyncStatus);

                foreach (var item in syncData.SyncedItems)
                {
                    var p = await allPlateMenus.FirstOrDefaultAsync(x => x.Id == item);
                    if (p == null) continue;

                    var pm = p.PlateMenuMachineSyncStatus.FirstOrDefault(x => x.MachineId.Equals(syncData.MachineId));
                    hasUpdate = true;
                    if (pm == null)
                    {
                        p.PlateMenuMachineSyncStatus.Add(
                            new PlateMenuMachineSyncStatus
                            {
                                Id = Guid.NewGuid(),
                                PlateMenuId = p.Id,
                                MachineId = syncData.MachineId,
                                IsSynced = true,
                                SyncDate = DateTime.Now
                            });
                        continue;
                    }
                    pm.SyncDate = DateTime.Now;
                }
            }
            if (hasUpdate)
            {
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            return true;
        }

        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Replicate)]
        public async Task ReplicatePlateMenu(ReplicateInput input)
        {
            try
            {
                var pmToClone = _plateMenuRepository.GetAllIncluding()
                                .Where(x => x.SessionId.ToString().Equals(input.SessionId)
                                        && x.SelectedDate.Date == input.DateFilter.Date)
                                .Include(x => x.PriceStrategies);
                if (!pmToClone.Any()) return;

                //var minDate = new DateTime(input.DateFilter.Year, input.DateFilter.Month, input.DateFilter.Day).AddDays(1).AddMilliseconds(-1);
                var futurePm = _plateMenuRepository.GetAllIncluding()
                                .Where(x => x.SessionId.ToString().Equals(input.SessionId)
                                            && x.SelectedDate.Date >= input.DateFilter.Date.AddDays(1)
                                            && x.SelectedDate.Date <= input.DateFilter.Date.AddDays(input.Days))
                                .Include(x => x.PriceStrategies);

                for (int i = 1; i <= input.Days; i++)
                {
                    var cloneDate = input.DateFilter.AddDays(i);
                    var futurePmAtCloneDate = futurePm.Where(x => x.SelectedDate.Date == cloneDate.Date);
                    foreach (var pm in pmToClone)
                    {
                        var existPm = futurePmAtCloneDate.FirstOrDefault(x => x.PlateId == pm.PlateId);
                        if(existPm == null)
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

                            await _plateMenuRepository.InsertAsync(newPm);
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
                                var updateList = new List<Guid>();
                                foreach (var ps in pm.PriceStrategies)
                                {
                                    var existPs = existPm.PriceStrategies.FirstOrDefault(x => x.PriceCodeId == ps.PriceCodeId);
                                    if(existPs != null)
                                    {
                                        existPs.Value = ps.Value;
                                        updateList.Add(existPs.Id);
                                        //await _priceStrategyRepository.UpdateAsync(existPs);
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
                                if(existPm.PriceStrategies != null)
                                {
                                    //foreach (var ps in existPm.PriceStrategies)
                                    //{
                                    //    await _priceStrategyRepository.DeleteAsync(ps.Id);
                                    //}
                                    existPm.PriceStrategies.Clear();
                                }
                            }
                            await _plateMenuRepository.UpdateAsync(existPm);
                        }
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
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
                    if (!plateMenus.Any(x => x.Key == currentDay))
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