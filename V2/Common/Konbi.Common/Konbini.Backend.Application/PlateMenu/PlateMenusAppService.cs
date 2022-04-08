using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Runtime.Caching;
using Abp.UI;
using KonbiCloud.Authorization;
using KonbiCloud.Features.Custom;
using KonbiCloud.Plate;
using KonbiCloud.PlateMenu.Dtos;
using KonbiCloud.PlateMenus;
using KonbiCloud.PlateMenus.Dtos;
using KonbiCloud.Prices;
using KonbiCloud.Products;
using KonbiCloud.RFIDTable.Cache;
using KonbiCloud.Sessions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ICacheManager _cacheManager;

        public PlateMenusAppService(IRepository<MenuSchedule.PlateMenu, Guid> plateMenuRepository,
                                    IRepository<Plate.Plate, Guid> plateRepository,
                                    IRepository<Session, Guid> sessionRepository,
                                    IRepository<Product, Guid> productRepository,
                                    IRepository<PriceStrategyCode, int> priceStrategyCodeRepository,
                                    IRepository<PriceStrategy, Guid> priceStrategyRepository,
									IRfidTableFeatureChecker rfidTableFeatureChecker,
                                    IUnitOfWorkManager unitOfWorkManager, ICacheManager cacheManager)
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
        }

        public async Task<PagedResultDto<PlateMenuDto>> GetAllPlateMenus(PlateMenusInput input)
        {
            try
            {
                rfidTableFeatureChecker.CheckRfidTableFeatures(AbpSession.TenantId);
                if (input.DateFilter != null)
                {
                    var sessions = await _sessionRepository.GetAllListAsync();
                    var plates = await _plateRepository.GetAllListAsync();
                    foreach (var session in sessions)
                    {
                        //Get platemenu by session and date
                        var pmSessionDate = await _plateMenuRepository.GetAllListAsync(x => x.SelectedDate.Date == input.DateFilter.Value.Date && x.SessionId == session.Id);
                        var priceStrategyContractor = new PriceStrategyCode();
                        using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant))
                        {
                            priceStrategyContractor = await _priceStrategyCodeRepository.FirstOrDefaultAsync(x => x.Code.Equals(PlateConsts.Contractor));
                        }

                        var priceStrategies = new List<PriceStrategy>();
                        if (priceStrategyContractor != null && priceStrategyContractor.Id > 0)
                        {
                            priceStrategies = await _priceStrategyRepository.GetAllListAsync(x => x.PriceCodeId == priceStrategyContractor.Id
                                                                                            && pmSessionDate.Any(y => y.Id == x.PlateMenuId));
                        }

                        foreach (var plate in plates)
                        {
                            //Check if menu schedule exist
                            var pm = pmSessionDate.FirstOrDefault(x => x.PlateId == plate.Id);
                            if (pm != null)
                            {
                                if(priceStrategies.Any())
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
                                    }
                                }
                                continue;
                            }

                            //Generate data
                            var pmId = await _plateMenuRepository.InsertAndGetIdAsync(new MenuSchedule.PlateMenu
                            {
                                Id = Guid.NewGuid(),
                                TenantId = AbpSession.TenantId,
                                PlateId = plate.Id,
                                Price = 0,
                                SessionId = session.Id,
                                SelectedDate = input.DateFilter.Value
                            });

                            if (priceStrategyContractor != null && priceStrategyContractor.Id > 0)
                            {
                                await _priceStrategyRepository.InsertAsync(new PriceStrategy
                                {
                                    Id = Guid.NewGuid(),
                                    TenantId = AbpSession.TenantId == null ? 1 : AbpSession.TenantId.Value,
                                    Value = 0,
                                    PriceCodeId = priceStrategyContractor.Id,
                                    PlateMenuId = pmId
                                });
                            }
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
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

                var totalCount = plateMenus.Count();

                var dto = plateMenus.Select(x => new PlateMenuDto()
                {
                    Id = x.Id.ToString(),
                    Plate = ObjectMapper.Map<Plate.Plate>(x.Plate),
                    Price = x.Price,
                    CategoryName = x.Plate.PlateCategory == null ? null : x.Plate.PlateCategory.Name,
                    PriceStrategyId = (x.PriceStrategies == null || !x.PriceStrategies.Any()) ? "" : x.PriceStrategies.ToList()[0].Id.ToString(),
                    PriceStrategy = (x.PriceStrategies == null || !x.PriceStrategies.Any()) ? 0 : x.PriceStrategies.ToList()[0].Value,
                    Session = x.Session
                });

                var results = await dto
                    .OrderBy(input.Sorting ?? "plate.name asc")
                    .PageBy(input)
                    .ToListAsync();

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
    }
}