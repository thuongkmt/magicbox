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
    public class MachineSessionAppService : KonbiCloudAppServiceBase, IMachineSessionAppServices
    {
        private readonly IRepository<Session, Guid> sessionRepository;
        private readonly IRepository<AlertConfiguration, Guid> alertConfigurations;

        public MachineSessionAppService(IRepository<Session, Guid> sessionRepository, IRepository<AlertConfiguration,Guid> alertConfigurations)
        {
            this.sessionRepository = sessionRepository;
            this.alertConfigurations = alertConfigurations;
        }
        public async Task Create(CreateMachineSessionInput input)
        {
            try
            {
                var oldMachineSession = await sessionRepository.FirstOrDefaultAsync(e => e.Name.Trim().ToLower() == input.Name.Trim().ToLower());

                if (oldMachineSession != null)
                    throw new UserFriendlyException("The machine session with ID =" + input.Name + " is taken.");

                int numberExistingSession = await sessionRepository.CountAsync();
                if(numberExistingSession >=3 )
                {
                    throw new UserFriendlyException("System can not create more than three session.");
                }

                var session = new Session()
                {                    
                    TenantId = AbpSession.GetTenantId(),
                    Name = input.Name
                };
               
                await sessionRepository.InsertAsync(session);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.Error(e.Message,e);
            }

        }

        public async Task Delete(EntityDto<Guid> input)
        {
            try
            {
                var machineSession = await sessionRepository.FirstOrDefaultAsync(e => e.Id == input.Id);

                // process remove plan inventory and other relatiob items... reserve
                await sessionRepository.DeleteAsync(machineSession);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }            
        }

        public async Task<PageResultListDto<Session>> GetAll(MachineSessionListDto input)
        {            
            var allMachineSessions = await sessionRepository.GetAllListAsync();

            int totalCount = allMachineSessions.Count();

            var machineSessions = allMachineSessions
                .OrderByDescending(e => e.Name)
                .Skip(input.SkipCount)
                 .Take(input.MaxResultCount)
                .ToList();


            var results = new PageResultListDto<Session>(machineSessions, totalCount);                           
            return results;
        }

        public async Task<Session> GetDetail(EntityDto<Guid> input)
        {
            var machineSession = await sessionRepository.FirstOrDefaultAsync(e => e.Id == input.Id);

            if (machineSession == null)
            {
                throw new UserFriendlyException("Could not found the machine session, maybe it's deleted..");
            }

            return machineSession;
        }

        public async Task<Session> Update(Session input)
        {
            var machineSession = await sessionRepository.FirstOrDefaultAsync(e => e.Id == input.Id);

            //ObjectMapper.Map(input, machine);
            machineSession.Name = input.Name;
            await sessionRepository.UpdateAsync(machineSession);
            return machineSession;
        }

        // for alert setting reserve
        public Settings.AlertConfiguration GetAlertConfiguration()
        {

            AlertConfiguration config = alertConfigurations.FirstOrDefault(x => true);
            return config != null ? config:  new Settings.AlertConfiguration
            {
                ToEmail="",
                WhenChilledMachineTemperatureAbove = 10,
                WhenHotMachineTemperatureBelow = 40,
                SendEmailWhenProductExpiredDate = 1,
                WhenStockBellow=3
            };
        }

        public AlertConfiguration UpdateAlertConfig(Settings.AlertConfiguration input)
        {
            Console.WriteLine("receive update");
            AlertConfiguration config = alertConfigurations.FirstOrDefault(x => true);
            if(config == null)
            {
                config = new AlertConfiguration();
                config.TenantId = AbpSession.GetTenantId();
                alertConfigurations.Insert(config);
            }
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
