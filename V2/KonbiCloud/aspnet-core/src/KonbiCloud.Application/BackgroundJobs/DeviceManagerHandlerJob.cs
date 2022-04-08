using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using KonbiCloud.Machines;
using KonbiCloud.SignalR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.BackgroundJobs
{
    public class DeviceManagerHandlerJob : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IMagicBoxMessageCommunicator _magicBoxMessageCommunicator;
        public DeviceManagerHandlerJob(AbpTimer timer,
            IRepository<Machine,Guid> machineRepository,
            IMagicBoxMessageCommunicator magicBoxMessageCommunicator
            ) : base(timer)
        {
            Timer.Period = 60000; //check connection every 1 minutes!
            Timer.RunOnStart = true;

            _machineRepository = machineRepository;
            _magicBoxMessageCommunicator = magicBoxMessageCommunicator;
        }

        protected override void DoWork()
        {
           
        }

        private async Task SaveMachine(Machine machine)
        {
            //save to database
            await _machineRepository.InsertAsync(machine);
            //inform web to get new data
            var msg = new GeneralMessage()
            {

            };
            await _magicBoxMessageCommunicator.SendMessageToAllClient(msg);
        }

    }
}
