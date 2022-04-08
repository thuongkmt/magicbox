using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Abp.Timing;
using KonbiCloud.Diagnostic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace KonbiCloud.BackgroundJobs
{
    public class CleanOldMachineDataBackgroundJobs : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IRepository<MachineError> machineErrorRepository;
        private readonly IRepository<VendingStatus> vendingStatusRepository;
        private readonly IRepository<VendingHistory, long> vendingHistoryRepository;

        public CleanOldMachineDataBackgroundJobs(AbpTimer timer, IRepository<MachineError> _machineErrorRepository, IRepository<VendingStatus> _vendingStatusRepository, IRepository<VendingHistory, long> _vendingHistoryRepository)
       : base(timer)
        {            
            Timer.Period = 7*24*3600000; //weekly
            this.machineErrorRepository = _machineErrorRepository;
            this.vendingStatusRepository = _vendingStatusRepository;
            this.vendingHistoryRepository = _vendingHistoryRepository;
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Logger.Info("Start remove old data progress");
           
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var oneWeekAgo = Clock.Now.Subtract(TimeSpan.FromDays(7));

                List<VendingHistory> needRevovingHistoryItems = vendingHistoryRepository.GetAll().Where(h => h.CreationTime < oneWeekAgo).ToList();
                foreach(VendingHistory deleteItem in needRevovingHistoryItems)
                {
                    vendingHistoryRepository.Delete(deleteItem);
                }
                Logger.Info("Implement delete "+needRevovingHistoryItems.Count().ToString()+" from VendingHistory");

                List<VendingStatus> needRevovingVendingStatusItems = vendingStatusRepository.GetAll().Where(h => h.CreationTime < oneWeekAgo).ToList();
                foreach (VendingStatus deleteItem in needRevovingVendingStatusItems)
                {
                    vendingStatusRepository.Delete(deleteItem);
                }
                Logger.Info("Implement delete " + needRevovingVendingStatusItems.Count().ToString() + " from VendingStatus");                                

                CurrentUnitOfWork.SaveChanges();
            }

            watch.Stop();          
            Logger.Info("Remove old data progress wast milisecond: "+ watch.ElapsedMilliseconds);
        }

    }
}
