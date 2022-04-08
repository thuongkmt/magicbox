using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Runtime.Caching;
using Abp.UI;
using KonbiCloud.Authorization;
using KonbiCloud.CloudSync;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.RFIDTable.Cache;
using KonbiCloud.Tray.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace KonbiCloud.Plate
{
    [AbpAuthorize(AppPermissions.Pages_Trays)]
    public class TrayAppService : KonbiCloudAppServiceBase, ITrayAppService
    {
        private readonly IRepository<Tray, Guid> _trayRepository;
        private readonly ITraySyncService _traySyncService;
        private readonly ICacheManager _cacheManager;
        private readonly IDetailLogService detailLogService;

        public TrayAppService(IRepository<Tray, Guid> trayRepository,
                              ITraySyncService traySyncService,
                              ICacheManager cacheManager,
                              IDetailLogService detailLog)
        {
            _trayRepository = trayRepository;
            _traySyncService = traySyncService;
            this._cacheManager = cacheManager;
            this.detailLogService = detailLog;
        }

        public async Task<PagedResultDto<TrayDto>> GetAllTrays(TrayRequest request)
        {
            try
            {
                var trays = _trayRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(request.NameFilter), e => e.Name.ToLower().Contains(request.NameFilter.ToLower().Trim()))
                        .WhereIf(!string.IsNullOrWhiteSpace(request.CodeFilter), e => e.Code.ToLower().Contains(request.CodeFilter.ToLower().Trim()));

                var totalCount = await trays.CountAsync();

                var dto = trays.Select(x => x.MapTo<TrayDto>());

                var results = await dto
                    .OrderBy(request.Sorting ?? "Name asc")
                    .PageBy(request)
                    .ToListAsync();
                detailLogService.Log($"Get All Trays returns : {results.Count}");
                return new PagedResultDto<TrayDto>(totalCount, results);
            }
            catch (Exception ex)
            {
                Logger.Error($"Get all Trays: {ex.Message}", ex);
                return new PagedResultDto<TrayDto>(0, new List<TrayDto>());
            }
        }

        public async Task<TrayDto> CreateOrEditTray(TrayDto input)
        {
            if (input.Id == null)
            {
                if (await _trayRepository.FirstOrDefaultAsync(x => x.Code.ToLower().Equals(input.Code.Trim().ToLower())) != null)
                {
                    return new TrayDto { Message = $"Tray code {input.Code} already existed, please use another code." };
                }
                await Create(input);
            }
            else
            {
                if (await _trayRepository.FirstOrDefaultAsync(x => x.Id != input.Id.Value && x.Code.ToLower().Equals(input.Code.Trim().ToLower())) != null)
                {
                    return new TrayDto { Message = $"Tray code {input.Code} already existed, please use another code." };

                }
                await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                await Update(input);
            }
            return new TrayDto { Message = null };
        }

        [AbpAuthorize(AppPermissions.Pages_Trays_Create)]
        private async Task Create(TrayDto input)
        {
            var tray = new Tray
            {
                Name = input.Name,
                Code = input.Code
            };

            if (AbpSession.TenantId != null)
            {
                tray.TenantId = AbpSession.TenantId;
            }
            await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
            await _trayRepository.InsertAsync(tray);
        }

        [AbpAuthorize(AppPermissions.Pages_Trays_Edit)]
        private async Task Update(TrayDto input)
        {
            var tray = await _trayRepository.FirstOrDefaultAsync((Guid)input.Id);
            tray.Name = input.Name;
            tray.Code = input.Code;
            //ObjectMapper.Map(input, tray);
            await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Trays_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
            await _trayRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_Trays_Sync)]
        public async Task<bool> SyncTrayData()
        {
            var mId = Guid.Empty;
            Guid.TryParse(await SettingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);
            if (mId == Guid.Empty)
            {
                throw new UserFriendlyException("Machine configuration error");
            }

            var trays = await _traySyncService.Sync(mId);
            if (trays == null)
            {
                throw new UserFriendlyException("Cannot get Trays from server");
            }

            var existTrays = new List<Tray>();
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                existTrays = await _trayRepository.GetAllListAsync();
            }
            
            try
            {
                foreach (var tray in trays)
                {
                    var oldTray = existTrays.FirstOrDefault(x => x.Id == tray.Id || x.Code.Equals(tray.Code));
                    if (oldTray == null)
                    {
                        tray.TenantId = AbpSession.TenantId;
                        await _trayRepository.InsertAsync(tray);
                    }
                    else
                    {
                        oldTray.IsDeleted = tray.IsDeleted;
                        oldTray.Name = tray.Name;
                        oldTray.Code = tray.Code;
                        await _trayRepository.UpdateAsync(oldTray);
                    }
                }
                
                await CurrentUnitOfWork.SaveChangesAsync();
                await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Sync Trays: {ex.Message}", ex);
                throw new UserFriendlyException("Sync Trays failed");
            }
        }
    }
}