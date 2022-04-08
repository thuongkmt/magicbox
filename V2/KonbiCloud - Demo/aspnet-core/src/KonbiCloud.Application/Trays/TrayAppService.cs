
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using KonbiCloud.Authorization;
using KonbiCloud.Machines;
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
        private readonly IRepository<Machine, Guid> _machineRepository;

        public TrayAppService(IRepository<Tray, Guid> trayRepository,
                              IRepository<Machine, Guid> machineRepository)
        {
            _trayRepository = trayRepository;
            _machineRepository = machineRepository;
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

                return new PagedResultDto<TrayDto>(totalCount, results);
            }
            catch (UserFriendlyException ue)
            {
                throw ue;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
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

            await _trayRepository.InsertAsync(tray);
        }

        [AbpAuthorize(AppPermissions.Pages_Trays_Edit)]
        private async Task Update(TrayDto input)
        {
            var tray = await _trayRepository.FirstOrDefaultAsync((Guid)input.Id);
            tray.Name = input.Name;
            tray.Code = input.Code;
            //ObjectMapper.Map(input, tray);
        }

        [AbpAuthorize(AppPermissions.Pages_Trays_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            await _trayRepository.DeleteAsync(input.Id);
        }
        [AbpAllowAnonymous]
        public async Task<IList<Tray>> GetTrays(EntityDto<Guid> machineInput)
        {
            var trays = new List<Tray>();
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
            {
                var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == machineInput.Id);
                if (machine == null)
                {
                    Logger.Error($"Sync Tray: MachineId: {machineInput.Id} does not exist");
                    return null;
                }
                else if (machine.IsDeleted)
                {
                    Logger.Error($"Sync Tray: Machine with id: {machineInput.Id} is deleted");
                    return null;
                }
                trays = await _trayRepository.GetAllListAsync(x => x.TenantId == machine.TenantId);
            }
            return trays;
        }
    }
}