using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using KonbiCloud.Common;
using KonbiCloud.TemperatureLogs;
using System;
using System.Linq;

namespace KonbiCloud.BackgroundJobs
{
    public class CleanTemperatureJob :PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IRepository<TemperatureLog> _temperatureLogsRepository;
        private readonly IDetailLogService _detailLogService;

        public CleanTemperatureJob(
            AbpTimer timer, 
            IRepository<TemperatureLog> temperatureLogsRepository,
            IDetailLogService detailLogService
        ) : base(timer)
        {
            Timer.Period = 60 * 1000 * 60 * 24 * 14; //2 weeks
            //Timer.Period = 60 * 1000;
            _detailLogService = detailLogService;
            _temperatureLogsRepository = temperatureLogsRepository;
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            try
            {
                var deleteTemperatureLogs = _temperatureLogsRepository.GetAll().Where(x => x.CreationTime.AddDays(14) < DateTime.Now).OrderBy(x => x.CreationTime).ToList();

                if (!deleteTemperatureLogs.Any()) return;

                _detailLogService.Log($"Clean temperature logs from: " + deleteTemperatureLogs.First().CreationTime.ToString());

                foreach(var item in deleteTemperatureLogs)
                {
                    _temperatureLogsRepository.DeleteAsync(item.Id);
                }
            }
            catch(Exception ex)
            {
                _detailLogService.Log($"Error when clear temperature logs: " + ex.Message);
            }
        }
    }
}
