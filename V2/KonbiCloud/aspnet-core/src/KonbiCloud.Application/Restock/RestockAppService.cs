using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Timing;
using KonbiCloud.Authorization;
using KonbiCloud.Common;
using KonbiCloud.Inventories;
using KonbiCloud.Restock;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace KonbiCloud.MachineLoadout
{
    [AbpAuthorize(AppPermissions.Pages_Restock)]

    public class RestockAppService : KonbiCloudAppServiceBase, IMachineLoadoutAppService
    {
        private readonly IRepository<Topup, Guid> _restockSessionRepository;
        private readonly IDetailLogService _detailLogService;

        public RestockAppService(IRepository<Topup, Guid> restockSessionRepository,
                                 IDetailLogService detailLog)
        {
            _restockSessionRepository = restockSessionRepository;
            _detailLogService = detailLog;
        }

        public async Task<RestockSessionModelDto> GetRestockSession(EntityDto<Guid> machine)
        {
            var restockSessionDto = new RestockSessionModelDto();
            try
            {
                var restockSession = await _restockSessionRepository.FirstOrDefaultAsync(x => x.MachineId == machine.Id && !x.EndDate.HasValue);
                if (restockSession == null) return restockSessionDto;
                restockSession.MapTo(restockSessionDto);
                return restockSessionDto;
            }
            catch (Exception ex)
            {
                Logger.Error($"Get all loadout error:{ex.Message}", ex);
                return restockSessionDto;
            }

        }

        public async Task<bool> StartRestock(EntityDto<Guid> machine)
        {
            try
            {
                _detailLogService.Log($"StartRestock input machineId: {machine.Id}");
                var currentSession = await _restockSessionRepository.GetAll()
                                        .Where(x => x.MachineId == machine.Id)
                                        .OrderByDescending(x => x.CreationTime)
                                        .FirstOrDefaultAsync();

                var newSession = new Topup { MachineId = machine.Id, IsInprogress = true, StartDate = Clock.Now};
                if (currentSession == null)
                {
                    await _restockSessionRepository.InsertAsync(newSession);
                    return true;
                }

                _detailLogService.Log($"StartRestock data: {JsonConvert.SerializeObject(currentSession)}");
                currentSession.EndDate = Clock.Now;
                await _restockSessionRepository.UpdateAsync(currentSession);

                newSession.Total = currentSession.Total;
                newSession.IsInprogress = true;
                newSession.StartDate = Clock.Now;
                await _restockSessionRepository.InsertAsync(newSession);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"StartRestock error:{ex.Message}", ex);
                return false;
            }
            
        }

        public async Task<bool> EndRestock(EntityDto<Guid> machine)
        {
            try
            {
                _detailLogService.Log($"EndRestock data: {machine.Id}");
                var restockSession = await _restockSessionRepository.FirstOrDefaultAsync(x => x.MachineId == machine.Id && !x.EndDate.HasValue);
                if (restockSession == null) return true;
                _detailLogService.Log($"EndRestock - Current Restock Session data: {JsonConvert.SerializeObject(restockSession)}");
                restockSession.IsInprogress = false;
                await _restockSessionRepository.UpdateAsync(restockSession);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"EndRestock error:{ex.Message}", ex);
                return false;
            }

        }
    }
}
