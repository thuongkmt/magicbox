using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using KonbiCloud.CloudSync;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Plate;
using System;
using System.Linq;

namespace KonbiCloud.BackgroundJobs
{
    public class SyncDishJob : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IRepository<Disc, Guid> dishRepository;
        private readonly IDishSyncService dishSyncService;
        private readonly IDiscsAppService dishAppService;
        private readonly IDetailLogService detailLogService;
        private bool isSyncRunning;

        public SyncDishJob(AbpTimer timer,
                           IRepository<Disc, Guid> _dishRepository,
                           IDishSyncService _dishSyncService,
                           IDiscsAppService _dishAppService,
                           IDetailLogService detailLog) : base(timer)
        {
            Timer.Period = 10 * 60 * 1000; //10 mins
            //Timer.Period = 10000; //10 s
            dishRepository = _dishRepository;
            dishSyncService = _dishSyncService;
            dishAppService = _dishAppService;
            this.detailLogService = detailLog;
        }

        [UnitOfWork]
        protected async override void DoWork()
        {
            if (SettingManager == null)
            {
                detailLogService.Log($"Sync Dish: SettingManager is null");
                return;
            }

            detailLogService.Log($"Start sync inventory isRunning = {isSyncRunning}");
            if (isSyncRunning) return;
            isSyncRunning = true;

            var allowSyncToServer = false;
            bool.TryParse(SettingManager.GetSettingValue(AppSettingNames.AllowPushDishToServer), out allowSyncToServer);
            if (!allowSyncToServer) return;
            try
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant, AbpDataFilters.SoftDelete))
                {
                    var mId = Guid.Empty;
                    Guid.TryParse(SettingManager.GetSettingValue(AppSettingNames.MachineId), out mId);
                    if (mId == Guid.Empty)
                    {
                        detailLogService.Log($"Sync dish: Machine Id is null");
                        isSyncRunning = false;
                        return;
                    }

                    var dishes = dishRepository.GetAll().Where(x => !x.IsSynced || x.DeletionTime > x.SyncDate).ToList();
                    detailLogService.Log($"Start push {dishes.Count()} dishes to server");
                    if(dishes.Any())
                    {
                        var syncItem = new SyncedItemData<Disc>
                        {
                            MachineId = mId,
                            SyncedItems = dishes
                        };
                        var ok = await dishSyncService.PushToServer(syncItem);
                        detailLogService.Log($"Sync dish result: {ok}");

                        if (ok)
                        {
                            await dishAppService.UpdateSyncStatus(dishes.Select(x => new Plate.Dtos.DiscDto
                            {
                                Id = x.Id
                            }));
                        }
                    }
                }
                isSyncRunning = false;
            }
            catch (Exception ex)
            {
                isSyncRunning = false;
                Logger.Error($"Push dish result: {ex.Message}", ex);
            }
            finally
            {
                isSyncRunning = false;
            }
        }
    }
}
