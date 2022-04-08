
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using KonbiCloud.Machines.Exporting;
using KonbiCloud.Machines.Dtos;
using KonbiCloud.Dto;
using Abp.Application.Services.Dto;
using KonbiCloud.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using KonbiCloud.Sessions;
using Microsoft.EntityFrameworkCore;
using KonbiCloud.CloudSync;
using Abp.Domain.Uow;
using Abp.UI;
using KonbiCloud.RFIDTable.Cache;
using Abp.Runtime.Caching;
using KonbiCloud.Configuration;
using KonbiCloud.Common;

namespace KonbiCloud.Machines
{
    [AbpAuthorize(AppPermissions.Pages_Sessions)]
    public class SessionsAppService : KonbiCloudAppServiceBase, ISessionsAppService
    {
        private readonly IRepository<Session, Guid> _sessionRepository;
        private readonly ISessionsExcelExporter _sessionsExcelExporter;
        private readonly ISessionSyncService _sessionSyncService;
        private readonly ICacheManager _cacheManager;
        private readonly IDetailLogService _detailLogService;

        public SessionsAppService(IRepository<Session, Guid> sessionRepository, ISessionsExcelExporter sessionsExcelExporter,
                                    ISessionSyncService sessionSyncService, ICacheManager cacheManager, IDetailLogService detailLog)
        {
            _sessionRepository = sessionRepository;
            _sessionsExcelExporter = sessionsExcelExporter;
            _sessionSyncService = sessionSyncService;
            _cacheManager = cacheManager;
            _detailLogService = detailLog;
        }

        public async Task<PagedResultDto<GetSessionForView>> GetAll(GetAllSessionsInput input)
        {
            try
            {
                var filteredSessions = _sessionRepository.GetAll()
                            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Name.Contains(input.Filter) || e.FromHrs.Contains(input.Filter) || e.ToHrs.Contains(input.Filter))
                            .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name.ToLower() == input.NameFilter.ToLower().Trim())
                            .WhereIf(!string.IsNullOrWhiteSpace(input.FromHrsFilter), e => e.FromHrs.ToLower() == input.FromHrsFilter.ToLower().Trim())
                            .WhereIf(!string.IsNullOrWhiteSpace(input.ToHrsFilter), e => e.ToHrs.ToLower() == input.ToHrsFilter.ToLower().Trim());


                var query = (from o in filteredSessions
                             select new GetSessionForView()
                             {
                                 Session = ObjectMapper.Map<SessionDto>(o)
                             });

                var totalCount = await query.CountAsync();

                var sessions = await query
                    .OrderBy(input.Sorting ?? "session.FromHrs asc")
                    .PageBy(input)
                    .ToListAsync();

                return new PagedResultDto<GetSessionForView>(
                    totalCount,
                    sessions
                );
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new PagedResultDto<GetSessionForView>(0, new List<GetSessionForView>());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Sessions_Edit)]
        public async Task<GetSessionForEditOutput> GetSessionForEdit(EntityDto<Guid> input)
        {
            try
            {
                var session = await _sessionRepository.FirstOrDefaultAsync(input.Id);

                var output = new GetSessionForEditOutput { Session = ObjectMapper.Map<CreateOrEditSessionDto>(session) };

                return output;
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new GetSessionForEditOutput();
            }
        }

        public async Task CreateOrEdit(CreateOrEditSessionDto input)
        {
            if (input.Id == null)
            {
                await Create(input);
            }
            else
            {
                await Update(input);
            }
            await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Sessions_Create)]
        private async Task Create(CreateOrEditSessionDto input)
        {
            try
            {
                var session = ObjectMapper.Map<Session>(input);
                if (AbpSession.TenantId != null)
                {
                    session.TenantId = (int?)AbpSession.TenantId;
                }
                await _sessionRepository.InsertAsync(session);
                await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Sessions_Edit)]
        private async Task Update(CreateOrEditSessionDto input)
        {
            try
            {
                var session = await _sessionRepository.FirstOrDefaultAsync((Guid)input.Id);
                ObjectMapper.Map(input, session);
                await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Sessions_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            try
            {
                await _sessionRepository.DeleteAsync(input.Id);
                await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        public async Task<FileDto> GetSessionsToExcel(GetAllSessionsForExcelInput input)
        {

            var filteredSessions = _sessionRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Name.Contains(input.Filter) || e.FromHrs.Contains(input.Filter) || e.ToHrs.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name.ToLower() == input.NameFilter.ToLower().Trim())
                        .WhereIf(!string.IsNullOrWhiteSpace(input.FromHrsFilter), e => e.FromHrs.ToLower() == input.FromHrsFilter.ToLower().Trim())
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ToHrsFilter), e => e.ToHrs.ToLower() == input.ToHrsFilter.ToLower().Trim());


            var query = (from o in filteredSessions
                         select new GetSessionForView()
                         {
                             Session = ObjectMapper.Map<SessionDto>(o)
                         });


            var sessionListDtos = await query.ToListAsync();

            return _sessionsExcelExporter.ExportToFile(sessionListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_Sessions_Sync)]
        public async Task<bool> SyncSessionData()
        {
            var mId = Guid.Empty;
            Guid.TryParse(await SettingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);
            if (mId == Guid.Empty)
            {
                throw new UserFriendlyException("Machine configuration error");

            }
            var sessions = await _sessionSyncService.Sync(mId);
            if (sessions == null)
            {
                throw new UserFriendlyException("Cannot get Sessions from server");
            }

            var existSessions = new List<Session>();
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                existSessions = await _sessionRepository.GetAllListAsync();
            }

            try
            {
                foreach (var session in sessions)
                {
                    var oldSession = existSessions.FirstOrDefault(x => x.Id == session.Id);
                    if (oldSession == null)
                    {
                        session.TenantId = AbpSession.TenantId;
                        await _sessionRepository.InsertAsync(session);
                    }
                    else
                    {
                        oldSession.IsDeleted = session.IsDeleted;
                        oldSession.Name = session.Name;
                        oldSession.FromHrs = session.FromHrs;
                        oldSession.ToHrs = session.ToHrs;
                        await _sessionRepository.UpdateAsync(oldSession);
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw new UserFriendlyException("Sync Sessions failed");
            }
        }
    }
}