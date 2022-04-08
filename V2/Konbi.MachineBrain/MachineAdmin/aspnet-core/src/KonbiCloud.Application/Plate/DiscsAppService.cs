using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using KonbiCloud.Plate.Exporting;
using KonbiCloud.Plate.Dtos;
using KonbiCloud.Dto;
using Abp.Application.Services.Dto;
using KonbiCloud.Authorization;
using Abp.Authorization;
using KonbiCloud.SignalR;
using Microsoft.EntityFrameworkCore;
using KonbiCloud.CloudSync;
using Abp.Domain.Uow;
using Abp.UI;
using Abp.Configuration;
using KonbiCloud.Configuration;
using KonbiCloud.RFIDTable.Cache;
using Abp.Runtime.Caching;
using KonbiCloud.Common;

namespace KonbiCloud.Plate
{
    [AbpAuthorize(AppPermissions.Pages_Discs)]
    public class DiscsAppService : KonbiCloudAppServiceBase, IDiscsAppService
    {
        private readonly IRepository<Disc, Guid> _discRepository;
        private readonly IDiscsExcelExporter _discsExcelExporter;
        private readonly IRepository<Plate, Guid> _plateRepository;
        private readonly IMessageCommunicator messageCommunicator;
        private readonly IDishSyncService _dishSyncService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ICacheManager _cacheManager;
        private readonly IDetailLogService _detailLogService;

        public DiscsAppService(IRepository<Disc, Guid> discRepository, IDiscsExcelExporter discsExcelExporter,
                               IRepository<Plate, Guid> plateRepository, IMessageCommunicator messageCommunicator,
                               ICacheManager cacheManager,
                               IDishSyncService dishSyncService, IUnitOfWorkManager unitOfWorkManager, IDetailLogService detailLog)
        {
            _cacheManager = cacheManager;
            _discRepository = discRepository;
            _discsExcelExporter = discsExcelExporter;
            _plateRepository = plateRepository;
            this.messageCommunicator = messageCommunicator;
            this._dishSyncService = dishSyncService;
            _unitOfWorkManager = unitOfWorkManager;
            _detailLogService = detailLog;
        }

        public async Task<PagedResultDto<GetDiscForView>> GetAll(GetAllDiscsInput input)
        {
            try { 
                var filteredDiscs = _discRepository.GetAll()
                             .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Uid.Contains(input.Filter) || e.Code.Contains(input.Filter))
                             .WhereIf(!string.IsNullOrWhiteSpace(input.UidFilter), e => e.Uid.ToLower() == input.UidFilter.ToLower().Trim())
                             .WhereIf(!string.IsNullOrWhiteSpace(input.CodeFilter), e => e.Code.ToLower() == input.CodeFilter.ToLower().Trim())
                             .WhereIf(!string.IsNullOrWhiteSpace(input.PlateIdFilter), e => e.PlateId == new Guid(input.PlateIdFilter));


                var query = (from o in filteredDiscs
                             join o1 in _plateRepository.GetAll() on o.PlateId equals o1.Id into j1
                             from s1 in j1.DefaultIfEmpty()

                             select new GetDiscForView()
                             {
                                 Disc = ObjectMapper.Map<DiscDto>(o),
                                 PlateName = s1 == null ? "" : s1.Name
                             });
                            //.WhereIf(!string.IsNullOrWhiteSpace(input.PlateNameFilter), e => e.PlateName.ToLower() == input.PlateNameFilter.ToLower().Trim());

                var totalCount = await query.CountAsync();

                var discs = await query
                    .OrderBy(input.Sorting ?? "disc.id asc")
                    .PageBy(input)
                    .ToListAsync();

                return new PagedResultDto<GetDiscForView>(
                    totalCount,
                    discs
                );
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new PagedResultDto<GetDiscForView>(0, new List<GetDiscForView>());
            }
     
        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Edit)]
        public async Task<GetDiscForEditOutput> GetDiscForEdit(DiscDto input)
        {
            try
            {
                var disc = await _discRepository.FirstOrDefaultAsync(input.Id);

                var output = new GetDiscForEditOutput { Disc = ObjectMapper.Map<CreateOrEditDiscDto>(disc) };

                if (output.Disc.PlateId != null)
                {
                    var plate = await _plateRepository.FirstOrDefaultAsync((Guid)output.Disc.PlateId);
                    output.PlateName = plate.Name;
                }

                return output;
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new GetDiscForEditOutput();
            }
        }

        public async Task CreateOrEdit(List<CreateOrEditDiscDto> input)
        {
            //if(input.Id == null){
            await Create(input);
            //}
            //else{
            //	await Update(input);
            //}
            await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Create)]
        private async Task Create(List<CreateOrEditDiscDto> input)
        {
            try
            {
                //TODO: need to find bulk insert DB
                var listDishes = new List<Disc>();
                foreach (var entity in input)
                {
                    var disc = ObjectMapper.Map<Disc>(entity);
                    listDishes.Add(disc);
                    if (AbpSession.TenantId != null)
                    {
                        disc.TenantId = AbpSession.TenantId;
                    }

                    await _discRepository.InsertAsync(disc);
                }

                await CurrentUnitOfWork.SaveChangesAsync();

                var allowSyncToServer = await SettingManager.GetSettingValueAsync<bool>(AppSettingNames.AllowPushDishToServer);
                if (allowSyncToServer)
                {
                    try
                    {
                        var mId = Guid.Empty;
                        Guid.TryParse(await SettingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);
                        if (mId == Guid.Empty)
                        {
                            Logger.Info($"Push dish: Machine Id is null");
                        }
                        else
                        {
                            var syncItem = new SyncedItemData<Disc>
                            {
                                MachineId = mId,
                                SyncedItems = listDishes
                            };

                            var success = await _dishSyncService.PushToServer(syncItem);
                            if (success)
                            {
                                var dishes = await _discRepository.GetAllListAsync(x => !x.IsSynced);
                                foreach (var d in listDishes)
                                {
                                    var dish = dishes.FirstOrDefault(x => x.Id == d.Id);
                                    if (dish != null)
                                    {
                                        dish.IsSynced = true;
                                        dish.SyncDate = DateTime.Now;
                                    }
                                }
                                await CurrentUnitOfWork.SaveChangesAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message, ex);
                    }
                }
                await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Edit)]
        private async Task Update(CreateOrEditDiscDto input)
        {
            try
            {
                var disc = await _discRepository.FirstOrDefaultAsync(input.Id.Value);
                if (disc != null)
                {
                    ObjectMapper.Map(input, disc);
                }
                await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Delete)]
        public async Task Delete(DiscDto input)
        {
            try
            {
                var disc = await _discRepository.FirstOrDefaultAsync(input.Id);
                if (disc != null)
                {
                    await _discRepository.DeleteAsync(disc);
                    await CurrentUnitOfWork.SaveChangesAsync();
                    await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    var allowSyncToServer = await SettingManager.GetSettingValueAsync<bool>(AppSettingNames.AllowPushDishToServer);
                    if (allowSyncToServer)
                    {
                        var mId = Guid.Empty;
                        Guid.TryParse(await SettingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);
                        if (mId == Guid.Empty)
                        {
                            Logger.Info($"Push dish: Machine Id is null");
                        }
                        else
                        {
                            disc.IsDeleted = true;
                            var syncItem = new SyncedItemData<Disc>
                            {
                                MachineId = mId,
                                SyncedItems = new List<Disc>
                            {
                                disc
                            }
                            };
                            await _dishSyncService.PushToServer(syncItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        public async Task<FileDto> GetDiscsToExcel(GetAllDiscsForExcelInput input)
        {

            var filteredDiscs = _discRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Uid.Contains(input.Filter) || e.Code.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UidFilter), e => e.Uid.ToLower() == input.UidFilter.ToLower().Trim())
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CodeFilter), e => e.Code.ToLower() == input.CodeFilter.ToLower().Trim());


            var query = (from o in filteredDiscs
                         join o1 in _plateRepository.GetAll() on o.PlateId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         select new GetDiscForView()
                         {
                             Disc = ObjectMapper.Map<DiscDto>(o),
                             PlateName = s1 == null ? "" : s1.Name
                         })
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PlateNameFilter), e => e.PlateName.ToLower() == input.PlateNameFilter.ToLower().Trim());


            var discListDtos = await query.ToListAsync();

            return _discsExcelExporter.ExportToFile(discListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_Discs)]
        public async Task<PagedResultDto<PlateLookupTableDto>> GetAllPlateForLookupTable(GetAllForLookupTableInput input)
        {
            try
            {
                var query = _plateRepository.GetAll().WhereIf(
                       !string.IsNullOrWhiteSpace(input.Filter),
                      e => e.Name.Contains(input.Filter)
                   );

                var totalCount = await query.CountAsync();

                var plateList = await query.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Code
                })
                    .PageBy(input)
                    .ToListAsync();

                var lookupTableDtoList = new List<PlateLookupTableDto>();
                foreach (var plate in plateList)
                {
                    lookupTableDtoList.Add(new PlateLookupTableDto
                    {
                        Id = plate.Id.ToString(),
                        DisplayName = plate.Name,
                        Code = plate.Code
                    });
                }

                return new PagedResultDto<PlateLookupTableDto>(
                    totalCount,
                    lookupTableDtoList
                );
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new PagedResultDto<PlateLookupTableDto>(0, new List<PlateLookupTableDto>());
            }
        }


        public async Task TestSendSignalRMessage(List<CreateOrEditDiscDto> input)
        {
           // "{"type":"RFIDTable_DetectedDisc","data":{"Plates":[{"UID":"E0040150900F4B0A","UType":"0709"}]}}"

            var mess = "{\"type\":\"RFIDTable_DetectedDisc\",\"data\":{\"Plates\":[";
            int i = 0;
            foreach (var entity in input)
            {
                if (i == (input.Count - 1))
                {
                    mess += "{\"UType\":\"" + entity.Code + "\",\"UID\":\"" + entity.Uid + "\"}";
                }
                else
                {
                    mess += "{\"UType\":\"" + entity.Code + "\",\"UID\":\"" + entity.Uid + "\"},";
                }
                i++;
            }
            mess += "]}}";
            await messageCommunicator.SendRfidTableMessageToAllClient(new GeneralMessage { Message = mess });
            // await messageCommunicator.SendTestMessageToAllClient(new GeneralMessage { Message = "{\"type\":\"RFIDTable_DetectedDisc\",\"data\":[{\"code\":\"010203\",\"uid\":\"12345\"},{\"code\":\"010203\",\"uid\":\"98765\"}]}" });
        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Sync)]
        public async Task<bool> SyncPlateDataFromServer()
        {
            var mId = Guid.Empty;
            Guid.TryParse(await SettingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);
            if (mId == Guid.Empty)
            {
                throw new UserFriendlyException("Machine configuration error");
            }
            var syncedList = new SyncedItemData<Guid> { MachineId = mId, SyncedItems = new List<Guid>() };
            var existDishes = new List<Disc>();
            var plates = new List<Plate>();
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                existDishes = await _discRepository.GetAllListAsync();
                plates = await _plateRepository.GetAllListAsync();
            }

            var dishesFromServer = await _dishSyncService.SyncFromServer(mId);
            if (dishesFromServer == null)
            {
                throw new UserFriendlyException("Cannot get Dishes from server");
            }
            try
            {
                foreach (var d in dishesFromServer)
                {
                    //Check if plate does not exist -> continue
                    if (d.PlateId != Guid.Empty)
                    {
                        var plate = plates.FirstOrDefault(x => x.Id == d.PlateId);
                        if (plate == null)
                            continue;
                    }

                    var ed = existDishes.FirstOrDefault(x => x.Id == d.Id);
                    if (ed == null)
                    {
                        d.TenantId = AbpSession.TenantId;
                        await _discRepository.InsertAsync(d);
                    }
                    else
                    {
                        ed.IsDeleted = d.IsDeleted;
                        ed.TenantId = AbpSession.TenantId;
                        ed.Uid = d.Uid;
                        ed.Code = d.Code;
                        ed.PlateId = d.PlateId;
                        ed.IsSynced = true;
                    }

                    syncedList.SyncedItems.Add(d.Id);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
                //Update sync status to server
                _dishSyncService.UpdateSyncStatus(syncedList);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw new UserFriendlyException("Sync Dish failed");
            }
        }

        [AbpAllowAnonymous]
        public async Task UpdateSyncStatus(IEnumerable<DiscDto> dishes)
        {
            try
            {
                var existDishes = new List<Disc>();
                using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
                {
                    existDishes = await _discRepository.GetAllListAsync();
                }

                foreach (var d in dishes)
                {
                    var existDish = existDishes.FirstOrDefault(x => x.Id == d.Id);
                    if (existDish != null)
                    {
                        existDish.IsSynced = true;
                        existDish.SyncDate = DateTime.Now;
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
        }
    }
}