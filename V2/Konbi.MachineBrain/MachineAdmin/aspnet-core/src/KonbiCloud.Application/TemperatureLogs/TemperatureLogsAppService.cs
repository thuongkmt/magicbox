using System;
using System.Linq;
using Abp.Authorization;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using KonbiCloud.Configuration;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using KonbiCloud.TemperatureLogs.Dtos;
using Konbini.Messages.Services;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Abp.Configuration;
using KonbiCloud.Authorization;

namespace KonbiCloud.TemperatureLogs
{
    [AbpAuthorize(AppPermissions.Page_Temperature)]
    public class TemperatureLogsAppService : KonbiCloudAppServiceBase, ITemperatureLogsAppService
    {
        private readonly bool _useCloud;
        private readonly Guid _machineId;
        private readonly ISendMessageToCloudService _sendMessageToCloudService;
        private readonly IRepository<TemperatureLog> _temperatureLogsRepository;

        /// <summary>
        /// Injection.
        /// </summary>
        /// <param name="temperatureLogsRepository"></param>
        public TemperatureLogsAppService(
            ISettingManager settingManager,
            IRepository<TemperatureLog> temperatureLogsRepository,
            ISendMessageToCloudService sendMessageToCloudService)
        {
            _temperatureLogsRepository = temperatureLogsRepository;
            _sendMessageToCloudService = sendMessageToCloudService;
            bool.TryParse(settingManager.GetSettingValue(AppSettingNames.UseCloud), out _useCloud);
            _machineId = Guid.Parse(settingManager.GetSettingValue(AppSettingNames.MachineId));
        }

        /// <summary>
        /// Get TemperatureLogs. 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>

        public async Task<ListResultDto<TemperatureLogListDto>> GetTemperatureLogs(GetTemperatureLogInput input)
        {
            var temperatureLogs = _temperatureLogsRepository.GetAll();
            var timeStamps = new List<DateTime>();

            if (input.DateFilter != null)
            {
                temperatureLogs = temperatureLogs.Where(p => p.CreationTime >= input.DateFilter && p.CreationTime < input.DateFilter.Value.AddDays(1));

                var startTime = input.DateFilter.Value.Date;
                do
                {
                    startTime = startTime.AddMinutes(10);
                    if (startTime > input.DateFilter.Value.AddDays(1)) break;
                    timeStamps.Add(startTime);
                } while (true);
            }
            else
            {
                temperatureLogs = temperatureLogs.Where(p => p.CreationTime >= DateTime.Now.Date && p.CreationTime < DateTime.Now.AddDays(1));
                var startTime = DateTime.Now.Date;
                do
                {
                    startTime = startTime.AddMinutes(10);
                    if (startTime > DateTime.Now.AddDays(1)) break;
                    timeStamps.Add(startTime);
                } while (true);
            }

            var data = temperatureLogs.OrderBy(x => x.CreationTime);
            var listTemperatureLog = new List<TemperatureLog>();

            if (data.Any())
            {
                foreach (var item in data)
                {
                    if (timeStamps.Any(x => (item.CreationTime.Minute == x.Minute) && (int)item.CreationTime.Subtract(x).TotalMinutes == 0))
                    {
                        item.CreationTime = new DateTime(item.CreationTime.Year, item.CreationTime.Month, item.CreationTime.Day, item.CreationTime.Hour, item.CreationTime.Minute, 0, DateTimeKind.Local);
                        listTemperatureLog.Add(item);
                    }
                }
            }

            var result = new ListResultDto<TemperatureLogListDto>(ObjectMapper.Map<List<TemperatureLogListDto>>(listTemperatureLog));
            result.Items = result.Items.OrderBy(x => x.CreationTime).ToList();

            return result;
        }

        /// <summary>
        /// Delete temperature after 2 weekly.
        /// </summary>
        /// <returns></returns>
        public async Task DeleteTemperatureLogs()
        {
            try
            {
                var temperatureLogs = _temperatureLogsRepository
                    .GetAll()
                    .Where(p => p.CreationTime.AddDays(14) < DateTime.Now)
                    .ToList();

                foreach (var item in temperatureLogs)
                {
                    // Soft delete temperatureLog.
                    await _temperatureLogsRepository.DeleteAsync(item.Id);

                    // Add log.
                    Logger.Info($"Delete TemperatureLog after 2 weekly: {item}");
                }
            }
            catch (Exception ex)
            {
                // Add message error to log.
                Logger.Error(ex.Message);
                return;
            }

        }

        /// <summary>
        /// Add new TemperatureLog.
        /// </summary>
        /// <param name="temperatureLog"></param>
        /// <returns></returns>
        [AbpAllowAnonymous]
        public async Task AddTemperatureLog(decimal temperatureLog)
        {
            try
            {
                // Add log.
                Logger.Info($"Add TemperatureLog: {temperatureLog}");

                var _temperatureLog = new TemperatureLog
                {
                    Temperature = temperatureLog
                };

                // Insert new temperaturelog.
                var newTemperatureLog = await _temperatureLogsRepository.InsertAsync(_temperatureLog);

                // Auto sync temperatureLog to Cloud.
                // TrungPQ: Check use Cloud.
                if (_useCloud)
                {
                    var result = _sendMessageToCloudService.SendQueuedMsgToCloud(new KeyValueMessage()
                    {
                        Key = MessageKeys.TemperatureLogs,
                        MachineId = _machineId,
                        Value = temperatureLog
                    });

                    if (result)
                    {
                        // Update temperaturelog with IsSynced.
                        newTemperatureLog.IsSynced = true;
                        newTemperatureLog.SyncDate = DateTime.Now;

                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Add message error to log.
                Logger.Error($"Error Insert TemperatureLogs: {ex.Message}");
                return;
            }
        }
    }
}
