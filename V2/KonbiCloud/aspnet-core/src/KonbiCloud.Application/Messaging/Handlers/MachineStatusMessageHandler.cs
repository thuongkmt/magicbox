using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using Abp.Timing;
using Castle.Core.Logging;
using KonbiCloud.Enums;
using KonbiCloud.Machines;
using KonbiCloud.Machines.Dtos;
using KonbiCloud.SignalR;
using Konbini.Messages;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace KonbiCloud.Messaging.Handlers
{
    public class MachineStatusMessageHandler : IMachineStatusMessageHandler, ITransientDependency
    {
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly ICacheManager _cacheManager;
        private readonly ILogger _logger;
        private readonly IMagicBoxMessageCommunicator _magicBoxMessage;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public MachineStatusMessageHandler(IRepository<Machine, Guid> machineRepository,
                                            ICacheManager cacheManager,
                                            ILogger logger,
                                            IMagicBoxMessageCommunicator magicBoxMessage,
                                            IUnitOfWorkManager unitOfWorkManager)
        {
            _machineRepository = machineRepository;
            _cacheManager = cacheManager;
            _logger = logger;
            _magicBoxMessage = magicBoxMessage;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                _logger.Info($"Machine Status Message Data\r\n{keyValueMessage.JsonValue}");

                var msDto = JsonConvert.DeserializeObject<MachineStatusDto>(keyValueMessage.JsonValue);
                if (msDto == null)
                {
                    _logger.Info($"Machine Status Message Data is null");
                    return false;
                }

                msDto.LastUpdate = Clock.Now;

                //Check machine
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
                    {
                        var mc = await _machineRepository.FirstOrDefaultAsync(x => x.Id == keyValueMessage.MachineId);
                        if (mc == null)
                        {
                            _logger.Info($"Can not find machine with id = " + keyValueMessage.MachineId);
                            return false;
                        }

                        //Set cache
                        await _cacheManager.GetCache(Common.Const.MachineStatus)
                                           .SetAsync(keyValueMessage.MachineId.ToString(), msDto);

                        //Notify Client
                        await _magicBoxMessage.SendMessageToAllClient(
                                                   new MagicBoxMessage
                                                   {
                                                       MessageType = MagicBoxMessageType.MachineStatus,
                                                       MachineId = keyValueMessage.MachineId
                                                   });
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.Error("Process Machine Status Message Handler", e);
                return false;
            }
        }
    }
}
