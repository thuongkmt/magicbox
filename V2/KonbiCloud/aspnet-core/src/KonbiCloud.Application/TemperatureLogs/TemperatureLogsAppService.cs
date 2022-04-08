using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Timing;
using KonbiCloud.Machines;
using KonbiCloud.TemperatureLogs.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KonbiCloud.TemperatureLogs
{
    /// <summary>
    /// TemperatureLogs Appplication Service.
    /// </summary>
    public class TemperatureLogsAppService : KonbiCloudAppServiceBase, ITemperatureLogsAppService
    {
        private readonly IRepository<TemperatureLog> _temperatureLogsRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;

        /// <summary>
        /// Injection.
        /// </summary>
        /// <param name="temperatureLogsRepository"></param>
        public TemperatureLogsAppService(
            IRepository<TemperatureLog> temperatureLogsRepository,
            IRepository<Machine, Guid> machineRepository
            )
        {
            _temperatureLogsRepository = temperatureLogsRepository;
            _machineRepository = machineRepository;
        }

        /// <summary>
        /// Get TemperatureLogs. 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ListResultDto<TemperatureLogListDto>> GetTemperatureLogs(GetTemperatureLogInput input)
        {
            List<string> MachineIdString = input.Filter.Split(',').ToList();

            List<Guid> MachineIds = new List<Guid>();
            foreach (var item in MachineIdString)
            {
                MachineIds.Add(new Guid(item));
            }

            var temperatureLogs = _temperatureLogsRepository
                .GetAllIncluding(x => x.Machine);

            var timeStamps = new List<DateTime>();

            if (input.DateFilter != null)
            {
                temperatureLogs = temperatureLogs.Where(p => MachineIds.Contains(p.MachineId) && p.CreationTime >= input.DateFilter && p.CreationTime < input.DateFilter.Value.AddDays(1));
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
                temperatureLogs = temperatureLogs.Where(p => MachineIds.Contains(p.MachineId) && p.CreationTime >= Clock.Now.Date && p.CreationTime < Clock.Now.AddDays(1));
                var startTime = Clock.Now.Date;
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
                foreach(var item in data)
                {
                    if(timeStamps.Any(x => (item.CreationTime.Minute == x.Minute) && (int)item.CreationTime.Subtract(x).TotalMinutes == 0))
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
        public async Task DeleteTemperatureLogs(List<Guid> MachineIds)
        {
            try
            {
                var temperatureLogs = _temperatureLogsRepository
                .GetAllIncluding(x => x.Machine)
                .Where(p => MachineIds.Contains(p.MachineId) && p.CreationTime.AddDays(14) < Clock.Now)
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
    }
}
