
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
using Microsoft.EntityFrameworkCore;
using KonbiCloud.CloudSync;
using Abp.Domain.Uow;
using KonbiCloud.Common;

namespace KonbiCloud.Machines
{
    [AbpAuthorize(AppPermissions.Pages_Sessions)]
    public class SessionsAppService : KonbiCloudAppServiceBase, ISessionsAppService
    {
        private readonly IRepository<Session, Guid> _sessionRepository;
        private readonly ISessionsExcelExporter _sessionsExcelExporter;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IDetailLogService _detailLogService;

        public SessionsAppService(IRepository<Session, Guid> sessionRepository,
            ISessionsExcelExporter sessionsExcelExporter, IRepository<Machine, Guid> machineRepository, IDetailLogService detailLog)
        {
            _sessionRepository = sessionRepository;
            _sessionsExcelExporter = sessionsExcelExporter;
            _machineRepository = machineRepository;
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
                await CurrentUnitOfWork.SaveChangesAsync();
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
                await CurrentUnitOfWork.SaveChangesAsync();
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

        [AbpAllowAnonymous]
        public async Task<IList<Session>> GetSessions(EntityDto<Guid> machineInput)
        {
            var sessions = new List<Session>();
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
            {
                var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == machineInput.Id);
                if(machine == null)
                {
                    Logger.Error($"Sync Session: MachineId: {machineInput.Id} does not exist");
                    return null;
                }
                else if (machine.IsDeleted)
                {
                    Logger.Error($"Sync Session: Machine with id: {machineInput.Id} is deleted");
                    return null;
                }
                sessions = await _sessionRepository.GetAllListAsync(x => x.TenantId == machine.TenantId);
            }
            return sessions;
        }
    }
}