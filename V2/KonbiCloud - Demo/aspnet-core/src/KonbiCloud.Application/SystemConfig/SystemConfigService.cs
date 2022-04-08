using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Runtime.Security;
using Castle.Core.Logging;
using KonbiCloud.Machines;
using KonbiCloud.SystemConfig.Dtos;
using Microsoft.EntityFrameworkCore;


namespace KonbiCloud.SystemConfig
{
    public class SystemConfigService : KonbiCloudAppServiceBase, ISystemConfig
    {
        //private readonly IRepository<Machine, Guid> machineRepository;
        private readonly ILogger _logger;
        private const string passwordKey = "Abp.Net.Mail.Smtp.Password";
        public SystemConfigService(ILogger logger)
        {
            //this.machineRepository = machineRepository;
            _logger = logger;
        }

        public async Task<ListResultDto<SystemConfigDto>> GetAll(SystemConfigListInput input)
        {
            try
            {
                List<SystemConfigDto> result = new List<SystemConfigDto>();

                var tenantId = AbpSession.TenantId ?? 0;

                //if (tenantId == 0 && input.MachineId.HasValue)
                //{
                //    var machine = await machineRepository.FirstOrDefaultAsync(x => x.Id == input.MachineId.Value);
                //    tenantId = machine?.TenantId ?? 0;
                //}

                var lstSettingForTenant = new List<ISettingValue>();

                try
                {
                    var settingForTenant = await SettingManager.GetAllSettingValuesForTenantAsync(tenantId);
                    lstSettingForTenant = settingForTenant.ToArray().ToList();
                }
                catch (Exception exeption)
                {
                    _logger.Info(exeption.Message);
                    Console.WriteLine(exeption.Message);
                }

                //var userIdentify = (await UserManager.GetUserByIdAsync((long)AbpSession.UserId)).ToUserIdentifier();
                //var settingForUser = await SettingManager.GetAllSettingValuesForUserAsync(userIdentify);

                var settingsAll = await SettingManager.GetAllSettingValuesAsync();

                foreach (var item in settingsAll)
                {
                    var itemTenant = lstSettingForTenant.Find(x => x.Name == item.Name);

                    var newItem = new SystemConfigDto();
                    if (itemTenant != null)
                    {
                        newItem.Name = itemTenant.Name;
                        if (newItem.Name.Equals(passwordKey))
                        {
                            newItem.Value = SimpleStringCipher.Instance.Decrypt(itemTenant.Value);
                        }
                        else
                        {
                            newItem.Value = itemTenant.Value;
                        }
                    }
                    else
                    {
                        newItem.Name = item.Name;
                        if (newItem.Name.Equals(passwordKey))
                        {
                            newItem.Value = SimpleStringCipher.Instance.Decrypt(item.Value);
                        }
                        else
                        {
                            newItem.Value = item.Value;
                        }
                    }
                    result.Add(newItem);
                }
                return new ListResultDto<SystemConfigDto>(result.MapTo<List<SystemConfigDto>>());
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return new ListResultDto<SystemConfigDto>();
            }
        }

        public async Task Update(UpdateConfigInput input)
        {
            try
            {
                var tenantId = AbpSession.TenantId ?? 0;
                if (input.Name.Equals(passwordKey))
                {
                    await SettingManager.ChangeSettingForTenantAsync(tenantId, input.Name, SimpleStringCipher.Instance.Encrypt(input.Value));
                }
                else
                {
                    await SettingManager.ChangeSettingForTenantAsync(tenantId, input.Name, input.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }

        }
    }
}
