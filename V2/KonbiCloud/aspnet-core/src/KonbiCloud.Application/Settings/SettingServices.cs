using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using KonbiCloud.Authorization;
using KonbiCloud.Machines;
using KonbiCloud.Settings;
using KonbiCloud.Settings.Dtos;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Konbini.Messages.Services;

namespace KonbiCloud.MachineSessions
{
    [AbpAuthorize(AppPermissions.Pages_AlertSetting)]
    public class AlertSettingAppServices : KonbiCloudAppServiceBase, IAlertSettingAppServices
    {       
        private readonly IRepository<AlertConfiguration, Guid> _alertConfigurationsRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly ISendMessageToMachineClientService _sendMessageToMachineService;

        public AlertSettingAppServices(IRepository<Machine, Guid> machineRepository            
            , IRepository<AlertConfiguration, Guid> alertConfigurations
            , ISendMessageToMachineClientService sendMessageToMachineService
        )
        {           
            _alertConfigurationsRepository = alertConfigurations;
            _machineRepository = machineRepository;
            _sendMessageToMachineService = sendMessageToMachineService;
        }       

        // for alert setting reserve
        public AlertSettingDto GetAlertConfiguration(string machineID="")
        {
            var config = _alertConfigurationsRepository.FirstOrDefault(a => true);

            if (config != null)
            {
                return new AlertSettingDto
                {
                    Id = config.Id,
                    ToEmail = config.ToEmail,
                    WhenChilledMachineTemperatureAbove = config.WhenChilledMachineTemperatureAbove,
                    WhenFrozenMachineTemperatureAbove = config.WhenFrozenMachineTemperatureAbove,
                    WhenHotMachineTemperatureBelow = config.WhenHotMachineTemperatureBelow,
                    SendEmailWhenProductExpiredDate = config.SendEmailWhenProductExpiredDate,
                    WhenStockBellow = config.WhenStockBellow
                };
            }

            return null;
        }

        [AbpAuthorize(AppPermissions.Pages_AlertSetting_Edit)]
        public async Task<AlertConfiguration> CreateOrEdit(AlertSettingDto input)
        {
            AlertConfiguration result = null;

            if (!_alertConfigurationsRepository.GetAll().Any())
            {
                result = await Create(input);
            }
            else
            {
                result = await Update(input);
            }

            int? tenantId = null;
            if (AbpSession.TenantId != null)
            {
                tenantId = (int?)AbpSession.TenantId;
            }

            var machines = _machineRepository.GetAll().Where(x => x.TenantId == tenantId);

            if (machines.Any())
            {
                foreach (var machine in machines)
                {
                    _sendMessageToMachineService.SendQueuedMsgToMachines(new KeyValueMessage
                    {
                        Key = MessageKeys.AlertConfiguration,
                        MachineId = machine.Id,
                        Value = result,
                    }, CloudToMachineType.ToMachineId);
                }
            }
            return result;
        }

        public async Task<AlertConfiguration> Create(AlertSettingDto input)
        {
            var alertConfiguration = ObjectMapper.Map<AlertConfiguration>(input);

            if (AbpSession.TenantId != null)
            {
                alertConfiguration.TenantId = (int)AbpSession.TenantId;
            }

            await _alertConfigurationsRepository.InsertAsync(alertConfiguration);

            return alertConfiguration;
        }

        public async Task<AlertConfiguration> Update(AlertSettingDto input)
        {
            var alertConfiguration = await _alertConfigurationsRepository.FirstOrDefaultAsync(x => true);
            ObjectMapper.Map(input, alertConfiguration);
            await _alertConfigurationsRepository.UpdateAsync(alertConfiguration);
            return alertConfiguration;
        }
        // end reserve
    }
}
