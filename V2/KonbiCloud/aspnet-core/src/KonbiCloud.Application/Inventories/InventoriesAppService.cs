using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Runtime.Caching;
using Abp.UI;
using Abp.Timing;
using KonbiCloud.Authorization;
using KonbiCloud.Common;
using KonbiCloud.Dto;
using KonbiCloud.Inventories.Dtos;
using KonbiCloud.Inventories.Exporting;
using KonbiCloud.Machines;
using KonbiCloud.Migrations;
using KonbiCloud.Products;
using KonbiCloud.Transactions;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Konbini.Messages.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;

namespace KonbiCloud.Inventories
{
    [AbpAuthorize(AppPermissions.Pages_Inventories)]
    public class InventoriesAppService : KonbiCloudAppServiceBase, IInventoriesAppService
    {
        private readonly IRepository<InventoryItem, Guid> _inventoryRepository;
        private readonly IRepository<CurrentInventory, long> _currentInventoryRepository;
        private readonly IInventoriesExcelExporter _inventoriesExcelExporter;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IRepository<Topup, Guid> _topupRepository;
        private readonly IRepository<KonbiCloud.Restock.RestockSession, int> _restockSessionRepository;

        private readonly IRepository<DetailTransaction, long> _detailTransactRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<ProductTag, Guid> _productTagRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IDetailLogService _detailLogService;
        private readonly IRepository<TopupHistory, long> _topupHistoryRepository;
        private readonly ISendMessageToMachineClientService _sendMessageToMachineService;


        public InventoriesAppService(IRepository<InventoryItem, Guid> inventoryRepository,
              IInventoriesExcelExporter inventoriesExcelExporter,
              IRepository<Product, Guid> productRepository,
              IRepository<Machine, Guid> machineRepository,
              IRepository<Topup, Guid> topupRepository,
              IRepository<DetailTransaction, long> detailTransactRepository,
              ICacheManager cacheManager,
              IRepository<ProductTag, Guid> productTagRepository,
              IDetailLogService detailLogService,
              IRepository<TopupHistory, long> topupHistoryRepository,
              IRepository<CurrentInventory, long> currentInventoryRepository,
              ISendMessageToMachineClientService sendMessageToMachineService,
              IUnitOfWorkManager unitOfWorkManager,
              IRepository<KonbiCloud.Restock.RestockSession, int> restockSessionRepository)
        {
            _inventoryRepository = inventoryRepository;
            _inventoriesExcelExporter = inventoriesExcelExporter;
            _productRepository = productRepository;
            _machineRepository = machineRepository;
            _topupRepository = topupRepository;
            _detailTransactRepository = detailTransactRepository;
            _cacheManager = cacheManager;
            _productTagRepository = productTagRepository;
            _detailLogService = detailLogService;
            _topupHistoryRepository = topupHistoryRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _currentInventoryRepository = currentInventoryRepository;
            _sendMessageToMachineService = sendMessageToMachineService;
            _restockSessionRepository = restockSessionRepository;
        }

        public async Task<PagedResultDto<GetInventoryForViewDto>> GetAll(GetAllInventoriesInput input)
        {

            var filteredInventories = _inventoryRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.TagId.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.TagIdFilter), e => e.TagId.ToLower() == input.TagIdFilter.ToLower().Trim())
                        .WhereIf(input.MinTrayLevelFilter != null, e => e.TrayLevel >= input.MinTrayLevelFilter)
                        .WhereIf(input.MaxTrayLevelFilter != null, e => e.TrayLevel <= input.MaxTrayLevelFilter)
                        .WhereIf(input.MinPriceFilter != null, e => e.Price >= input.MinPriceFilter)
                        .WhereIf(input.MaxPriceFilter != null, e => e.Price <= input.MaxPriceFilter);


            var query = (from o in filteredInventories
                         join o1 in _productRepository.GetAll() on o.ProductId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         select new GetInventoryForViewDto()
                         {
                             Inventory = ObjectMapper.Map<InventoryDto>(o),
                             ProductName = s1 == null ? "" : s1.Name.ToString()
                         })
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ProductNameFilter), e => e.ProductName.ToLower() == input.ProductNameFilter.ToLower().Trim());

            var totalCount = await query.CountAsync();

            var inventories = await query
                .OrderBy(input.Sorting ?? "inventory.id asc")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<GetInventoryForViewDto>(
                totalCount,
                inventories
            );
        }

        public async Task<GetInventoryForViewDto> GetInventoryForView(Guid id)
        {
            var inventory = await _inventoryRepository.GetAsync(id);

            var output = new GetInventoryForViewDto { Inventory = ObjectMapper.Map<InventoryDto>(inventory) };

            if (output.Inventory.ProductId != null)
            {
                var product = await _productRepository.FirstOrDefaultAsync((Guid)output.Inventory.ProductId);
                output.ProductName = product.Name.ToString();
            }

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_Inventories_Edit)]
        public async Task<GetInventoryForEditOutput> GetInventoryForEdit(EntityDto<Guid> input)
        {
            var inventory = await _inventoryRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetInventoryForEditOutput { Inventory = ObjectMapper.Map<CreateOrEditInventoryDto>(inventory) };

            if (output.Inventory.ProductId != null)
            {
                var product = await _productRepository.FirstOrDefaultAsync((Guid)output.Inventory.ProductId);
                output.ProductName = product.Name.ToString();
            }

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditInventoryDto input)
        {
            if (input.Id == null)
            {
                await Create(input);
            }
            else
            {
                await Update(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Inventories_Create)]
        private async Task Create(CreateOrEditInventoryDto input)
        {
            var inventory = ObjectMapper.Map<InventoryItem>(input);


            if (AbpSession.TenantId != null)
            {
                inventory.TenantId = (int?)AbpSession.TenantId;
            }


            await _inventoryRepository.InsertAsync(inventory);
        }

        [AbpAuthorize(AppPermissions.Pages_Inventories_Edit)]
        private async Task Update(CreateOrEditInventoryDto input)
        {
            var inventory = await _inventoryRepository.FirstOrDefaultAsync((Guid)input.Id);
            ObjectMapper.Map(input, inventory);
        }

        [AbpAuthorize(AppPermissions.Pages_Inventories_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            await _inventoryRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetInventoriesToExcel(GetAllInventoriesForExcelInput input)
        {

            var filteredInventories = _inventoryRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.TagId.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.TagIdFilter), e => e.TagId.ToLower() == input.TagIdFilter.ToLower().Trim())
                        .WhereIf(input.MinTrayLevelFilter != null, e => e.TrayLevel >= input.MinTrayLevelFilter)
                        .WhereIf(input.MaxTrayLevelFilter != null, e => e.TrayLevel <= input.MaxTrayLevelFilter)
                        .WhereIf(input.MinPriceFilter != null, e => e.Price >= input.MinPriceFilter)
                        .WhereIf(input.MaxPriceFilter != null, e => e.Price <= input.MaxPriceFilter);


            var query = (from o in filteredInventories
                         join o1 in _productRepository.GetAll() on o.ProductId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         select new GetInventoryForViewDto()
                         {
                             Inventory = ObjectMapper.Map<InventoryDto>(o),
                             ProductName = s1 == null ? "" : s1.Name.ToString()
                         })
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ProductNameFilter), e => e.ProductName.ToLower() == input.ProductNameFilter.ToLower().Trim());


            var inventoryListDtos = await query.ToListAsync();

            return _inventoriesExcelExporter.ExportToFile(inventoryListDtos);
        }



        [AbpAuthorize(AppPermissions.Pages_Inventories)]
        public async Task<PagedResultDto<ProductLookupTableDto>> GetAllProductForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _productRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Name.ToString().Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var productList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<ProductLookupTableDto>();
            foreach (var product in productList)
            {
                lookupTableDtoList.Add(new ProductLookupTableDto
                {
                    Id = product.Id.ToString(),
                    DisplayName = product.Name?.ToString()
                });
            }

            return new PagedResultDto<ProductLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }


        [AbpAuthorize(AppPermissions.Pages_Inventories)]
        public async Task<ListResultDto<InventoryOverviewDto>> GetInventoryOverview()
        {
            try
            {
                var result = await _topupRepository.GetAll()
                    .Where(x => x.EndDate == null)
                    .Select(x => new InventoryOverviewDto()
                    {
                        MachineId = x.MachineId,
                        TopupId = x.Id,
                        MachineName = x.Machine.Name,
                        TopupDate = x.StartDate,
                        Total = x.Total,
                        Sold = x.Sold,
                    })
                    .ToListAsync();

                return new ListResultDto<InventoryOverviewDto>(result);
            }
            catch (Exception ex)
            {
                Logger.Error("Error", ex);
                throw new UserFriendlyException("Error", ex);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Inventories)]
        public async Task<InventoryDetailOutput> GetInventoryDetail(Guid machineId, Guid topupId)
        {
            try
            {
                var topup = await _topupRepository.GetAsync(topupId);
                var machine = await _machineRepository.GetAsync(machineId);

                if (topup == null || machine == null)
                {
                    Logger.Error("InventoryItem Detail: Can not found the topup in the system.");
                    throw new UserFriendlyException("Can not found the topup in the system.");
                }

                var output = new InventoryDetailOutput()
                {
                    MachineName = machine.Name,
                    TopupDate = topup.StartDate,
                    Total = topup.Total,
                    //Sold = topup.Sold,
                    Error = topup.Error
                };

                var detailList = new List<InventoryDetailDto>();
                var inventories = await _inventoryRepository.GetAllIncluding()
                    .Where(x => x.TopupId == topupId && x.MachineId == machineId).Include(x => x.Product)
                    .Include(x => x.Topup)
                    .ToListAsync();

                var trans = await _detailTransactRepository.GetAll()
                    .Include(x => x.Inventories).ThenInclude(i => i.Product)
                    .Where(x => x.TopupId == topupId && x.MachineId == machineId).ToListAsync();

                var products = inventories.Select(x => x.Product).Distinct().ToList();
                foreach (var product in products)
                {
                    var productInventories = inventories.Where(x => x.ProductId == product.Id);
                    var item = new InventoryDetailDto()
                    {
                        ProductName = product.Name,
                        Total = productInventories.Count()
                    };

                    //find all trans with that contain current product
                    var tranInventories = trans.Where(x => x.Inventories.Any(i => i.ProductId == product.Id) && x.Status == Enums.TransactionStatus.Success).ToList();


                    item.LastSoldDate =
                        tranInventories.OrderByDescending(x => x.PaymentTime).FirstOrDefault()?.PaymentTime ?? null;

                    item.Sold = tranInventories.Select(x => x.Inventories.Count(i => i.ProductId == product.Id && x.Status == Enums.TransactionStatus.Success)).Sum(x => x);
                    detailList.Add(item);
                }

                output.InventoryDetailList = detailList;
                return output;
            }
            catch (Exception ex)
            {
                Logger.Error("Error", ex);
                throw new UserFriendlyException("Error", ex);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Inventories)]
        public async Task<PagedResultDto<CurrentInventoryDto>> CurrentInventories(GetCurrentInventoryInput input)
        {
            var result = new PagedResultDto<CurrentInventoryDto>(0, new List<CurrentInventoryDto>());
            try
            {
                //var dataTest = new List<KeyValuePair<string, string>>
                //{
                //    new KeyValuePair<string, string>("tag77", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("tag88", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("tag99", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("tag100", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("abc", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("defs", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("fhfgh", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("uioiu", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("werwer", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("nghkjh", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("lk;lk;", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("dsfsdf", Guid.NewGuid().ToString())
                //};

                ////Set cache
                //await _cacheManager.GetCache(Common.Const.ProductTagRealtime)
                //                   .SetAsync("ad76de49-47d2-fb93-37ea-f08d40dbe152", dataTest);

                //var dataTest2 = new List<KeyValuePair<string, string>>
                //{
                //    new KeyValuePair<string, string>("dsfg", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("hkjk", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("uioiuo", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("qewqw", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("abc", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("defs", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("fhfgh", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("uioiu", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("werwer", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("nghkjh", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("lk;lk;", Guid.NewGuid().ToString()),
                //    new KeyValuePair<string, string>("dsfsdf", Guid.NewGuid().ToString())
                //};

                //await _cacheManager.GetCache(Common.Const.ProductTagRealtime)
                //                   .SetAsync("cd0445a9-b4ad-4f69-a772-ca5eac1be248", dataTest2);


                var machines = await _machineRepository.GetAllListAsync(x => input.MachinesFilter == null ||
                                                                             input.MachinesFilter.Contains(x.Id));

                var allItems = new List<CurrentInventoryDto>();

                foreach (var machine in machines)
                {
                    var machineInventory = GetMachineInventoryFromCache(machine.Id);

                    if (machineInventory.Tags.Count == 0) continue;

                    var subItems = machineInventory.Tags.Select(x => new CurrentInventoryDto()
                    {
                        Name = x.ProductName,
                        TagId = x.Tag,
                        MachineName = machine.Name
                    });
                    allItems.AddRange(subItems);
                }

                //Filter
                allItems = allItems.Where(x => (string.IsNullOrEmpty(input.Filter) || x.Name.ToLower().Contains(input.Filter.ToLower())) &&
                                               (string.IsNullOrEmpty(input.TagIdFilter) || x.TagId.ToLower().Contains(input.TagIdFilter.ToLower()))
                                          ).ToList();

                var totalCount = allItems.Count;
                var filtered = new List<CurrentInventoryDto>();
                if (!string.IsNullOrEmpty(input.Sorting))
                {
                    var sortParams = input.Sorting.Split(" ");
                    if (sortParams.Length == 2)
                    {
                        var field = sortParams[0];
                        var order = sortParams[1];
                        if ("name".Equals(field))
                        {
                            if ("ASC".Equals(order))
                            {
                                filtered = allItems.OrderBy(x => x.Name)
                                                   .Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
                            }
                            else
                            {
                                filtered = allItems.OrderByDescending(x => x.Name)
                                                   .Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
                            }
                        }
                        else if ("tagid".Equals(field))
                        {
                            if ("ASC".Equals(order))
                            {
                                filtered = allItems.OrderBy(x => x.TagId)
                                                   .Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
                            }
                            else
                            {
                                filtered = allItems.OrderByDescending(x => x.TagId)
                                                   .Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
                            }
                        }
                        else if ("machinename".Equals(field))
                        {
                            if ("ASC".Equals(order))
                            {
                                filtered = allItems.OrderBy(x => x.MachineName).ThenBy(x => x.Name)
                                                   .Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
                            }
                            else
                            {
                                filtered = allItems.OrderByDescending(x => x.MachineName).ThenBy(x => x.Name)
                                                   .Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
                            }
                        }
                    }
                }
                else
                {
                    filtered = allItems.OrderBy(x => x.MachineName)
                                        .ThenBy(x => x.TagId).Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
                }

                return result = new PagedResultDto<CurrentInventoryDto>(totalCount, filtered);
            }
            catch (Exception ex)
            {
                var x = ex.Message;
            }

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Inventories)]
        public async Task<ListResultDto<InventoryOverviewDto>> GetMachinesInventoryRealTime()
        {
            try
            {
                var result = await _machineRepository.GetAll().Select(x => new InventoryOverviewDto()
                {
                    MachineId = x.Id,
                    MachineName = x.Name,
                })
                .ToListAsync();

                foreach (var item in result)
                {
                    var topUp = await _topupRepository.GetAll().Where(x => x.MachineId == item.MachineId).OrderByDescending(el => el.StartDate).FirstOrDefaultAsync();

                    if (topUp != null)
                    {
                        item.TopupId = topUp.Id;
                        item.TopupDate = topUp.StartDate;
                        item.Sold = _inventoryRepository.GetAll().Where(el => el.TopupId == topUp.Id && el.Transaction.Status == Enums.TransactionStatus.Success).Count();
                        item.Total = topUp.Total;


                    }
                    //Get current stock number from cache
                    var machineInventory = GetMachineInventoryFromCache(item.MachineId);
                    item.CurrentStock = machineInventory.Tags.Count;
                    item.LastUpdated = machineInventory.LastUpdated;
                }

                return new ListResultDto<InventoryOverviewDto>(result.OrderBy(x => x.MachineName).ToList());
            }
            catch (Exception ex)
            {
                Logger.Error("Error", ex);
                throw new UserFriendlyException("Error", ex);
            }

        }

        public async Task<List<InventoryDetailForRepportOutput>> GetInventoryForReportByMachine(Guid machineId)
        {
            try
            {
                var results = new List<InventoryDetailForRepportOutput>();
                var machine = _machineRepository.GetAll().Where(x => x.Id == machineId).FirstOrDefault();

                var topUp = await _topupRepository.FirstOrDefaultAsync(x => x.MachineId == machine.Id && x.EndDate == null); ;
                //var machine = await _machineRepository.GetAsync(machineId);
                var topupId = topUp.Id;

                if (machine == null)
                {
                    Logger.Error("Can not found the machine in the system. ID: " + machineId);
                    throw new UserFriendlyException("Can not found the machine in the system.");
                }

                //Get machine inventory from cache

                var machineInventory = GetMachineInventoryFromCache(machineId);

                var inventoryDetailList = new List<InventoryDetailForReportDto>();

                var output = new InventoryDetailForRepportOutput()
                {
                    MachineName = machine.Name,
                    CurrentStock = machineInventory.Tags.Count
                };

                //If the machine has no topup => has no data in inventory, lookup from cache only
                if (topupId == null || topupId == Guid.Empty)
                {

                    output.Total = machineInventory.Tags.Count;
                    //Get all tags from cache => find all products have those tags
                    if (machineInventory.Tags.Count > 0)
                    {
                        var tagNames = machineInventory.Tags.Select(x => x.Tag).ToList();
                        var productIds = _productTagRepository.GetAll().Where(x => tagNames.Contains(x.Name)).Select(x => x.ProductId).Distinct().ToList();
                        var products = _productRepository.GetAll()
                            .Include("Inventories.Product.ProductCategoryRelations.ProductCategory")
                            .Where(x => productIds.Contains(x.Id));

                        foreach (var item in products)
                        {
                            var productTagNames = _productTagRepository.GetAll().Where(x => x.ProductId == item.Id).Select(x => x.Name).ToList();

                            //Get current stock number from cache
                            var currentStockNumber = machineInventory.Tags.Where(x => productTagNames.Contains(x.Tag)).Count();

                            var cateName = item.ProductCategoryRelations.Select(c => c.ProductCategory.Name).ToList();
                            var cate = string.Join(", ", cateName);

                            var inventoryDetail = new InventoryDetailForReportDto
                            {
                                ProductName = item.Name,
                                ProductCategory = cate,
                                ProductSku = item.SKU,
                                ProductDescription = item.Desc,
                                ProductPrice = item.Price,
                                Total = currentStockNumber,
                                CurrentStock = currentStockNumber,
                                Sold = 0,
                            };

                            inventoryDetailList.Add(inventoryDetail);
                        }

                        //Unregisted tag will be shown as : ProductName = TAG
                        var unregistedTags = tagNames.Where(x => !_productTagRepository.GetAll().Any(t => t.Name == x));

                        if (unregistedTags.Any())
                        {
                            inventoryDetailList.Add(new InventoryDetailForReportDto
                            {
                                ProductName = "TAG",
                                Total = unregistedTags.Count(),
                                CurrentStock = unregistedTags.Count(),
                                Sold = 0
                            });
                        }
                    }


                }
                else
                {
                    var topup = await _topupRepository.GetAsync(topupId);
                    if (topup == null)
                    {
                        Logger.Error("InventoryItem Detail: Can not found the topup in the system.");
                        throw new UserFriendlyException("Can not found the topup in the system.");
                    }
                    output.TopupDate = topup.StartDate;

                    var inventories = await _inventoryRepository.GetAllIncluding()
                        .Where(x => x.TopupId == topupId && x.MachineId == machineId).Include(x => x.Product)
                        .Include(x => x.Topup)
                        .Include("Product.ProductCategoryRelations.ProductCategory")
                        .GroupBy(x => x.ProductId).ToListAsync();

                    var trans = await _detailTransactRepository.GetAll()
                        .Include(x => x.Inventories).ThenInclude(i => i.Product)
                        .Where(x => x.TopupId == topupId && x.MachineId == machineId && x.Status == Enums.TransactionStatus.Success).ToListAsync();

                    //First , collect all products existing in inventories
                    foreach (var item in inventories)
                    {
                        if (item.Any())
                        {
                            //find all trans with that contain current product
                            var tranInventories = trans.Where(x => x.Inventories.Any(i => i.ProductId == item.Key)).ToList();
                            var product = item.FirstOrDefault().Product;
                            var productTagNames = _productTagRepository.GetAll().Where(x => x.ProductId == item.Key).ToList().Select(x => x.Name).ToList();

                            //Get current stock number from cache
                            var currentStockNumber = machineInventory.Tags.Where(x => productTagNames.Contains(x.Tag)).Count();
                            // currentStockNumber = 99;
                            var soldNumber = tranInventories.Select(x => x.Inventories.Count(i => i.ProductId == product.Id)).Sum(x => x);

                            var cateName = product.ProductCategoryRelations.Select(c => c.ProductCategory.Name).ToList();
                            var cate = string.Join(", ", cateName);
                            var totalItem = item.Count();

                            var inventoryDetail = new InventoryDetailForReportDto
                            {
                                ProductName = product.Name,
                                ProductCategory = cate,
                                ProductSku = product.SKU,
                                ProductDescription = product.Desc,
                                ProductPrice = product.Price,

                                Total = totalItem,//currentStockNumber + soldNumber,
                                CurrentStock = currentStockNumber,
                                Sold = soldNumber,
                                Missing = totalItem - currentStockNumber - soldNumber,

                                LastSoldDate = tranInventories.OrderByDescending(x => x.PaymentTime).FirstOrDefault()?.PaymentTime ?? null
                            };

                            inventoryDetailList.Add(inventoryDetail);
                        }
                    }

                    //Then, collect products exist in cache but not exist in inventory

                    var tagNames = machineInventory.Tags.Select(x => x.Tag).ToList();
                    var currentStockProductIds = _productTagRepository.GetAll().Where(x => tagNames.Contains(x.Name)).Select(x => x.ProductId).Distinct().ToList();
                    var inventoryProductIds = inventories.Select(x => x.Key).ToList();
                    var productIds = currentStockProductIds.Where(x => !inventoryProductIds.Contains(x)).ToList();
                    var products = _productRepository.GetAll()
                        .Include("ProductCategoryRelations.ProductCategory")
                        .Where(x => productIds.Contains(x.Id));

                    foreach (var item in products)
                    {
                        var productTagNames = _productTagRepository.GetAll().Where(x => x.ProductId == item.Id).Select(x => x.Name).ToList();

                        //Get current stock number from cache
                        var currentStockNumber = machineInventory.Tags.Where(x => productTagNames.Contains(x.Tag)).Count();

                        var cateName = item.ProductCategoryRelations.Select(c => c.ProductCategory.Name).ToList();
                        var cate = string.Join(", ", cateName);

                        var inventoryDetail = new InventoryDetailForReportDto
                        {
                            ProductName = item.Name,
                            ProductCategory = cate,
                            ProductSku = item.SKU,
                            ProductDescription = item.Desc,
                            ProductPrice = item.Price,

                            Total = currentStockNumber,
                            CurrentStock = currentStockNumber,
                            Sold = 0,
                        };

                        inventoryDetailList.Add(inventoryDetail);
                    }

                    //Unregisted tag will be shown as : ProductName = TAG
                    var unregistedTags = tagNames.Where(x => !_productTagRepository.GetAll().Any(t => t.Name == x));

                    if (unregistedTags.Any())
                    {
                        inventoryDetailList.Add(new InventoryDetailForReportDto
                        {
                            ProductName = "TAG",
                            Total = unregistedTags.Count(),
                            CurrentStock = unregistedTags.Count(),
                            Sold = 0
                        });
                    }

                }

                output.InventoryDetailList = inventoryDetailList.Where(x => x.Total > 0 || x.Sold > 0 || x.LeftOver > 0).ToList();
                //output total = total current stock + sold number
                output.Total = machineInventory.Tags.Count + output.Sold;

                results.Add(output);





                return results;
            }
            catch (Exception ex)
            {
                Logger.Error("Error", ex);
                throw new UserFriendlyException("Error", ex);
            }
        }

        public async Task<List<InventoryDetailForRepportOutput>> GetInventoryForReport()
        {
            try
            {
                var results = new List<InventoryDetailForRepportOutput>();

                var machines = await _machineRepository.GetAll().ToListAsync();

                //var cache = _cacheManager.GetCache(Common.Const.ProductTagRealtime);

                foreach (var machine in machines)
                {
                    var machineId = machine.Id;

                    var topUp = await _topupRepository.FirstOrDefaultAsync(x => x.MachineId == machine.Id && x.EndDate == null); ;
                    //var machine = await _machineRepository.GetAsync(machineId);
                    var topupId = topUp.Id;

                    if (machine == null)
                    {
                        Logger.Error("Can not found the machine in the system.");
                        throw new UserFriendlyException("Can not found the machine in the system.");
                    }


                    //Get machine inventory from cache
                    var machineInventory = GetMachineInventoryFromCache(machineId);


                    var inventoryDetailList = new List<InventoryDetailForReportDto>();

                    var output = new InventoryDetailForRepportOutput()
                    {
                        MachineName = machine.Name,
                        CurrentStock = machineInventory.Tags.Count
                    };

                    //If the machine has no topup => has no data in inventory, lookup from cache only
                    if (topupId == null || topupId == Guid.Empty)
                    {
                        if (machineInventory.Tags.Count > 0)
                        {
                            output.Total = machineInventory.Tags.Count;
                            //Get all tags from cache => find all products have those tags
                            var tagNames = machineInventory.Tags.Select(x => x.Tag).ToList();
                            var productIds = _productTagRepository.GetAll().Where(x => tagNames.Contains(x.Name)).Select(x => x.ProductId).Distinct().ToList();
                            var products = _productRepository.GetAll()
                                .Include("Inventories.Product.ProductCategoryRelations.ProductCategory")
                                .Where(x => productIds.Contains(x.Id));

                            foreach (var item in products)
                            {
                                var productTagNames = _productTagRepository.GetAll().Where(x => x.ProductId == item.Id).Select(x => x.Name).ToList();

                                //Get current stock number from cache
                                var currentStockNumber = machineInventory.Tags.Where(x => productTagNames.Contains(x.Tag)).Count();

                                var cateName = item.ProductCategoryRelations.Select(c => c.ProductCategory.Name).ToList();
                                var cate = string.Join(", ", cateName);

                                var inventoryDetail = new InventoryDetailForReportDto
                                {
                                    ProductName = item.Name,
                                    ProductCategory = cate,
                                    ProductSku = item.SKU,
                                    ProductDescription = item.Desc,
                                    ProductPrice = item.Price,
                                    Total = currentStockNumber,
                                    CurrentStock = currentStockNumber,
                                    Sold = 0,
                                };

                                inventoryDetailList.Add(inventoryDetail);
                            }

                            //Unregisted tag will be shown as : ProductName = TAG
                            var unregistedTags = tagNames.Where(x => !_productTagRepository.GetAll().Any(t => t.Name == x));

                            if (unregistedTags.Any())
                            {
                                inventoryDetailList.Add(new InventoryDetailForReportDto
                                {
                                    ProductName = "TAG",
                                    Total = unregistedTags.Count(),
                                    CurrentStock = unregistedTags.Count(),
                                    Sold = 0
                                });
                            }
                        }
                    }
                    else
                    {
                        var topup = await _topupRepository.GetAsync(topupId);
                        if (topup == null)
                        {
                            Logger.Error("InventoryItem Detail: Can not found the topup in the system.");
                            throw new UserFriendlyException("Can not found the topup in the system.");
                        }
                        output.TopupDate = topup.StartDate;

                        var inventories = await _inventoryRepository.GetAllIncluding()
                            .Where(x => x.TopupId == topupId && x.MachineId == machineId).Include(x => x.Product)
                            .Include(x => x.Topup)
                            .Include("Product.ProductCategoryRelations.ProductCategory")
                            .GroupBy(x => x.ProductId).ToListAsync();

                        var trans = await _detailTransactRepository.GetAll()
                            .Include(x => x.Inventories).ThenInclude(i => i.Product)
                            .Where(x => x.TopupId == topupId && x.MachineId == machineId && x.Status == Enums.TransactionStatus.Success).ToListAsync();

                        //First , collect all products existing in inventories
                        foreach (var item in inventories)
                        {
                            if (item.Any())
                            {
                                //find all trans with that contain current product
                                var tranInventories = trans.Where(x => x.Inventories.Any(i => i.ProductId == item.Key)).ToList();
                                var product = item.FirstOrDefault().Product;
                                var productTagNames = _productTagRepository.GetAll().Where(x => x.ProductId == item.Key).ToList().Select(x => x.Name).ToList();

                                //Get current stock number from cache
                                var currentStockNumber = machineInventory.Tags.Where(x => productTagNames.Contains(x.Tag)).Count();
                                // currentStockNumber = 99;
                                var soldNumber = tranInventories.Select(x => x.Inventories.Count(i => i.ProductId == product.Id)).Sum(x => x);

                                var cateName = product.ProductCategoryRelations.Select(c => c.ProductCategory.Name).ToList();
                                var cate = string.Join(", ", cateName);
                                var totalItem = item.Count();

                                var inventoryDetail = new InventoryDetailForReportDto
                                {
                                    ProductName = product.Name,
                                    ProductCategory = cate,
                                    ProductSku = product.SKU,
                                    ProductDescription = product.Desc,
                                    ProductPrice = product.Price,

                                    Total = totalItem,//currentStockNumber + soldNumber,
                                    CurrentStock = currentStockNumber,
                                    Sold = soldNumber,
                                    Missing = totalItem - currentStockNumber - soldNumber,

                                    LastSoldDate = tranInventories.OrderByDescending(x => x.PaymentTime).FirstOrDefault()?.PaymentTime ?? null
                                };

                                inventoryDetailList.Add(inventoryDetail);
                            }
                        }

                        //Then, collect products exist in cache but not exist in inventory
                        if (machineInventory.Tags.Count > 0)
                        {
                            var tagNames = machineInventory.Tags.Select(x => x.Tag).ToList();
                            var currentStockProductIds = _productTagRepository.GetAll().Where(x => tagNames.Contains(x.Name)).Select(x => x.ProductId).Distinct().ToList();
                            var inventoryProductIds = inventories.Select(x => x.Key).ToList();
                            var productIds = currentStockProductIds.Where(x => !inventoryProductIds.Contains(x)).ToList();
                            var products = _productRepository.GetAll()
                                .Include("ProductCategoryRelations.ProductCategory")
                                .Where(x => productIds.Contains(x.Id));

                            foreach (var item in products)
                            {
                                var productTagNames = _productTagRepository.GetAll().Where(x => x.ProductId == item.Id).Select(x => x.Name).ToList();

                                //Get current stock number from cache
                                var currentStockNumber = machineInventory.Tags.Where(x => productTagNames.Contains(x.Tag)).Count();

                                var cateName = item.ProductCategoryRelations.Select(c => c.ProductCategory.Name).ToList();
                                var cate = string.Join(", ", cateName);

                                var inventoryDetail = new InventoryDetailForReportDto
                                {
                                    ProductName = item.Name,
                                    ProductCategory = cate,
                                    ProductSku = item.SKU,
                                    ProductDescription = item.Desc,
                                    ProductPrice = item.Price,

                                    Total = currentStockNumber,
                                    CurrentStock = currentStockNumber,
                                    Sold = 0
                                };

                                inventoryDetailList.Add(inventoryDetail);
                            }

                            //Unregisted tag will be shown as : ProductName = TAG
                            var unregistedTags = tagNames.Where(x => !_productTagRepository.GetAll().Any(t => t.Name == x));

                            if (unregistedTags.Any())
                            {
                                inventoryDetailList.Add(new InventoryDetailForReportDto
                                {
                                    ProductName = "TAG",
                                    Total = unregistedTags.Count(),
                                    CurrentStock = unregistedTags.Count(),
                                    Sold = 0
                                });
                            }
                        }
                    }

                    output.InventoryDetailList = inventoryDetailList.Where(x => x.Total > 0 || x.Sold > 0 || x.LeftOver > 0).ToList();
                    //output total = total current stock + sold number
                    output.Total = machineInventory.Tags.Count + output.Sold;

                    results.Add(output);


                }



                return results;
            }
            catch (Exception ex)
            {
                Logger.Error("Error", ex);
                throw new UserFriendlyException("Error", ex);
            }
        }

        public async Task<InventoryDetailOutput> GetMachineInventoryDetailRealTime(Guid machineId, Guid topupId)
        {
            try
            {
                var machine = await _machineRepository.GetAsync(machineId);

                if (machine == null)
                {
                    Logger.Error("Can not found the machine in the system.");
                    throw new UserFriendlyException("Can not found the machine in the system.");
                }

                //Get cache
                var machineInventory = GetMachineInventoryFromCache(machineId);

                var inventoryDetailList = new List<InventoryDetailDto>();

                var output = new InventoryDetailOutput()
                {
                    MachineName = machine.Name,
                    CurrentStock = machineInventory.Tags.Count
                };
                var tagList = machineInventory.Tags.Select(x => x.Tag).ToList();
                //If the machine has no topup => has no data in inventory, lookup from cache only
                if (topupId == null || topupId == Guid.Empty)
                {
                    if (machineInventory.Tags.Count > 0)
                    {
                        output.Total = machineInventory.Tags.Count;
                        //Get all tags from cache => find all products have those tags


                        var productTags = await _productTagRepository.GetAllIncluding(el => el.Product)
                                            .Where(el => tagList.Contains(el.Name))
                                            .Select(el => new
                                            {
                                                ProductName = el.Product != null ? el.Product.Name : "",
                                                Tag = el.Name
                                            }


                                            ).ToListAsync();
                        productTags.GroupBy(el => el.ProductName).ToList().ForEach(el =>
                        {

                            var inventoryDetail = new InventoryDetailDto
                            {
                                ProductName = el.Key,
                                Total = el.Count(),
                                CurrentStock = el.Count(),
                                Sold = 0,
                                Type = "C1"
                            };

                            inventoryDetailList.Add(inventoryDetail);
                        });
                        //Unregisted tag will be shown as : ProductName = TAG
                        var unregistedTags = tagList.Where(x => !productTags.Any(pt => pt.Tag == x));

                        if (unregistedTags.Any())
                        {
                            inventoryDetailList.Add(new InventoryDetailDto
                            {
                                ProductName = "TAG",
                                Total = unregistedTags.Count(),
                                CurrentStock = unregistedTags.Count(),
                                Sold = 0
                            });
                        }

                    }
                }
                else
                {
                    var topup = await _topupRepository.GetAll().Where(el => el.Id == topupId).FirstOrDefaultAsync();
                    if (topup == null)
                    {
                        Logger.Error("InventoryItem Detail: Can not found the topup in the system.");
                        throw new UserFriendlyException("Can not found the topup in the system.");
                    }
                    output.Total = topup.Total;
                    output.TopupDate = topup.StartDate;

                    var currentInventory = _inventoryRepository.GetAllIncluding(el => el.Product, el => el.Transaction)
                                            .Where(el => el.TopupId == topup.Id && el.State!= Enums.TagState.Unloaded)
                                            .Select(el => new { ProductName = el.Product.Name, el.ProductId, Tag = el.TagId, el.DetailTransactionId, el.Transaction });

                    var inventoryGroupedByProductName = currentInventory
                                                            .GroupBy(el => el.ProductName);

                    inventoryGroupedByProductName.ToList().ForEach(el =>
                    {
                        var inventoryDetail = new InventoryDetailDto
                        {
                            ProductName = el.Key,
                            Total = el.Count(),//currentStockNumber + soldNumber,
                            CurrentStock = el.Count(t => tagList.Contains(t.Tag)),
                            Sold = el.Count(c => c.DetailTransactionId.HasValue),
                            LastSoldDate = el.Where(t => t.Transaction != null).OrderByDescending(p => p.Transaction.PaymentTime).FirstOrDefault()?.Transaction?.PaymentTime,
                            Type = "I1"
                        };

                        inventoryDetailList.Add(inventoryDetail);
                    });

                    //Unregisted tag will be shown as : ProductName = TAG
                    var unregistedTags = tagList.Where(x => !currentInventory.Any(el => el.Tag == x));

                    if (unregistedTags.Any())
                    {
                        inventoryDetailList.Add(new InventoryDetailDto
                        {
                            ProductName = "TAG",
                            Total = unregistedTags.Count(),
                            CurrentStock = unregistedTags.Count(),
                            Sold = 0
                        });
                    }
                }


                output.InventoryDetailList = inventoryDetailList;
                //output total = total current stock + sold number

                output.Missing = output.InventoryDetailList.Sum(x => x.Missing);
                output.LastUpdated = machineInventory.LastUpdated;

                return output;
            }
            catch (Exception ex)
            {
                Logger.Error("Error", ex);
                throw new UserFriendlyException("Error", ex);
            }
        }

        [AbpAllowAnonymous]
        public async Task<bool> UnloadItems(SyncUnloadDto input)
        {
            try
            {
                Guid newTopupId = Guid.Empty;
                KonbiCloud.Restock.RestockSession rs = null;

                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
                    {
                        int? tenantId = null;
                        //Check machine exist
                        var mc = await _machineRepository.FirstOrDefaultAsync(x => x.Id == input.MachineId);
                        if (mc == null)
                        {
                            _detailLogService.Log("Can not find the machine with id = " + input.MachineId);
                            return false;
                        }


                        rs = await _restockSessionRepository.GetAll().OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                        if (rs == null)
                        {
                            _detailLogService.Log("Can not find any restock session ");
                            //return false;
                        }
                        else
                        {
                            rs.Unloaded = input.Ids.Count();
                        }

                        tenantId = mc.TenantId;
                        _detailLogService.Log("Restock from machine : " + input.MachineId + " and tenant: " + tenantId);

                        _detailLogService.Log("Restocker name : " + input.RestockerName);

                        //Get all unloaded inventories from request
                        var unloadInventories = await _inventoryRepository.GetAllListAsync(x => input.Ids.Any(id => id == x.Id));

                        //Close all topups
                        var unclosedTopups = await _topupRepository.GetAll().Where(x =>
                                x.MachineId == input.MachineId && x.EndDate == null).OrderByDescending(x => x.CreationTime).ToListAsync();

                        foreach (var unclosedTopup in unclosedTopups)
                        {
                            unclosedTopup.EndDate = Clock.Now;
                        }

                        var newTopup = await _topupRepository.InsertAsync(new Topup
                        {
                            Id = input.NewTopup.Id,
                            StartDate = input.NewTopup.StartDate,
                            IsInprogress = input.NewTopup.IsProcessing,
                            Total = input.NewTopup.Total,
                            RestockerName = input.RestockerName,
                            Type = Enums.TopupTypeEnum.Unload,
                            MachineId = input.MachineId,
                            TenantId = tenantId,
                            RestockSession = rs
                        });

                        _detailLogService.Log("New topup: " + newTopup);

                        newTopupId = newTopup.Id;

                        //Move unsold inventories to new topup
                        var oldTopupId = unclosedTopups.Any() ? unclosedTopups[0].Id : Guid.Empty;
                        _detailLogService.Log("Old topup id = : " + oldTopupId);

                        var oldInventories = await _inventoryRepository.GetAllListAsync(x => x.DetailTransactionId == null && x.TopupId == oldTopupId);
                        _detailLogService.Log("Old inventories: " + oldInventories);

                        foreach (var oldInventory in oldInventories)
                        {
                            if (unloadInventories.Any(x => x.TagId.Equals(oldInventory.TagId)))
                            {
                                oldInventory.State = Enums.TagState.Unloaded;
                            }
                            else
                            {
                                oldInventory.State = Enums.TagState.Stocked;
                            }

                            _detailLogService.Log("Move inventories: " + oldInventory.Id + " from : " + oldTopupId + " to new topup : " + newTopupId);

                            oldInventory.Topup = newTopup;

                            //Insert old product into TopupInventory
                            await _topupHistoryRepository.InsertAsync(new TopupHistory
                            {
                                TopupId = newTopupId,
                                InventoryId = oldInventory.Id,
                                TenantId = tenantId
                            });

                        }

                        await CurrentUnitOfWork.SaveChangesAsync();
                        await unitOfWork.CompleteAsync();
                    }
                }

                //Update total
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
                    {
                        var nt = await _topupRepository.FirstOrDefaultAsync(x => x.Id == newTopupId);
                        if (nt != null)
                        {
                            var totalItem = await _inventoryRepository.CountAsync(x => x.DetailTransactionId == null && x.TopupId == newTopupId && x.State != Enums.TagState.Unloaded);
                            nt.Total = totalItem;
                            await unitOfWork.CompleteAsync();
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error", ex);
                return false;
            }
        }
        [AbpAllowAnonymous]
        public async Task<bool> Restock(RestockInput input)
        {
            try
            {
                _detailLogService.Log($"Restock, data:\n\r {JsonConvert.SerializeObject(input)}");
                
                if (input.Inventory == null || input.Inventory.Count <= 0)
                {
                    _detailLogService.Log($"Cannot restock because submitted inventory is invalid");
                    _detailLogService.Log($"Restock data: {JsonConvert.SerializeObject(input)}");
                    return false;
                }
                Guid newTopupId = Guid.Empty;
                CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant);


                int? tenantId = null;
                //Check machine exist
                var mc = await _machineRepository.FirstOrDefaultAsync(x => x.Id == input.MachineId);
                if (mc == null)
                {
                    var msg = $"Can not find the machine with id = {input.MachineId}";
                    _detailLogService.Log($"{msg}, data: " + JsonConvert.SerializeObject(input));
                    throw new UserFriendlyException(msg);                  
                }

                tenantId = mc.TenantId;
                _detailLogService.Log($"Restock from machine : {mc.Name}, Data:{JsonConvert.SerializeObject(input)}");

                //Close all topups
                var unclosedTopups = await _topupRepository.GetAll().Where(x =>
                        x.MachineId == input.MachineId && x.EndDate == null).ToListAsync();

                foreach (var unclosedTopup in unclosedTopups)
                {
                    unclosedTopup.EndDate = Clock.Now;
                }
                var previousTopupSession = unclosedTopups.OrderByDescending(el => el.StartDate).FirstOrDefault();

                // find the topup session in cloud
                var topupSession = await _topupRepository.GetAllIncluding(el => el.Inventory, el => el.History).Where(el => el.Id == input.Id).FirstOrDefaultAsync();
                var isnewTopup = false;

                // calculate total 
                var total = input.Inventory.Count(el => el.State != Enums.TagState.Unloaded);

                if (topupSession == null)
                {
                    isnewTopup = true;
                    topupSession = new Topup()
                    {
                        Id = input.Id,
                        Total = input.Total > 0 ? input.Total : total,
                        MachineId = input.MachineId,
                        TenantId = mc.TenantId,
                        StartDate = input.StartDate.HasValue? input.StartDate.Value: Clock.Now,
                        RestockerName = input.RestockerName,
                        PreviousTopupId = previousTopupSession?.Id,
                        Inventory = new List<InventoryItem>(),
                        History = new List<TopupHistory>()
                    };

                }
                else
                {
                    topupSession.Total = input.Total > 0 ? input.Total : total;
                }

                // determine topup type by submitted inventory
                var stockedCount = input.Inventory.Count(el => el.State == Enums.TagState.Stocked);
                var unloadedCount = input.Inventory.Count(el => el.State == Enums.TagState.Unloaded);
                if (stockedCount > 0 && unloadedCount > 0)
                    topupSession.Type = Enums.TopupTypeEnum.UnloadAndRestock;
                else if (stockedCount > 0)
                    topupSession.Type = Enums.TopupTypeEnum.Restock;
                else if (unloadedCount > 0)
                    topupSession.Type = Enums.TopupTypeEnum.Unload;

                if (topupSession.History.Count > 0)
                    topupSession.History.Clear();
                if (topupSession.Inventory.Count > 0)
                    topupSession.Inventory.Clear();
                // store topup history: leftover tags (state = Current), just restocked = Restocked, Unload tags with state = Unloaded.

                var productsInSubmmitedInventory = await _productRepository.GetAll().Where(el => input.Inventory.Select(i => i.ProductId).Contains(el.Id)).ToListAsync();


                //Move unsold inventories to new topup

                for(var i =0; i< input.Inventory.Count; i++)
                {
                    var inventoryItem = input.Inventory[i];
               

                    var updatingInventoryItem = await _inventoryRepository.GetAll().Where(el => el.Id == inventoryItem.Id).FirstOrDefaultAsync();
                    if (updatingInventoryItem == null)
                    {
                        updatingInventoryItem = new InventoryItem()
                        {
                            Id = inventoryItem.Id,
                            MachineId = mc.Id,
                            Price = inventoryItem.Price,
                            ProductId = inventoryItem.ProductId,
                            State = inventoryItem.State,
                            TagId = inventoryItem.TagId,
                            TopupId = null,
                            TenantId = mc.TenantId,
                            TrayLevel = inventoryItem.TrayLevel

                        };


                    }
                    else
                    {
                        updatingInventoryItem.TopupId = null;
                        updatingInventoryItem.State = inventoryItem.State;

                    }
                    topupSession.Inventory.Add(updatingInventoryItem);

                    var historyRecord = new TopupHistory()
                    {
                        Tag = inventoryItem.TagId,
                        ProductName = productsInSubmmitedInventory.FirstOrDefault(p => p.Id == inventoryItem.ProductId)?.Name,
                        Price = inventoryItem.Price,
                        TenantId = mc.TenantId,
                        InventoryId = inventoryItem.Id
                    };
                    if (inventoryItem.State == Enums.TagState.Active)
                        historyRecord.Type = Enums.TopupHistoryType.Current;
                    else if (inventoryItem.State == Enums.TagState.Stocked)
                        historyRecord.Type = Enums.TopupHistoryType.Stocked;
                    else if (inventoryItem.State == Enums.TagState.Unloaded)
                        historyRecord.Type = Enums.TopupHistoryType.Unloaded;

                    topupSession.History.Add(historyRecord);

                    //}
                    if (inventoryItem.State == Enums.TagState.Stocked)
                    {
                        //Update producttag
                        var pt = await _productTagRepository.FirstOrDefaultAsync(x => x.Name.Equals(inventoryItem.TagId) && x.State != Enums.ProductTagStateEnum.Stocked);
                        if (pt != null)
                        {
                            pt.State = Enums.ProductTagStateEnum.Stocked;
                            await _productTagRepository.UpdateAsync(pt);
                        }

                    }
                }
                if (isnewTopup)
                {
                    await _topupRepository.InsertAsync(topupSession);
                }
                else
                {
                    await _topupRepository.UpdateAsync(topupSession);
                }
                await CurrentUnitOfWork.SaveChangesAsync();


                _detailLogService.Log("Complete Restocking");
                return true;
            }
            catch (Exception e)
            {
                _detailLogService.Log("Restock Error: " + e);
                Logger.Error(e.Message, e);
                throw new UserFriendlyException(e.Message);
              
            }

        }

        [AbpAllowAnonymous]
        public async Task<bool> SyncRestock(List<InventoryItem> newInventories, Guid machineId)
        {
            try
            {
                Guid newTopupId = Guid.Empty;
                KonbiCloud.Restock.RestockSession rs = null;
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
                    {
                        int? tenantId = null;
                        //Check machine exist
                        var mc = await _machineRepository.FirstOrDefaultAsync(x => x.Id == machineId);
                        if (mc == null)
                        {
                            _detailLogService.Log("Can not find the machine with id = " + machineId);
                            return false;
                        }

                        rs = await _restockSessionRepository.GetAll().OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                        if (rs == null)
                        {
                            _detailLogService.Log("Can not find any restock session " + machineId);
                            // return false;
                        }
                        else
                        {
                            _detailLogService.Log("Foudn a restock session ID: " + rs.Id);
                            rs.Restocked = newInventories.Count;
                        }


                        tenantId = mc.TenantId;
                        _detailLogService.Log("Restock from machine : " + machineId + " and tenant: " + tenantId);

                        //Close all topups
                        var unclosedTopups = await _topupRepository.GetAll().Where(x =>
                                x.MachineId == machineId && x.EndDate == null).OrderByDescending(x => x.CreationTime).ToListAsync();

                        foreach (var unclosedTopup in unclosedTopups)
                        {
                            unclosedTopup.EndDate = Clock.Now;//
                        }

                        newInventories[0].Topup.MachineId = machineId;
                        newInventories[0].Topup.Total = 0;
                        newInventories[0].Topup.TenantId = tenantId;

                        var newTopup = await _topupRepository.InsertAsync(newInventories[0].Topup);

                        _detailLogService.Log("New topup: " + newInventories[0].Topup);

                        newTopupId = newTopup.Id;

                        //Move unsold inventories to new topup
                        var oldTopupId = unclosedTopups.Any() ? unclosedTopups[0].Id : Guid.Empty;
                        var oldInventories = await _inventoryRepository.GetAllListAsync(x => x.DetailTransactionId == null && x.TopupId == oldTopupId);
                        foreach (var oldInventory in oldInventories)
                        {
                            if (newInventories.Any(x => x.TagId.Equals(oldInventory.TagId)))
                            {
                                oldInventory.State = Enums.TagState.Removed;
                            }
                            else
                            {
                                oldInventory.State = Enums.TagState.Stocked;
                                oldInventory.Topup = newTopup;

                                //Insert old product into TopupInventory
                                await _topupHistoryRepository.InsertAsync(new TopupHistory
                                {
                                    TopupId = newTopupId,
                                    InventoryId = oldInventory.Id,
                                    TenantId = tenantId
                                });
                            }
                        }

                        //Insert new inventories
                        var existInventories = await _inventoryRepository.GetAllListAsync(x => x.MachineId == mc.Id && x.TopupId == newTopup.Id);
                        foreach (var inventory in newInventories)
                        {
                            //Check to skip is old invetory item
                            if (oldInventories.Any(x => x.TagId.Equals(inventory.TagId))) continue;

                            if (await _productRepository.CountAsync(x => x.Id == inventory.ProductId) <= 0) continue;
                            if (existInventories.Any(x => x.Id == inventory.Id)) continue;

                            //Insert new product into TopupInventory
                            await _topupHistoryRepository.InsertAsync(new TopupHistory
                            {
                                TopupId = newTopupId,
                                InventoryId = inventory.Id,
                                TenantId = tenantId
                            });

                            inventory.MachineId = mc.Id;
                            inventory.TenantId = tenantId;
                            await _inventoryRepository.InsertAsync(inventory);

                            //Update producttag
                            var pt = await _productTagRepository.FirstOrDefaultAsync(x => x.Name.Equals(inventory.TagId));
                            pt.State = Enums.ProductTagStateEnum.Stocked;
                            await _productTagRepository.UpdateAsync(pt);
                        }

                        await CurrentUnitOfWork.SaveChangesAsync();

                        await unitOfWork.CompleteAsync();
                    }
                }


                //Update total
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
                    {
                        var nt = await _topupRepository.FirstOrDefaultAsync(x => x.Id == newTopupId);
                        if (nt != null)
                        {
                            var totalItem = await _inventoryRepository.CountAsync(x => x.DetailTransactionId == null && x.TopupId == newTopupId);
                            nt.Total = totalItem;
                            if (rs != null)
                            {
                                rs.Total = totalItem;
                            }
                            rs.Total = totalItem;
                            nt.RestockSession = rs;
                            _detailLogService.Log("Update total items for topup : " + newTopupId + ",total items = " + totalItem);
                        }
                        await unitOfWork.CompleteAsync();
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                _detailLogService.Log("Process Restock: " + e);
                return false;
            }
        }
        public bool RequestMachineToUpdateInventory(Guid machineId)
        {
            return _sendMessageToMachineService.SendQueuedMsgToMachines(new KeyValueMessage
            {
                MachineId = machineId,
                Key = MessageKeys.SyncInventoriesToCloud
            }, CloudToMachineType.ToMachineId);
        }
        /// <summary>
        /// Get Machine inventory from cache or load from db if it has not been loaded to cache yet
        /// </summary>
        /// <param name="machineId"></param>
        /// <returns></returns>
        private MachineInventoryCacheDto GetMachineInventoryFromCache(Guid machineId)
        {
            var cache = _cacheManager.GetCache(Common.Const.ProductTagRealtime);
            return cache.Get(machineId.ToString(), () =>
            {
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