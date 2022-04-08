using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Abp.Timing;
using KonbiCloud.Common;
using KonbiCloud.TemperatureLogs;
using System;
using System.Linq;

namespace KonbiCloud.BackgroundJobs
{
    public class CleanTemperatureJob : PeriodicBackgroundWorkerBase, ISingletonDependency
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

                _detailLogService.Log($"Start cleaning temperature");

                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    _detailLogService.Log($"Start cleaning temperature 1");

                    var deleteTemperatureLogs = _temperatureLogsRepository.GetAll().Where(x => x.CreationTime.AddDays(14) < Clock.Now).OrderBy(x => x.CreationTime).ToList();

                    _detailLogService.Log($"Start cleaning temperature 2");

                    if (!deleteTemperatureLogs.Any()) return;

                    _detailLogService.Log($"Clean temperature logs from: " + deleteTemperatureLogs.First().CreationTime.ToString());

                    foreach (var item in deleteTemperatureLogs)
                    {
                        _temperatureLogsRepository.DeleteAsync(item.Id);
                    }

                    //CurrentUnitOfWork.SaveChanges();
                    //CurrentUnitOfWork.Completed();
                }
            }
            catch (Exception ex)
            {
                _detailLogService.Log($"Error when clear temperature logs: " + ex);
            }
        }
    }
}
