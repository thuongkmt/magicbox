using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using KonbiCloud.Authorization;
using KonbiCloud.Common;
using KonbiCloud.Common.Dtos;
using KonbiCloud.Inventories;
using KonbiCloud.Machines;
using KonbiCloud.Machines.Dtos;
using KonbiCloud.MultiTenancy;
using KonbiCloud.Products.Dtos;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Konbini.Messages.Services;
using Microsoft.EntityFrameworkCore;

namespace KonbiCloud.Products
{
    [AbpAuthorize(AppPermissions.Pages_ProductsMachinePrice)]
    public class ProductMachinePriceAppService : KonbiCloudAppServiceBase, IProductMachinePriceAppService
    {
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<ProductMachinePrice> _productMachinePriceRepository;
        private readonly ISendMessageToMachineClientService _sendMessageToMachineService;
        private readonly IDetailLogService _detailLogService;
        private readonly IRepository<Topup, Guid> _topupRepository;
        private readonly IRepository<InventoryItem, Guid> _inventoryRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IRepository<Tenant> _tenantRepository;
        /// <summary>
        /// Injection ProductMachinePriceAppService.
        /// </summary>
        /// <param name="machineRepository"></param>
        /// <param name="productRepository"></param>
        /// <param name="_productMachinePriceRepository"></param>
        public ProductMachinePriceAppService(
            IDetailLogService logService,
            IRepository<Product, Guid> productRepository,
            IRepository<ProductMachinePrice> productMachinePriceRepository,
            ISendMessageToMachineClientService sendMessageToMachineService,
            IRepository<Topup,Guid> topupRepository,
            IRepository<InventoryItem,Guid> inventoryRepository, 
            IRepository<Machine, Guid> machineRepository,
            IRepository<Tenant> tenantRepository
        )
        {
            _detailLogService = logService;
            _productRepository = productRepository;
            _productMachinePriceRepository = productMachinePriceRepository;
            _sendMessageToMachineService = sendMessageToMachineService;
            _topupRepository = topupRepository;
            _inventoryRepository = inventoryRepository;
            _machineRepository = machineRepository;
            _tenantRepository = tenantRepository;
        }

        /// <summary>
        /// Get ProductMachinePrices. 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultDto<ProductDto>> GetProductMachinePrices(GetProductMachinePriceInput input)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input.Filter))
                {
                    return new PagedResultDto<ProductDto>();
                }

                // Get all Products by machine.
                var productMachinePrices = _productMachinePriceRepository
                                            .GetAllIncluding(x => x.Product, x => x.Machine)
                                            .Where(p => p.MachineId == new Guid(input.Filter))
                                            .OrderBy(p => p.CreationTime)
                                            .ToList();

                // Get all products.
                var allProducts = _productRepository.GetAll();

                if(input.CategoryFilter != Guid.Empty)
                {
                    allProducts = allProducts.Where(x => x.ProductCategoryRelations.Any(p => p.ProductCategoryId == input.CategoryFilter));
                }

                var query = from o in allProducts select ObjectMapper.Map<ProductDto>(o);

                var totalCount = query.Count();

                List<ProductDto> products = await query
                    .OrderBy(input.Sorting ?? "name asc")
                    .PageBy(input)
                    .ToListAsync();

                // Set Price with machine.
                foreach (var item in products)
                {
                    var exists = productMachinePrices.Find(x => x.ProductId == item.Id);
                    if (exists != null)
                    {
                        item.Price = Math.Round((double)exists.Price,2);
                    }
                    // Using defaul price.
                    //else
                    //{
                    //    item.Product.Price = null;
                    //}
                }

                return new PagedResultDto<ProductDto>(
                    totalCount,
                    products
                );
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new PagedResultDto<ProductDto>();
            }
        }

        /// <summary>
        /// Update or Create Product Machine Prices.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// 
        [AbpAuthorize(AppPermissions.Pages_ProductsMachinePrice_Edit)]
        public async Task<bool> UpdateProductMachinePrices(UpdateProductMachinePriceInput input)
        {
            try
            {
                int? tenantId = null;

                if(AbpSession.TenantId != null)
                {
                    tenantId = AbpSession.TenantId;
                }

                // Find ProductMachinePrice.
                var exists = _productMachinePriceRepository.GetAll()
                    .FirstOrDefault(x => x.MachineId == input.MachineId && x.ProductId == input.ProductId);

                if (exists != null)
                {
                    // Update price.
                    exists.Price = input.Price;
                }
                else
                {
                    // Create new price.
                    var productMachinePrice = new ProductMachinePrice
                    {
                        MachineId = input.MachineId,
                        ProductId = input.ProductId,
                        Price = input.Price,
                        TenantId = tenantId
                    };

                    // Set value for exists then auto sync.
                    exists = productMachinePrice;

                    _detailLogService.Log($"Change ProductMachinePrice: {exists}");

                    await _productMachinePriceRepository.InsertAsync(productMachinePrice);
                }

                //Update price for inventories
                var currentTopup = _topupRepository.GetAll().Where(x => x.MachineId == input.MachineId && x.EndDate == null).FirstOrDefault();
                if(currentTopup != null)
                {
                    var topUpId = currentTopup.Id;
                    var currentTopupInventories = _inventoryRepository.GetAll().Where(x => x.MachineId == input.MachineId && x.ProductId == input.ProductId && x.DetailTransactionId == null);

                    if(currentTopupInventories.Any())
                    {
                        foreach(var item in currentTopupInventories)
                        {
                            item.Price = (double)input.Price;
                            await _inventoryRepository.UpdateAsync(item);
                        }
                    }
                }

                _detailLogService.Log($"Auto sync ProductMachinePrice: {exists}");

                // Auto sync to local machine.
                _sendMessageToMachineService.SendQueuedMsgToMachines(new KeyValueMessage
                {
                    Key = MessageKeys.ProductMachinePrice,
                    Value = exists,
                    MachineId = input.MachineId
                }, CloudToMachineType.ToMachineId);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return false;
            }
        }

        public async Task<PageResultListDto<MachineListDto>> GetAllMachines(MachineInputListDto input)
        {
            var allMachines = _machineRepository.GetAll();
            int totalCount = await allMachines.CountAsync();

            if (string.IsNullOrEmpty(input.Sorting) || input.Sorting == "undefined")
            {
                input.Sorting = "name asc";
            }
            var machines = await allMachines
                .OrderBy(input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var results = new PageResultListDto<MachineListDto>(machines.MapTo<List<MachineListDto>>(), totalCount);

            var tenants = await _tenantRepository.GetAllListAsync();
            foreach (var item in results.Items)
            {
                item.TenantName = tenants.FirstOrDefault(x => x.Id == item.TenantId)?.Name ?? "";
            }
            return results;
        }
    }
}
