using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using KonbiCloud.Common.Dtos;
using KonbiCloud.Machines;
using KonbiCloud.MachineSessions.Dtos;
using KonbiCloud.Settings;
using Microsoft.EntityFrameworkCore;

namespace KonbiCloud.MachineSessions
{
    public class AlertSettingAppServices : KonbiCloudAppServiceBase
    {       
        private readonly IRepository<AlertConfiguration, Guid> alertConfigurations;
        private readonly IRepository<Machine, Guid> machineRepository;

        public AlertSettingAppServices(IRepository<Machine, Guid> machineRepository            
            , IRepository<AlertConfiguration, Guid> alertConfigurations)
        {           
            this.alertConfigurations = alertConfigurations;
            this.machineRepository = machineRepository;
        }       
        // for alert setting reserve
        public Settings.AlertConfiguration GetAlertConfiguration(string machineID="")
        {
            int? tenantID = 0;
            if(!string.IsNullOrEmpty(machineID))
            {
                Guid gMachineID = new Guid(machineID);
                Machine curentMachine = machineRepository.FirstOrDefault(m => m.Id == gMachineID);
                if(curentMachine != null)
                {
                    tenantID = curentMachine.TenantId;
                }
            }

            AlertConfiguration config = alertConfigurations.FirstOrDefault(a=>a.TenantId == tenantID);
            return config != null ? config : new Settings.AlertConfiguration
            {
                ToEmail = "",
                WhenChilledMachineTemperatureAbove = 10,
                WhenHotMachineTemperatureBelow = 40,
                SendEmailWhenProductExpiredDate = 1,
                WhenStockBellow = 3
            };
        }

        public AlertConfiguration UpdateAlertConfig(AlertConfiguration input)
        {
            Console.WriteLine("receive update");
            AlertConfiguration config = alertConfigurations.FirstOrDefault(x => true);
            config.ToEmail = input.ToEmail;
            config.WhenChilledMachineTemperatureAbove = input.WhenChilledMachineTemperatureAbove;
            config.WhenHotMachineTemperatureBelow = input.WhenHotMachineTemperatureBelow;
            config.SendEmailWhenProductExpiredDate = input.SendEmailWhenProductExpiredDate;
            config.WhenStockBellow = input.WhenStockBellow;
            CurrentUnitOfWork.SaveChanges();

            return config;
        }
        // end reserve
    }
}
