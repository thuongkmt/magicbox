using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using Abp.UI;
using Abp.Timing;
using Castle.Core.Logging;
using KonbiCloud.Authorization;
using KonbiCloud.Common;
using KonbiCloud.Common.Dtos;
using KonbiCloud.Machines.Dtos;
using KonbiCloud.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using KonbiCloud.Configuration;
using Abp.Configuration;

namespace KonbiCloud.Machines
{
    [AbpAuthorize(AppPermissions.Pages_Machines)]

    public class MachineAppService : KonbiCloudAppServiceBase, IMachineAppService
    {
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IRepository<MachineErrorSolution> _machineErrorSolutionRepository;
        private readonly IRepository<Device,Guid> _deviceRepository;
        private readonly ICacheManager _cacheManager;
        private readonly ISlackService _slackService;
        private readonly ILogger _logger;

        public MachineAppService(IRepository<Machine, Guid> machineRepository,
            IRepository<Tenant> tenantRepository,
            IRepository<MachineErrorSolution> machineErrorSolutionRepository,
            IRepository<Device,Guid> deviceRepository,
            ILogger logger,
            ICacheManager cacheManager,
            ISlackService slackService)
        {
            _machineRepository = machineRepository;
            _tenantRepository = tenantRepository;
            _logger = logger;
            _cacheManager = cacheManager;
            _machineErrorSolutionRepository = machineErrorSolutionRepository;
            this._deviceRepository = deviceRepository;
            _slackService = slackService;
        }
        public async Task<PageResultListDto<MachineListDto>> GetAll(MachineInputListDto input)
        {
            var allMachines = _machineRepository.GetAll();
            int totalCount = await allMachines.CountAsync();

            if (string.IsNullOrEmpty(input.Sorting) || input.Sorting == "undefined")
            {
                input.Sorting = "name asc";
            }
            var machines = await allMachines
                .OrderBy(input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var results = new PageResultListDto<MachineListDto>(machines.MapTo<List<MachineListDto>>(), totalCount);

            var tenants = await _tenantRepository.GetAllListAsync();
            foreach (var item in results.Items)
            {
                item.TenantName = tenants.FirstOrDefault(x => x.Id == item.TenantId)?.Name ?? "";
            }
            return results;
        }

        public async Task<List<Device>> GetMachineDevices(Guid machineId)
        {
            var data = await _deviceRepository.GetAll().Where(x => x.Machine.Id == machineId).ToListAsync();
            return data;
        }

        public async Task<PageResultListDto<MachineErrorSolution>> GetMachineErrorSolutionAll(MachineInputListDto input)
        {
            var allMachineErrorSolutions = await _machineErrorSolutionRepository.GetAllListAsync();
            int totalCount = allMachineErrorSolutions.Count();

            var machineErrorSolutions = allMachineErrorSolutions
                .OrderBy(e => e.Id)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            var results = new PageResultListDto<MachineErrorSolution>(machineErrorSolutions, totalCount);
            return results;
        }

        public async Task<ListResultDto<MachineStatusDto>> QueryMachineStatus(MachineStatusDto input)
        {
            var msDtos = new List<MachineStatusDto>();
            try
            {
                var machines = await _machineRepository.GetAllListAsync(x => input.MachineId == null ||
                                                                        input.MachineId == Guid.Empty ||
                                                                        x.Id == input.MachineId);

                var cacheItem = _cacheManager.GetCache(Const.MachineStatus);
                bool machineHealth = true;
                foreach (var mc in machines.OrderBy(x => x.Name))
                {
                    var machineStatus = new MachineStatusDto
                    {
                        MachineId = mc.Id,
                        MachineName = mc.Name
                    };

                    var mcInCache = (MachineStatusDto)cacheItem.Get(mc.Id.ToString(), () => null);
                    if(mcInCache == null)
                    {
                        _logger.Info($"Can not find machine status cache");
                    }
                   
                    if (mcInCache != null)
                    {
                        machineStatus.DoorState = mcInCache.DoorState;
                        machineStatus.Temperature = mcInCache.Temperature;
                        machineStatus.MachineIp = mcInCache.MachineIp;
                        machineStatus.Cpu = mcInCache.Cpu;
                        machineStatus.Memory = mcInCache.Memory;
                        machineStatus.Hdd = mcInCache.Hdd;
                        machineStatus.PaymentState = mcInCache.PaymentState;
                        machineStatus.IsOnline = true;

                        if(mcInCache.LastUpdate == null)
                        {
                            machineStatus.IsOnline = false;
                            machineHealth = false;
                        }
                        else
                        {
                            if (Clock.Now.Subtract(mcInCache.LastUpdate).TotalMinutes > 5)
                            {
                                machineStatus.IsOnline = false;
                                machineHealth = false;
                            }
                        }
                    }
                    //NOTIFY TO SALCK
                    //if (!machineHealth)
                    //{
                    //    var channelName = await SettingManager.GetSettingValueAsync(AppSettings.Slack.ChannelName);
                    //    //if tenant register to us, then we create a channel for them if not will ignore this notification
                    //    if (channelName != null)
                    //    {
                    //        await _slackService.SendAlert(mc.Name, "Offline", channelName);
                    //    }
                    //}

                    msDtos.Add(machineStatus);
                }

                return new ListResultDto<MachineStatusDto>
                {
                    Items = msDtos
                };
            }
            catch (Exception e)
            {
                _logger.Error("Error Get All Machine Status", e);
                return new ListResultDto<MachineStatusDto>
                {
                    Items = msDtos
                };
            }
        }

        public async Task<ListResultDto<MachineComboboxDto>> GetMachinesForComboBox()
        {

            var result = _cacheManager
                .GetCache("GetMachinesForComboBox")
                .Get("-1", () =>
                 {
                     return _machineRepository
                         .GetAll()
                         .Select(x => new
                         {
                             x.Id,
                             x.Name
                         })
                         .Distinct()
                         .ToList();
                 });


            return new ListResultDto<MachineComboboxDto>
            {
                Items = result.Select(x => new MachineComboboxDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name
                }).OrderBy(x => x.Name)
                    .ToList()
            };
        }

        public async Task<Machine> GetDetail(EntityDto<Guid> input)
        {
            var machines = _machineRepository.GetAll()
                .Where(e => e.Id == input.Id);

            var machine = await machines.FirstOrDefaultAsync();

            if (machine == null)
            {
                throw new UserFriendlyException("Could not found the machine, maybe it's deleted..");
            }

            return machine;
        }

        public async Task Create(CreateMachineInput input)
        {
            Guid.TryParse(input.Id, out var @id);
            if (@id == null)
                throw new UserFriendlyException("ID is incorrect!");
            var oldMachine = await _machineRepository.FirstOrDefaultAsync(e => e.Id==@id);

            if (oldMachine != null)
                throw new UserFriendlyException("The machine with ID =" + input.Id + " is taken.");

            var tenantId = AbpSession.TenantId;
            try
            {
                var machine = new Machine
                {
                    Id = @id,
                    TenantId = tenantId,
                    Name = input.Name,
                    //CashlessTerminalId = input.CashlessTerminalId
                };
                //migrate from old cloud: await _serviceBus.CreateSubscription(input.Id);
                //await _plannedInventoryAppService.InitPlannedInventory(machine);
                await _machineRepository.InsertAsync(machine);
                await CurrentUnitOfWork.SaveChangesAsync();

            }catch(Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
        }

        public async Task<Machine> Update(Machine input)
        {
            //CheckUpdatePermission();

            var machine = await _machineRepository.FirstOrDefaultAsync(e => e.Id == input.Id);

            ObjectMapper.Map(input, machine);
            machine.Name = input.Name;
            //machine.CashlessTerminalId = input.CashlessTerminalId;

            //migrate from old cloud: await _serviceBus.CreateSubscription(input.Id.ToString());

            await _machineRepository.UpdateAsync(machine);
            return machine;
        }

        public async Task<SendRemoteCommandOutput> SendCommandToMachine(SendRemoteCommandInput input)
        {
            //CheckUpdatePermission();
            // implenent send command to machine ID with Commandam and Command Argument
            var result = await Task.Run(() =>
            {
                try
                {

                    var cmdMessage = $"redis_command_{(int)input.Command}::{input.CommandArgs}";
                    //_redisService.PublishCommandToMachine(input.MachineID, cmdMessage);
                    return new SendRemoteCommandOutput
                    {
                        IsSuccess = true,
                        Message = "Success send to machine"
                    };
                }
                catch (Exception ex)
                {
                    _logger.Error("Error when send command to machine",ex);
                    return new SendRemoteCommandOutput
                    {
                        IsSuccess = false,
                        Message = "Failed send to machine"
                    };
                }

            });

            return result;
        }

        public async Task Delete(EntityDto<Guid> input)
        {
            var machine = await _machineRepository.FirstOrDefaultAsync(e => e.Id == input.Id);
            await _machineRepository.DeleteAsync(machine);
        }
    }
}
