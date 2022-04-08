using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Castle.Core.Logging;
using KonbiCloud.Common;
using KonbiCloud.Inventories;
using KonbiCloud.Inventories.Dtos;
using KonbiCloud.Machines;
using Konbini.Messages.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KonbiCloud.BackgroundJobs
{
    public class UpdateMachineInventoryToDbJob : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IDetailLogService _detailLogService;
        private readonly ICacheManager _cacheManager;
        private readonly ILogger _logger;
        private readonly ISendMessageToMachineClientService _sendMessageToMachineService;
        private readonly IRepository<CurrentInventory, long> _currentInventoryRepository;
        /// <summary>
        /// list of machine which should be proceeded to update it's inventory to db
        /// </summary>
        private ConcurrentQueue<Guid> _updatingMachineList { get; }
        private bool _isInprogress = false;
        public UpdateMachineInventoryToDbJob(AbpTimer timer,
            IRepository<Machine, Guid> machineRepository,
            IDetailLogService detailLogService,
            ICacheManager cacheManager,
            ILogger logger,
             IRepository<CurrentInventory, long> currentInventoryRepository,
            ISendMessageToMachineClientService sendMessageToMachineService
        ) : base(timer)
        {
            Timer.Period = 1000; // every 1 second
            _detailLogService = detailLogService;
            _machineRepository = machineRepository;
            _cacheManager = cacheManager;
            _logger = logger;
            _sendMessageToMachineService = sendMessageToMachineService;
            _currentInventoryRepository = currentInventoryRepository;

            _updatingMachineList = new ConcurrentQueue<Guid>();

        }
        [UnitOfWork]
        protected override void DoWork()
        {
            if (_updatingMachineList.Count == 0)
                return;
            try
            {
                if (_isInprogress)
                    return;
                _isInprogress = true;
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    while (_updatingMachineList.Count > 0)
                        if (_updatingMachineList.TryDequeue(out Guid machineId))
                        {
                            var machine = _machineRepository.GetAllIncluding(el => el.CurrentInventory).Where(el => el.Id == machineId).FirstOrDefault();

                            if (machine != null)
                            {
                                var machineInventory = GetMachineInventoryFromCache(machine.Id);
                                if (machine.CurrentInventory.Count > 0)
                                    machine.CurrentInventory.Clear();
                                machineInventory.Tags.ForEach(el =>
                                {
                                    machine.CurrentInventory.Add(new CurrentInventory()
                                    {
                                        MachineId = machineId,
                                        ProductName = el.ProductName,
                                        Tag = el.Tag
                                    });
                                });
                                machine.StockLastUpdated = machineInventory.LastUpdated;
                                _machineRepository.Update(machine);
                            }
                        }


                    CurrentUnitOfWork.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _detailLogService.Log($"Error when clear temperature logs: " + ex.Message);
            }
            finally
            {
                _isInprogress = false;
            }

        }
        public void RequestUpdatingInventory(Guid machineId)
        {

            if (!_updatingMachineList.Contains(machineId))
            {
                _updatingMachineList.Enqueue(machineId);
            }
        }
        private MachineInventoryCacheDto GetMachineInventoryFromCache(Guid machineId)
        {
            var cache = _cacheManager.GetCache(Common.Const.ProductTagRealtime);
            return cache.Get(machineId.ToString(), () => {
                var output = new MachineInventoryCacheDto() { MachineId = machineId, Tags = new List<TagProductDto>() };
                try
                {
                    var machine = _machineRepository.Get(machineId);
                    var result = _currentInventoryRepository.GetAll().Where(el => el.MachineId == machineId).ToList();
                    result.ForEach(el => output.Tags.Add(new TagProductDto() { Tag = el.Tag, ProductName = el.ProductName }));
                    output.LastUpdated = machine.StockLastUpdated;
                }
                catch (Exception ex)
                {

                    Logger.Error(ex.Message, ex);
                }
                return output;

            });
        }
    }
}
