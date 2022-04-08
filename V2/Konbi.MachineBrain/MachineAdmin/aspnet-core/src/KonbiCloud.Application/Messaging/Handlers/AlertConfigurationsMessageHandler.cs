using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using KonbiCloud.Configuration;
using KonbiCloud.Settings;
using Konbini.Messages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace KonbiCloud.Messaging.Handlers
{
    public class AlertConfigurationsMessageHandler : IAlertConfigurationsMessageHandler
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<AlertConfiguration, Guid> _alertConfigurationRepository;
        private readonly ILogger _logger;
        private readonly bool _useCloud;

        public AlertConfigurationsMessageHandler(
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<AlertConfiguration, Guid> alertConfigurationRepository,
            ISettingManager settingManager
        )
        {
            _unitOfWorkManager = unitOfWorkManager;
            _alertConfigurationRepository = alertConfigurationRepository;
            bool.TryParse(settingManager.GetSettingValue(AppSettingNames.UseCloud), out _useCloud);
        }

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                if (!_useCloud) return false;

                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    var alertConfiguration = JsonConvert.DeserializeObject<AlertConfiguration>(keyValueMessage.JsonValue);

                    var id = alertConfiguration.Id;

                    if (await _alertConfigurationRepository.GetAll().AnyAsync())
                    {
                        var oldAlertConfiguration = await _alertConfigurationRepository.GetAll().FirstOrDefaultAsync();

                        oldAlertConfiguration.ToEmail = alertConfiguration.ToEmail;
                        oldAlertConfiguration.WhenChilledMachineTemperatureAbove = alertConfiguration.WhenChilledMachineTemperatureAbove;
                        oldAlertConfiguration.WhenFrozenMachineTemperatureAbove = alertConfiguration.WhenFrozenMachineTemperatureAbove;
                        oldAlertConfiguration.WhenHotMachineTemperatureBelow = alertConfiguration.WhenHotMachineTemperatureBelow;
                        oldAlertConfiguration.SendEmailWhenProductExpiredDate = alertConfiguration.SendEmailWhenProductExpiredDate;
                        oldAlertConfiguration.WhenStockBellow = alertConfiguration.WhenStockBellow;

                        await _alertConfigurationRepository.UpdateAsync(oldAlertConfiguration);
                    }
                    else
                    {
                        await _alertConfigurationRepository.InsertAsync(alertConfiguration);
                    }

                    await unitOfWork.CompleteAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error when synchronize alert configuration from cloud to machine ", ex);
                return false;
            }
        }
    }
}
