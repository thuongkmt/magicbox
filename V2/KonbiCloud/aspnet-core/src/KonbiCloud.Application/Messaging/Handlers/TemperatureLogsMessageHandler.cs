using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using KonbiCloud.Machines;
using KonbiCloud.TemperatureLogs;
using Konbini.Messages;
using System;
using System.Threading.Tasks;

namespace KonbiCloud.Messaging.Handlers
{
    public class TemperatureLogsMessageHandler : ITemperatureLogsMessageHandler, ITransientDependency
    {
        private readonly IRepository<TemperatureLog> _temperatureLogsRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly ILogger _logger;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public TemperatureLogsMessageHandler(
            IRepository<TemperatureLog> temperatureLogsRepository,
            IRepository<Machine, Guid> machineRepository,
            ILogger logger,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _temperatureLogsRepository = temperatureLogsRepository;
            _machineRepository = machineRepository;
            _logger = logger;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant,AbpDataFilters.MustHaveTenant))
                    {
                        decimal temperature = 0;
                        decimal.TryParse(keyValueMessage.JsonValue, out temperature);

                        //Check machine
                        var mc = await _machineRepository.FirstOrDefaultAsync(x => x.Id == keyValueMessage.MachineId);
                        if (mc == null)
                        {
                            _logger.Error($"Could not found machines {keyValueMessage.MachineId}");
                            return false;
                        }

                        // Save TemperatureLogs.
                        var tmp = new TemperatureLog
                        {
                            MachineId = keyValueMessage.MachineId,
                            Temperature = temperature,
                            TenantId = mc.TenantId
                        };
                        await _temperatureLogsRepository.InsertAsync(tmp);
                        await unitOfWork.CompleteAsync();
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("Process TemperatureLogs Handler", e);
                return false;
            }
        }
    }
}
