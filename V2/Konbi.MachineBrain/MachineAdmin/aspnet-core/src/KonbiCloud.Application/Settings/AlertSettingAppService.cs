using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using KonbiCloud.Authorization;
using KonbiCloud.Settings.Dtos;

namespace KonbiCloud.Settings
{
    [AbpAuthorize(AppPermissions.Pages_AlertSetting)]
    public class AlertSettingAppService : KonbiCloudAppServiceBase, IAlertSettingAppService
    {
        private readonly IRepository<AlertConfiguration, Guid> _alertConfigurationsRepository;

        public AlertSettingAppService(
            IRepository<AlertConfiguration, Guid> alertConfigurations
        )
        {
            _alertConfigurationsRepository = alertConfigurations;
        }

        // for alert setting reserve
        public AlertSettingDto GetAlertConfiguration(string machineID = "")
        {
            //int? tenantID = 0;
            //if(!string.IsNullOrEmpty(machineID))
            //{
            //    Guid gMachineID = new Guid(machineID);
            //    Machine curentMachine = _machineRepository.FirstOrDefault(m => m.Id == gMachineID);
            //    if(curentMachine != null)
            //    {
            //        tenantID = curentMachine.TenantId;
            //    }
            //}

            //AlertConfiguration config = _alertConfigurationsRepository.FirstOrDefault(a=> a.TenantId == tenantID);

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
