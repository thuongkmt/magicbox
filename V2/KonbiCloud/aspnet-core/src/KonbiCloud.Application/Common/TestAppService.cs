using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Domain.Repositories;
using KonbiCloud.Configuration;
using KonbiCloud.Machines;
using KonbiCloud.Messaging;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Konbini.Messages.Services;

namespace KonbiCloud.Common
{
    public class TestAppService : KonbiCloudAppServiceBase
    {
        private readonly ISendMessageToMachineClientService _sendMessageToMachineService;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IRepository<Device, Guid> _deviceRepository;
        //private readonly string _machineId;

        public TestAppService(ISendMessageToMachineClientService sendMessageToMachineService, 
            ISettingManager settingManager,
            IRepository<Machine, Guid> machineRepository,
            IRepository<Device, Guid> deviceRepository
            )
        {
            _sendMessageToMachineService = sendMessageToMachineService;
            _machineRepository = machineRepository;
            _deviceRepository = deviceRepository;
        }

        public object SendTestRabbitMq()
        {
            var obj = new KeyValueMessage()
            {
                Key = MessageKeys.TestKey,
                Value = "Hello"
            };
            _sendMessageToMachineService.SendQueuedMsgToMachines(obj,CloudToMachineType.AllMachines);
            return obj;

        }

        public object SendTestRabbitMqToMachine(Guid machineId)
        {
            var obj = new KeyValueMessage()
            {
                MachineId=machineId,
                Key = MessageKeys.TestKey,
                Value = "Hello"
            };
            _sendMessageToMachineService.SendQueuedMsgToMachines(obj, CloudToMachineType.ToMachineId);
            return obj;

        }

        public async Task GenerateSampleMachineDevices(Guid machineId)
        {
            var vmc = new Device
            {
                   Code="VMC",
                   Name="Vmc",
                   Port="COM1"                    ,
                   Status="Normal",
                   State = "Dispensing",
                   CustomInfoes=new List<DeviceCustomInfo>
                   {
                       new DeviceCustomInfo{Property="Level", Value="W"},
                       
                   }
            };
            var payment = new Device
            {
                Code = "IUC",
                Name = "Iuc payment",
                Port = "",
                Status = "Abnormal",
                State = "Wating for payment",
                CustomInfoes = new List<DeviceCustomInfo>
                   {
                       new DeviceCustomInfo{Property="TerminalId", Value="11111"},
                       new DeviceCustomInfo{Property="Owner", Value="Konbini"},
                   }
            };

            var machine = _machineRepository.Get(machineId);
            machine.Devices.Add(vmc);
            machine.Devices.Add(payment);

            await _machineRepository.UpdateAsync(machine);
        }
    }
}
