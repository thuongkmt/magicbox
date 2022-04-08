using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using Castle.Core.Logging;
using KonbiCloud.Machines;
using KonbiCloud.Products;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Konbini.Messages.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KonbiCloud.Messaging.Handlers
{
    public class ManuallySyncProductCategoriesMessageHandler : IManuallySyncProductCategoriesMessageHandler, ITransientDependency
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<ProductCategory, Guid> _productCategoryRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly ISendMessageToMachineClientService _sendMessageToMachineService;
        private readonly ILogger _logger;

        public ManuallySyncProductCategoriesMessageHandler(
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<ProductCategory, Guid> productCategoryRepository,
            IRepository<Machine, Guid> machineRepository,
            ISendMessageToMachineClientService sendMessageToMachineService)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _productCategoryRepository = productCategoryRepository;
            _machineRepository = machineRepository;
            _sendMessageToMachineService = sendMessageToMachineService;
        }

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    var machineId = keyValueMessage.MachineId;

                    if (machineId == null)
                    {
                        _logger.Error("Machine id is null");
                        return false;
                    }

                    int? tenantId = null;
                    var data = new List<ProductCategory>();

                    using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant,AbpDataFilters.MustHaveTenant))
                    {
                        var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == machineId);
                        if (machine == null)
                        {
                            _logger.Error("Machine is null");
                            return false;
                        }
                        else
                        {
                            tenantId = machine.TenantId;
                            var categories = await _productCategoryRepository.GetAll().Where(x => x.TenantId == tenantId).ToListAsync();

                            foreach (var category in categories)
                            {
                                data.Add(new ProductCategory
                                {
                                    Id = category.Id,
                                    Name = category.Name,
                                    Code = category.Code,
                                    Desc = category.Desc
                                });
                            }
                        }
                    }

                    _sendMessageToMachineService.SendQueuedMsgToMachines(new KeyValueMessage
                    {
                        Key = MessageKeys.ManuallySyncProductCategory,
                        MachineId = machineId,
                        Value = data
                    }, CloudToMachineType.ToMachineId);

                    return true;
                }
            }
            catch(Exception ex)
            {
                _logger.Error("Error when sync product categories from cloud to machine: ", ex);
                return false;
            }
        }
    }
}
