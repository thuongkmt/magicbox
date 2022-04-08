using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using KonbiCloud.Machines;
using KonbiCloud.Products;
using KonbiCloud.Products.Dtos;
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
    public class ManuallySyncProductsMessageHandler : IManuallySyncProductsMessageHandler, ITransientDependency
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<ProductCategoryRelation, Guid> _productCategoryRelationRepository;
        private readonly IRepository<ProductCategory, Guid> _productCategoryRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly ISendMessageToMachineClientService _sendMessageToMachineService;
        private readonly ILogger _logger;

        public ManuallySyncProductsMessageHandler(
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<ProductCategory, Guid> productCategoryRepository,
            IRepository<ProductCategoryRelation, Guid> productCategoryRelationRepository,
            IRepository<Product, Guid> productRepository,
            IRepository<Machine, Guid> machineRepository,
            ISendMessageToMachineClientService sendMessageToMachineService)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _productCategoryRelationRepository = productCategoryRelationRepository;
            _productCategoryRepository = productCategoryRepository;
            _productRepository = productRepository;
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

                    if(machineId == null)
                    {
                        _logger.Error("Machine id is null");
                        return false;
                    }

                    int? tenantId = null;
                    var data = new List<SyncProductDto>();

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

                            var products = _productRepository.GetAll().Where(x => x.TenantId == tenantId);

                            foreach (var product in products)
                            {
                                var categoryIds = await _productCategoryRelationRepository.GetAll().Where(x => x.ProductId == product.Id).Select(x => x.ProductCategoryId).ToListAsync();
                                var categories = _productCategoryRepository.GetAll().Where(x => categoryIds.Contains(x.Id)).Select(x => new SyncProductCategoryDto
                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    Code = x.Code,
                                    Desc = x.Desc
                                }).ToList();

                                var syncProduct = new SyncProductDto
                                {
                                    Id = product.Id,
                                    Name = product.Name,
                                    SKU = product.SKU,
                                    Barcode = product.Barcode,
                                    Price = product.Price,
                                    ShortDesc = product.ShortDesc,
                                    Desc = product.Desc,
                                    Tag = product.Tag,
                                    ImageUrl = product.ImageUrl,
                                    Categories = categories
                                };

                                data.Add(syncProduct);
                            }
                        }
                    }
                    
                    _sendMessageToMachineService.SendQueuedMsgToMachines(new KeyValueMessage
                    {
                        Key = MessageKeys.ManuallySyncProduct,
                        MachineId = machineId,
                        Value = data
                    }, CloudToMachineType.ToMachineId);

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error when sync products from cloud to machine: ", ex);
                return false;
            }
        }
    }
}
