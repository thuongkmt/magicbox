

using KonbiCloud.Products;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using KonbiCloud.Inventories.Exporting;
using KonbiCloud.Inventories.Dtos;
using KonbiCloud.Dto;
using Abp.Application.Services.Dto;
using KonbiCloud.Authorization;
using Abp.Authorization;
using Abp.Configuration;
using Abp.UI;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Microsoft.EntityFrameworkCore;


namespace KonbiCloud.Inventories
{
    using Abp.Extensions;
    using KonbiBrain.Common;
    using KonbiCloud.Products.Dtos;
    using KonbiCloud.Restock;
    using Konbini.Messages.Services;
    using Newtonsoft.Json;
    using ServiceStack;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using GetAllForLookupTableInput = GetAllForLookupTableInput;

    [AbpAuthorize(AppPermissions.Pages_Inventories)]
    public class InventoriesAppService : KonbiCloudAppServiceBase, IInventoriesAppService
    {
        private readonly IRepository<InventoryItem, Guid> _inventoryRepository;
        private readonly IRepository<RestockSession, int> _restockSessionRepository;

        private readonly IInventoriesExcelExporter _inventoriesExcelExporter;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly ISendMessageToCloudService _sendMessageToCloudService;
        private readonly IRepository<Topup, Guid> _topupRepository;
        private readonly Guid _machineId;
        private readonly int _tenantId;
        private readonly bool _useCloud;
        private readonly bool _removeTagAfterSold;
        private string _cloudUrl;
        private bool _useRabbitMqToSync;
        private readonly IDetailLogService detailLogService;

        public InventoriesAppService(IRepository<InventoryItem, Guid> inventoryRepository,
                                     IInventoriesExcelExporter inventoriesExcelExporter,
                                     IRepository<Product, Guid> productRepository,
                                     IRepository<Topup, Guid> topupRepository,
                                     ISendMessageToCloudService sendMessageToCloudService,
                                     ISettingManager settingManager,
                                     IDetailLogService detailLog,
                                     IRepository<RestockSession, int> restockSessionRepository
                                     )
        {
            _inventoryRepository = inventoryRepository;
            _inventoriesExcelExporter = inventoriesExcelExporter;
            _productRepository = productRepository;
            this._topupRepository = topupRepository;
            _sendMessageToCloudService = sendMessageToCloudService;
            detailLogService = detailLog;
            _restockSessionRepository = restockSessionRepository;

            _machineId = Guid.Parse(settingManager.GetSettingValue(AppSettingNames.MachineId));
            _tenantId = int.Parse(settingManager.GetSettingValue(AppSettingNames.TenantId));

            bool.TryParse(settingManager.GetSettingValue(AppSettingNames.UseCloud), out _useCloud);
            bool.TryParse(settingManager.GetSettingValue(AppSettingNames.RemoveTagAfterSold), out _removeTagAfterSold);

            _cloudUrl = settingManager.GetSettingValue(AppSettingNames.CloudApiUrl);
            bool.TryParse(settingManager.GetSettingValue("RfidFridgeSetting.System.Cloud.SyncUseRabbitMq"), out _useRabbitMqToSync);

            if (!_cloudUrl.EndsWith("/"))
            {
                _cloudUrl += "/";
            }
        }

        [AbpAllowAnonymous]
        public async Task<KonbiCloud.Inventories.Dtos.TopupDto> NewTopup()
        {
            //close previous topup
            var unClosedTopups = await _topupRepository.GetAll().Where(x => !x.EndDate.HasValue).ToListAsync();
            foreach (var unClosedTopup in unClosedTopups)
            {
                unClosedTopup.EndDate = DateTime.Now;
                unClosedTopup.IsProcessing = false;
                await _topupRepository.UpdateAsync(unClosedTopup);
            }

            var topup = new Topup()
            {
                StartDate = DateTime.Now,
                IsProcessing = true
            };
            await _topupRepository.InsertAsync(topup);

            //move old inventory to new topup
            var oldInventories =
                await _inventoryRepository.GetAll().Where(x => x.DetailTransactionId == null).ToListAsync();
            var updatedList = new List<InventoryItem>();
            foreach (var oldInventory in oldInventories)
            {
                oldInventory.State = Enums.TagState.Stocked;
                oldInventory.Topup = topup;
                updatedList.Add(await _inventoryRepository.UpdateAsync(oldInventory));
            }


            await CurrentUnitOfWork.SaveChangesAsync();

            // TrungPQ: Check use Cloud.
            if (_useCloud)
            {
                _sendMessageToCloudService.SendQueuedMsgToCloud(new KeyValueMessage()
                {
                    Key = MessageKeys.UpdateInventoryList,
                    MachineId = _machineId,
                    Value = updatedList
                });
            }

            return ObjectMapper.Map<KonbiCloud.Inventories.Dtos.TopupDto>(topup);
        }


        /// <summary>
        /// Administrator click on "End Toppup" button
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        [AbpAllowAnonymous]
        public async Task<KonbiCloud.Inventories.Dtos.TopupDto> EndTopup(int total)
        {
            var totalItem = this._inventoryRepository.GetAll().Where(x => x.Transaction == null).Count();
            var currentTopup = await _topupRepository.GetAll().Where(x => !x.EndDate.HasValue)
                .OrderByDescending(x => x.StartDate).FirstOrDefaultAsync();
            if (currentTopup == null)
                throw new UserFriendlyException("Can not find current top-up, please try to register new top-up.");

            currentTopup.Total = totalItem;
            currentTopup.IsProcessing = false;
            await _topupRepository.UpdateAsync(currentTopup);

            // TrungPQ: Check use Cloud.
            if (_useCloud)
            {
                _sendMessageToCloudService.SendQueuedMsgToCloud(new KeyValueMessage()
                {
                    Key = MessageKeys.Topup,
                    MachineId = _machineId,
                    Value = currentTopup,
                });
            }
            return ObjectMapper.Map<KonbiCloud.Inventories.Dtos.TopupDto>(currentTopup);
        }

        public async Task<TopupDto> GetCurrentTopup()
        {
            var currentTopup = await _topupRepository.GetAll().Where(x => !x.EndDate.HasValue)
                .OrderByDescending(x => x.StartDate).FirstOrDefaultAsync();
            if (currentTopup == null)
                throw new UserFriendlyException("Can not find current top-up, please try to register new top-up.");

            return ObjectMapper.Map<TopupDto>(currentTopup);
        }

        [AbpAllowAnonymous]
        public async Task<List<GetInventoryForWebApiDto>> GetAllItems()
        {
            var filteredInventories = _inventoryRepository.GetAll();
            if (_removeTagAfterSold)
            {
                filteredInventories = filteredInventories.Where(x => x.DetailTransactionId == 0 || x.DetailTransactionId == null);
            }

            filteredInventories = filteredInventories.Where(x => x.State != Enums.TagState.Unloaded);

            var query = (from o in filteredInventories
                         join o1 in _productRepository.GetAll() on o.ProductId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()
                         select new GetInventoryForWebApiDto()
                         {
                             Inventory = ObjectMapper.Map<InventoryDto>(o),
                             Product = new ProductDto() { Name = s1.Name, Price = s1.Price, SKU = s1.SKU }
                         });

            var inventories = await query.ToListAsync();
            Logger.Info($"GetAllItems: {inventories.Count} items");

            return inventories;
        }

        [AbpAllowAnonymous]
        public async Task UnmapAll()
        {
            var items = this._inventoryRepository.GetAll();
            foreach (var item in items)
            {
                await _inventoryRepository.DeleteAsync(item);
            }
        }

        public async Task<PagedResultDto<GetInventoryForViewDto>> GetAll(GetAllInventoriesInput input)
        {
            try
            {
                var filteredInventories = _inventoryRepository.GetAllIncluding(x => x.Product)
                        .WhereIf(true, x => x.TopupId.HasValue)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.TagId.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.TagIdFilter), e => e.TagId.ToLower() == input.TagIdFilter.ToLower().Trim())
                        .WhereIf(input.MinTrayLevelFilter != null, e => e.TrayLevel >= input.MinTrayLevelFilter)
                        .WhereIf(input.MaxTrayLevelFilter != null, e => e.TrayLevel <= input.MaxTrayLevelFilter)
                        .WhereIf(input.MinPriceFilter != null, e => e.Price >= input.MinPriceFilter)
                        .WhereIf(input.MaxPriceFilter != null, e => e.Price <= input.MaxPriceFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ProductNameFilter), e => e.Product == null ||
                                                                                      e.Product.Name.ToLower().Equals(input.ProductNameFilter.ToLower().Trim()));
                var totalCount = await filteredInventories.CountAsync();
                var inventories = await filteredInventories.OrderBy(input.Sorting ?? "product.name asc")
                            .PageBy(input)
                            .Select(x => new GetInventoryForViewDto()
                            {
                                Inventory = ObjectMapper.Map<InventoryDto>(x),
                                ProductName = x.Product == null ? "" : x.Product.Name,
                                IsSold = x.DetailTransactionId > 0
                            }).ToListAsync();

                return new PagedResultDto<GetInventoryForViewDto>(
                    totalCount,
                    inventories
                );
            }
            catch (Exception ex)
            {
                Logger.Error("Error Get all inventories", ex);
                throw new UserFriendlyException("Cannot get inventories", ex);
            }

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

        [AbpAllowAnonymous]
        public async Task Topup(List<CreateOrEditInventoryDto> items)
        {
            foreach (var item in items)
            {
                await Create(item);
            }
        }

        [AbpAllowAnonymous]
        public async Task UnloadItems(UnloadInputDto input)
        {
            try
            {

                var jsInput = JsonConvert.SerializeObject(input);

                this.detailLogService.Log("UnloadInputDto DTO: " + jsInput);


                var unloadInventories = await _inventoryRepository.GetAllListAsync(x => input.Ids.Any(id => id == x.Id));

                //Close previous topup & Create new topup item
                var unClosedTopups = await _topupRepository.GetAll().Where(x => !x.EndDate.HasValue).OrderByDescending(x => x.CreationTime).ToListAsync();
                foreach (var unClosedTopup in unClosedTopups)
                {
                    unClosedTopup.EndDate = DateTime.Now;
                    unClosedTopup.IsProcessing = false;
                }

                // Get Restock session
                var restockSession = _restockSessionRepository.FirstOrDefault(x => x.Id == input.RestockSessionId);
                if (restockSession == null)
                {
                    detailLogService.Log("restockSession is null");
                }

                var newTopup = new Topup()
                {
                    StartDate = DateTime.Now,
                    IsProcessing = true,
                    Type = Enums.TopupTypeEnum.Unload,
                    RestockSession = restockSession
                };

                //Move old inventory to new topup
                var oldTopupId = unClosedTopups.Any() ? unClosedTopups[0].Id : Guid.Empty;
                var oldInventories = await _inventoryRepository.GetAllListAsync(x => x.DetailTransactionId == null && x.TopupId == oldTopupId);
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

                    oldInventory.Topup = newTopup;
                }

                await CurrentUnitOfWork.SaveChangesAsync();

                //Calculate total
                var totalItem = await _inventoryRepository.CountAsync(x => x.DetailTransactionId == null && x.TopupId == newTopup.Id && x.State != Enums.TagState.Unloaded);
                newTopup.Total = totalItem;
                newTopup.IsProcessing = false;
                //restockSession.Total = totalItem;


                await CurrentUnitOfWork.SaveChangesAsync();

                //Sync to Cloud
                if (_useCloud)
                {
                    var data = new SyncUnloadDto()
                    {
                        Ids = input.Ids,
                        RestockerName = input.RestockerName,
                        MachineId = _machineId,
                        NewTopup = new UnloadTopupDto
                        {
                            Id = newTopup.Id,
                            StartDate = newTopup.StartDate,
                            Total = newTopup.Total,
                            IsProcessing = newTopup.IsProcessing
                        }
                    };

                    this.detailLogService.Log("Sync unload items using WebApi");

                    var url = $"{_cloudUrl}api/services/app/Inventories/UnloadItems";
                    this.detailLogService.Log("Unload inventories SYNC Url: " + url);

                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                        {
                            request.Headers.TryAddWithoutValidation("accept", "text/plain");
                            // request.Headers.TryAddWithoutValidation("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJBc3BOZXQuSWRlbnRpdHkuU2VjdXJpdHlTdGFtcCI6IjJlOWQ3N2FmLTQwM2MtYzlkNS0zM2E0LTM5ZjMyMDU5Nzc3NiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiaHR0cDovL3d3dy5hc3BuZXRib2lsZXJwbGF0ZS5jb20vaWRlbnRpdHkvY2xhaW1zL3RlbmFudElkIjoiMSIsInN1YiI6IjIiLCJqdGkiOiI4MDQ2YzIxNy00ZDExLTQ2NmMtYjRmNS03NTI3MGQxZjdmYjQiLCJpYXQiOjE1ODQ5Mzg3MjUsInRva2VuX3ZhbGlkaXR5X2tleSI6ImZmMGFjNzlkLTVhNzMtNDM1ZC1iMjMwLWNjNjUwYzlmOGNkMyIsInVzZXJfaWRlbnRpZmllciI6IjJAMSIsIm5iZiI6MTU4NDkzODcyNSwiZXhwIjoxNTg1MDI1MTI1LCJpc3MiOiJLb25iaUNsb3VkIiwiYXVkIjoiS29uYmlDbG91ZCJ9.NBgrVNIg8rWCoBTOYoLywglOp0vh_Lw_j0c06COwfOU");

                            var unloadObjectJson = JsonConvert.SerializeObject(data);
                            this.detailLogService.Log("Unload SYNC JSON: " + unloadObjectJson);

                            request.Content = new StringContent(unloadObjectJson);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json-patch+json");

                            var response = await httpClient.SendAsync(request);
                            this.detailLogService.Log("Unload SYNC response: " + response);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error", ex);
                throw new UserFriendlyException("Cannot unload items", ex);
            }
        }

        [AbpAllowAnonymous]
        public async Task UnloadItemsByTagId(UnloadByTagIdInputDto input)
        {
            try
            {

                var jsInput = JsonConvert.SerializeObject(input);

                this.detailLogService.Log("UnloadInputDto DTO: " + jsInput);


                var unloadInventories = await _inventoryRepository.GetAllListAsync(x => input.Ids.Any(id => id == x.TagId));

                this.detailLogService.Log("unloadInventories DTO: " + JsonConvert.SerializeObject(unloadInventories));

                if (unloadInventories != null)
                {
                    this.detailLogService.Log("unloadInventories count: " + JsonConvert.SerializeObject(unloadInventories));
                }
                else
                {
                    throw new UserFriendlyException("Can't find any inventories to unload");
                }


                //Close previous topup & Create new topup item
                var unClosedTopups = await _topupRepository.GetAll().Where(x => !x.EndDate.HasValue).OrderByDescending(x => x.CreationTime).ToListAsync();
                foreach (var unClosedTopup in unClosedTopups)
                {
                    unClosedTopup.EndDate = DateTime.Now;
                    unClosedTopup.IsProcessing = false;
                }

                // Get Restock session
                var restockSession = _restockSessionRepository.FirstOrDefault(x => x.Id == input.RestockSessionId);
                if (restockSession == null)
                {
                    detailLogService.Log("restockSession is null");
                }

                var newTopup = new Topup()
                {
                    StartDate = DateTime.Now,
                    IsProcessing = true,
                    Type = Enums.TopupTypeEnum.Unload,
                    RestockSession = restockSession
                };

                //Move old inventory to new topup
                var oldTopupId = unClosedTopups.Any() ? unClosedTopups[0].Id : Guid.Empty;
                var oldInventories = await _inventoryRepository.GetAllListAsync(x => x.DetailTransactionId == null && x.TopupId == oldTopupId);
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

                    oldInventory.Topup = newTopup;
                }

                await CurrentUnitOfWork.SaveChangesAsync();

                //Calculate total
                var totalItem = await _inventoryRepository.CountAsync(x => x.DetailTransactionId == null && x.TopupId == newTopup.Id && x.State != Enums.TagState.Unloaded);
                newTopup.Total = totalItem;
                newTopup.IsProcessing = false;
                //restockSession.Total = totalItem;


                await CurrentUnitOfWork.SaveChangesAsync();

                //Sync to Cloud
                if (_useCloud)
                {
                    var ids = unloadInventories.Select(x => x.Id).ToList();

                    var data = new SyncUnloadDto()
                    {
                        Ids = ids,
                        RestockerName = input.RestockerName,
                        MachineId = _machineId,
                        NewTopup = new UnloadTopupDto
                        {
                            Id = newTopup.Id,
                            StartDate = newTopup.StartDate,
                            Total = newTopup.Total,
                            IsProcessing = newTopup.IsProcessing
                        }
                    };

                    this.detailLogService.Log("Sync unload items using WebApi");

                    var url = $"{_cloudUrl}api/services/app/Inventories/UnloadItems";
                    this.detailLogService.Log("Unload inventories SYNC Url: " + url);

                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                        {
                            request.Headers.TryAddWithoutValidation("accept", "text/plain");
                            // request.Headers.TryAddWithoutValidation("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJBc3BOZXQuSWRlbnRpdHkuU2VjdXJpdHlTdGFtcCI6IjJlOWQ3N2FmLTQwM2MtYzlkNS0zM2E0LTM5ZjMyMDU5Nzc3NiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiaHR0cDovL3d3dy5hc3BuZXRib2lsZXJwbGF0ZS5jb20vaWRlbnRpdHkvY2xhaW1zL3RlbmFudElkIjoiMSIsInN1YiI6IjIiLCJqdGkiOiI4MDQ2YzIxNy00ZDExLTQ2NmMtYjRmNS03NTI3MGQxZjdmYjQiLCJpYXQiOjE1ODQ5Mzg3MjUsInRva2VuX3ZhbGlkaXR5X2tleSI6ImZmMGFjNzlkLTVhNzMtNDM1ZC1iMjMwLWNjNjUwYzlmOGNkMyIsInVzZXJfaWRlbnRpZmllciI6IjJAMSIsIm5iZiI6MTU4NDkzODcyNSwiZXhwIjoxNTg1MDI1MTI1LCJpc3MiOiJLb25iaUNsb3VkIiwiYXVkIjoiS29uYmlDbG91ZCJ9.NBgrVNIg8rWCoBTOYoLywglOp0vh_Lw_j0c06COwfOU");

                            var unloadObjectJson = JsonConvert.SerializeObject(data);
                            this.detailLogService.Log("Unload SYNC JSON: " + unloadObjectJson);

                            request.Content = new StringContent(unloadObjectJson);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json-patch+json");

                            var response = await httpClient.SendAsync(request);
                            this.detailLogService.Log("Unload SYNC response: " + response);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error", ex);
                throw new UserFriendlyException("Cannot unload items", ex);
            }
        }

        [AbpAllowAnonymous]
        public async Task<List<InventoryItem>> UnloadItemsByTagIdV2(UnloadByTagIdInputDto input)
        {
            try
            {
                var returnData = new List<InventoryItem>();
                var jsInput = JsonConvert.SerializeObject(input);

                this.detailLogService.Log("UnloadInputDto DTO: " + jsInput);


                //Close previous topup & Create new topup item
                var unClosedTopups = await _topupRepository.GetAll().Where(x => !x.EndDate.HasValue).OrderByDescending(x => x.CreationTime).ToListAsync();
                foreach (var unClosedTopup in unClosedTopups)
                {
                    unClosedTopup.EndDate = DateTime.Now;
                    unClosedTopup.IsProcessing = false;
                }
                var oldTopupId = unClosedTopups.Any() ? unClosedTopups[0].Id : Guid.Empty;

                this.detailLogService.Log("Unload oldTopupId: " + oldTopupId);

                var unloadInventories = await _inventoryRepository.GetAllListAsync(x => input.Ids.Any(id => id == x.TagId) && x.TopupId == oldTopupId);

                this.detailLogService.Log("unloadInventories DTO: " + JsonConvert.SerializeObject(unloadInventories));

                if (unloadInventories != null)
                {
                    this.detailLogService.Log("unloadInventories count: " + unloadInventories.Count);
                }
                else
                {
                    throw new UserFriendlyException("Can't find any inventories to unload");
                }

                var newTopup = new Topup()
                {
                    StartDate = DateTime.Now,
                    IsProcessing = true,
                    Type = Enums.TopupTypeEnum.Unload,
                };

                var topup = await _topupRepository.InsertAsync(newTopup);

                //Move old inventory to new topup
                var oldInventories = await _inventoryRepository.GetAllListAsync(x => x.DetailTransactionId == null && x.TopupId == oldTopupId && x.State != Enums.TagState.Unloaded);

                this.detailLogService.Log("unloadInventories oldInventories: " + JsonConvert.SerializeObject(oldInventories));

                foreach (var oldInventory in oldInventories)
                {
                    this.detailLogService.Log($"TAG ID {oldInventory.TagId} STATE {oldInventory.State}");

                    if (unloadInventories.Any(x => x.TagId.Equals(oldInventory.TagId)))
                    {
                        oldInventory.State = Enums.TagState.Unloaded;
                        returnData.Add(oldInventory);

                        this.detailLogService.Log($"UNLOAD - Set  {oldInventory.TagId} to UNLOAD");
                    }
                    else
                    {
                        //oldInventory.State = Enums.TagState.Stocked;

                    }
                    oldInventory.Topup = topup;
                }

                await CurrentUnitOfWork.SaveChangesAsync();

                //returnData.AddRange(unloadInventories);
                return returnData;
                ////Calculate total
                //var totalItem = await _inventoryRepository.CountAsync(x => x.DetailTransactionId == null && x.TopupId == newTopup.Id && x.State != Enums.TagState.Unloaded);
                //newTopup.Total = totalItem;
                //newTopup.IsProcessing = false;
                ////restockSession.Total = totalItem;


                //await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Logger.Error("Error", ex);
                throw new UserFriendlyException("Cannot unload items", ex);
            }
        }

        //[AbpAuthorize(AppPermissions.Pages_Inventories_Restock)]
        [AbpAllowAnonymous]
        public async Task Restock(List<CreateOrEditInventoryDto> items)
        {
            try
            {
                if (items != null)
                {
                    Logger.Info("Restocking : " + JsonConvert.SerializeObject(items));

                }
                else
                {
                    Logger.Info("Restocking items is null");

                }

                //Close previous topup & Create new topup item
                var unClosedTopups = await _topupRepository.GetAll().Where(x => !x.EndDate.HasValue).OrderByDescending(x => x.CreationTime).ToListAsync();
                foreach (var unClosedTopup in unClosedTopups)
                {
                    unClosedTopup.EndDate = DateTime.Now;
                    unClosedTopup.IsProcessing = false;
                }

                var newTopup = new Topup()
                {
                    StartDate = DateTime.Now,
                    IsProcessing = true,
                    Type = Enums.TopupTypeEnum.Restock
                };

                //Move old inventory to new topup
                var oldTopupId = unClosedTopups.Any() ? unClosedTopups[0].Id : Guid.Empty;
                var oldInventories = await _inventoryRepository.GetAllListAsync(x => x.DetailTransactionId == null && x.TopupId == oldTopupId);
                foreach (var oldInventory in oldInventories)
                {
                    if (items.Any(x => x.TagId.Equals(oldInventory.TagId)))
                    {
                        oldInventory.State = Enums.TagState.Removed;
                    }
                    else
                    {
                        oldInventory.State = Enums.TagState.Stocked;
                        oldInventory.Topup = newTopup;
                    }
                }

                //Add new inventories
                var newInventories = new List<InventoryItem>();
                foreach (var item in items)
                {
                    //Check to skip is old invetory item
                    if (oldInventories.Any(x => x.TagId.Equals(item.TagId))) continue;

                    var newInventory = ObjectMapper.Map<InventoryItem>(item);
                    newInventory.State = Enums.TagState.Stocked;
                    newInventory.Topup = newTopup;
                    await _inventoryRepository.InsertAsync(newInventory);
                    newInventories.Add(newInventory);
                }
                await CurrentUnitOfWork.SaveChangesAsync();

                //Calculate total
                var totalItem = await _inventoryRepository.CountAsync(x => x.DetailTransactionId == null && x.TopupId == newTopup.Id);
                newTopup.Total = totalItem;
                newTopup.IsProcessing = false;

                //SynctoCloud
                if (_useCloud)
                {
                    if (_useRabbitMqToSync)
                    {
                        this.detailLogService.Log("Sync restock inventories using RabbitMQ");
                        if (_sendMessageToCloudService.SendQueuedMsgToCloud(new KeyValueMessage()
                        {
                            MachineId = _machineId,
                            Key = MessageKeys.Restock,
                            Value = newInventories
                        }))
                        {
                            foreach (var inventory in newInventories)
                            {
                                var i = await _inventoryRepository.FirstOrDefaultAsync(x => x.Id == inventory.Id);
                                if (i != null)
                                {
                                    i.MarkSync();
                                }
                            }
                        }
                    }
                    else
                    {
                        this.detailLogService.Log("Sync restock inventories using Web API");

                        var url = $"{_cloudUrl}api/services/app/Inventories/SyncRestock?machineId={_machineId}";
                        this.detailLogService.Log("Restock SYNC Url: " + url);

                        using (var httpClient = new HttpClient())
                        {
                            using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                            {
                                request.Headers.TryAddWithoutValidation("accept", "text/plain");
                                // request.Headers.TryAddWithoutValidation("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJBc3BOZXQuSWRlbnRpdHkuU2VjdXJpdHlTdGFtcCI6IjJlOWQ3N2FmLTQwM2MtYzlkNS0zM2E0LTM5ZjMyMDU5Nzc3NiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiaHR0cDovL3d3dy5hc3BuZXRib2lsZXJwbGF0ZS5jb20vaWRlbnRpdHkvY2xhaW1zL3RlbmFudElkIjoiMSIsInN1YiI6IjIiLCJqdGkiOiI4MDQ2YzIxNy00ZDExLTQ2NmMtYjRmNS03NTI3MGQxZjdmYjQiLCJpYXQiOjE1ODQ5Mzg3MjUsInRva2VuX3ZhbGlkaXR5X2tleSI6ImZmMGFjNzlkLTVhNzMtNDM1ZC1iMjMwLWNjNjUwYzlmOGNkMyIsInVzZXJfaWRlbnRpZmllciI6IjJAMSIsIm5iZiI6MTU4NDkzODcyNSwiZXhwIjoxNTg1MDI1MTI1LCJpc3MiOiJLb25iaUNsb3VkIiwiYXVkIjoiS29uYmlDbG91ZCJ9.NBgrVNIg8rWCoBTOYoLywglOp0vh_Lw_j0c06COwfOU");

                                var newInventoriesJson = JsonConvert.SerializeObject(newInventories);
                                this.detailLogService.Log("Inventories SYNC JSON: " + newInventoriesJson);

                                request.Content = new StringContent(newInventoriesJson);
                                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json-patch+json");

                                var response = await httpClient.SendAsync(request);
                                this.detailLogService.Log("Transaction SYNC response: " + response);
                            }
                        }
                    }

                    this.detailLogService.Log("Add restock inventories to cloud, machine id = " + _machineId);
                }

                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.Error("Error", ex);
                throw new UserFriendlyException("Cannot register new items", ex);
            }
        }

        [AbpAllowAnonymous]
        public async Task RestockWithRestockerName(RestockerInputDto input)
        {
            try
            {
                if (input.Items != null)
                {
                    detailLogService.Log("Restocking : " + JsonConvert.SerializeObject(input));

                }
                else
                {
                    detailLogService.Log("Restocking items is null");

                }
                //Close previous topup & Create new topup item
                var unClosedTopups = await _topupRepository.GetAll().Where(x => !x.EndDate.HasValue).OrderByDescending(x => x.CreationTime).ToListAsync();
                foreach (var unClosedTopup in unClosedTopups)
                {
                    unClosedTopup.EndDate = DateTime.Now;
                    unClosedTopup.IsProcessing = false;
                }

                // Get Restock session
                var restockSession = _restockSessionRepository.GetAll().FirstOrDefault(x => x.Id == input.RestockSessionId);
                if (restockSession == null)
                {
                    detailLogService.Log("restockSession is null");
                }
                var newTopup = new Topup()
                {
                    StartDate = DateTime.Now,
                    IsProcessing = true,
                    RestockerName = input.RestockerName,
                    Type = Enums.TopupTypeEnum.Restock,
                    //RestockSession = restockSession
                };

                //Move old inventory to new topup
                var oldTopupId = unClosedTopups.Any() ? unClosedTopups[0].Id : Guid.Empty;
                var oldInventories = await _inventoryRepository.GetAllListAsync(x => x.DetailTransactionId == null && x.TopupId == oldTopupId);
                foreach (var oldInventory in oldInventories)
                {
                    if (input.Items.Any(x => x.TagId.Equals(oldInventory.TagId)))
                    {
                        oldInventory.State = Enums.TagState.Removed;
                    }
                    else
                    {
                        oldInventory.State = Enums.TagState.Stocked;
                        oldInventory.Topup = newTopup;
                    }
                }

                //Add new inventories
                var newInventories = new List<InventoryItem>();
                foreach (var item in input.Items)
                {
                    //Check to skip is old invetory item
                    if (oldInventories.Any(x => x.TagId.Equals(item.TagId))) continue;

                    var newInventory = ObjectMapper.Map<InventoryItem>(item);
                    newInventory.State = Enums.TagState.Stocked;
                    newInventory.Topup = newTopup;
                    await _inventoryRepository.InsertAsync(newInventory);
                    newInventories.Add(newInventory);
                }
                await CurrentUnitOfWork.SaveChangesAsync();

                //Calculate total
                var totalItem = await _inventoryRepository.CountAsync(x => x.DetailTransactionId == null && x.TopupId == newTopup.Id);
                newTopup.Total = totalItem;
                newTopup.IsProcessing = false;
                // restockSession.Total = totalItem;

                //SynctoCloud
                if (_useCloud)
                {
                    if (_useRabbitMqToSync)
                    {
                        this.detailLogService.Log("Sync restock inventories using RabbitMQ");
                        if (_sendMessageToCloudService.SendQueuedMsgToCloud(new KeyValueMessage()
                        {
                            MachineId = _machineId,
                            Key = MessageKeys.Restock,
                            Value = newInventories
                        }))
                        {
                            foreach (var inventory in newInventories)
                            {
                                var i = await _inventoryRepository.FirstOrDefaultAsync(x => x.Id == inventory.Id);
                                if (i != null)
                                {
                                    i.MarkSync();
                                }
                            }
                        }
                    }
                    else
                    {
                        this.detailLogService.Log("Sync restock inventories using Web API");

                        var url = $"{_cloudUrl}api/services/app/Inventories/SyncRestock?machineId={_machineId}";
                        this.detailLogService.Log("Restock SYNC Url: " + url);

                        using (var httpClient = new HttpClient())
                        {
                            using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                            {
                                request.Headers.TryAddWithoutValidation("accept", "text/plain");
                                // request.Headers.TryAddWithoutValidation("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJBc3BOZXQuSWRlbnRpdHkuU2VjdXJpdHlTdGFtcCI6IjJlOWQ3N2FmLTQwM2MtYzlkNS0zM2E0LTM5ZjMyMDU5Nzc3NiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiaHR0cDovL3d3dy5hc3BuZXRib2lsZXJwbGF0ZS5jb20vaWRlbnRpdHkvY2xhaW1zL3RlbmFudElkIjoiMSIsInN1YiI6IjIiLCJqdGkiOiI4MDQ2YzIxNy00ZDExLTQ2NmMtYjRmNS03NTI3MGQxZjdmYjQiLCJpYXQiOjE1ODQ5Mzg3MjUsInRva2VuX3ZhbGlkaXR5X2tleSI6ImZmMGFjNzlkLTVhNzMtNDM1ZC1iMjMwLWNjNjUwYzlmOGNkMyIsInVzZXJfaWRlbnRpZmllciI6IjJAMSIsIm5iZiI6MTU4NDkzODcyNSwiZXhwIjoxNTg1MDI1MTI1LCJpc3MiOiJLb25iaUNsb3VkIiwiYXVkIjoiS29uYmlDbG91ZCJ9.NBgrVNIg8rWCoBTOYoLywglOp0vh_Lw_j0c06COwfOU");

                                var newInventoriesJson = JsonConvert.SerializeObject(newInventories);
                                this.detailLogService.Log("Inventories SYNC JSON: " + newInventoriesJson);

                                request.Content = new StringContent(newInventoriesJson);
                                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json-patch+json");

                                var response = await httpClient.SendAsync(request);
                                this.detailLogService.Log("Transaction SYNC response: " + response);
                            }
                        }
                    }

                    this.detailLogService.Log("Add restock inventories to cloud, machine id = " + _machineId);
                }

                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                detailLogService.Log(ex.ToString());
                var errmess = ex.ToString();
                if (errmess.Contains("Microsoft.EntityFrameworkCore.DbUpdateException"))
                {
                    throw new UserFriendlyException("Product is not synced", ex);
                }
                else
                {
                    throw new UserFriendlyException(ex.Message, ex);
                }
            }
        }

        [AbpAllowAnonymous]
        public async Task<List<InventoryItem>> RestockWithRestockerNameV2(RestockerInputDto input)
        {
            try
            {
                var returnData = new List<InventoryItem>();

                if (input.Items != null)
                {
                    detailLogService.Log("Restocking : " + JsonConvert.SerializeObject(input));

                }
                else
                {
                    detailLogService.Log("Restocking items is null");

                }
                //Close previous topup & Create new topup item
                var unClosedTopups = await _topupRepository.GetAll().Where(x => !x.EndDate.HasValue).OrderByDescending(x => x.CreationTime).ToListAsync();
                foreach (var unClosedTopup in unClosedTopups)
                {
                    unClosedTopup.EndDate = DateTime.Now;
                    unClosedTopup.IsProcessing = false;
                }

                var newTopup = new Topup()
                {
                    StartDate = DateTime.Now,
                    IsProcessing = true,
                    RestockerName = input.RestockerName,
                    Type = Enums.TopupTypeEnum.Restock,
                };

                var topup = await _topupRepository.InsertAsync(newTopup);

                var newInventories = new List<InventoryItem>();

                //Move old inventory to new topup
                var oldTopupId = unClosedTopups.Any() ? unClosedTopups[0].Id : Guid.Empty;
                var oldInventories = await _inventoryRepository.GetAllListAsync(x => x.DetailTransactionId == null && x.TopupId == oldTopupId && x.State != Enums.TagState.Unloaded);
                detailLogService.Log("Restocking oldInventories: " + JsonConvert.SerializeObject(oldInventories));

                var oldInvenTagId = string.Empty;
                foreach (var oldInventory in oldInventories)
                {
                    oldInventory.State = Enums.TagState.Stocked;
                    oldInventory.Topup = topup;
                    oldInvenTagId += oldInventory.TagId + " | ";

                    //if (input.Items.Any(x => x.TagId.Equals(oldInventory.TagId)))
                    //{
                    //    newInventories.Add(oldInventory);
                    //}
                }
                detailLogService.Log("oldInvenTagId: " + oldInvenTagId);


                var newInvenTagId = string.Empty;

                //Add new inventories
                foreach (var item in input.Items)
                {
                    //Check to skip is old invetory item
                    if (oldInventories.Any(x => x.TagId.Equals(item.TagId)))
                    {
                        detailLogService.Log("Skip item: " + item.TagId);
                        continue;
                    };

                    var newInventory = ObjectMapper.Map<InventoryItem>(item);
                    newInventory.State = Enums.TagState.Stocked;
                    newInventory.Topup = topup;
                    await _inventoryRepository.InsertAsync(newInventory);
                    newInventories.Add(newInventory);
                    newInvenTagId += newInventory.TagId + " | ";
                }
                detailLogService.Log("newInvenTagId: " + oldInvenTagId);

                await CurrentUnitOfWork.SaveChangesAsync();

                returnData.AddRange(newInventories);

                return returnData;
                ////Calculate total
                //var totalItem = await _inventoryRepository.CountAsync(x => x.DetailTransactionId == null && x.TopupId == newTopup.Id);
                //newTopup.Total = totalItem;
                //newTopup.IsProcessing = false;


                //await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                detailLogService.Log(ex.ToString());
                var errmess = ex.ToString();
                if (errmess.Contains("Microsoft.EntityFrameworkCore.DbUpdateException"))
                {
                    throw new UserFriendlyException("Product is not synced", ex);
                }
                else
                {
                    throw new UserFriendlyException(ex.Message, ex);
                }
            }
        }


        [AbpAllowAnonymous]
        public async Task RestockWithSession_Old(RestockWithSessionInputDto input)
        {
            try
            {
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(input.StartDate);
                var dtDateTime = dateTimeOffset.UtcDateTime.ToLocalTime();
                var entity = new RestockSession
                {
                    StartDate = dtDateTime,
                    RestockerName = input.RestockerName
                };

                var session = _restockSessionRepository.Insert(entity);
                CurrentUnitOfWork.SaveChanges();


                await CreateCloudRestockSession(entity);



                if (session == null)
                {
                    throw new UserFriendlyException("Can't not find Restock Session");
                }

                var numberOfRestock = input.Restock.Items.Count;
                var numberOfUnload = input.Unload.Ids.Count;

                if (numberOfRestock > 0)
                {
                    detailLogService.Log($"Restocking {numberOfRestock} items...");
                    await RestockWithRestockerName(input.Restock);
                }
                if (numberOfUnload > 0)
                {
                    detailLogService.Log($"Unloading {numberOfUnload} items...");
                    await UnloadItemsByTagId(input.Unload);
                }
                var currentTopup = await GetCurrentTopupInfo();

                detailLogService.Log($"currentTopup | Sold: {currentTopup.Sold} | Total {currentTopup.Total}");

                session.Total = currentTopup.Total;
                session.Sold = currentTopup.Sold;
                session.Unloaded = numberOfUnload;
                session.Restocked = numberOfRestock;

                session.EndDate = DateTime.Now;
                detailLogService.Log($"Restock/Unload done.");

                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                detailLogService.Log(ex.ToString());
                var errmess = ex.ToString();
                throw new UserFriendlyException(ex.Message, ex);
            }
        }


        [AbpAllowAnonymous]
        public async Task RestockWithSession(RestockWithSessionInputDto input)
        {
            try
            {
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(input.StartDate);
                var dtDateTime = dateTimeOffset.UtcDateTime.ToLocalTime();

                var numberOfRestock = input.Restock.Items.Count;
                var numberOfUnload = input.Unload.Ids.Count;
                var itemsActive = new List<InventoryItem>();
                var itemsRestock = new List<InventoryItem>();
                var itemsUnload = new List<InventoryItem>();

                var currentTopup = await _topupRepository.GetAll()
                                            .Where(x => !x.EndDate.HasValue)
                                            .FirstOrDefaultAsync();

                if (currentTopup == null)
                {
                    currentTopup = await _topupRepository.GetAll()
                                         .OrderByDescending(x => x.LastModificationTime)
                                            .FirstOrDefaultAsync();
                    detailLogService.Log($"Can't find restock by enddate, find by LastModificationTime");

                }
                if (currentTopup == null)
                {
                    throw new Exception("Can't find current topup session");
                }

                detailLogService.Log($"Current Topup ID: {currentTopup.Id}");

                itemsActive = await _inventoryRepository.GetAll().Where(x => x.DetailTransactionId == null && x.TopupId == currentTopup.Id && x.State != Enums.TagState.Unloaded).ToListAsync();
                detailLogService.Log($"Total active After: {itemsActive.Count} items...");
                detailLogService.Log($"itemsActive: {JsonConvert.SerializeObject(itemsActive)}");

                if (numberOfRestock > 0)
                {
                    detailLogService.Log($"Restocking {numberOfRestock} items...");
                    itemsRestock = await RestockWithRestockerNameV2(input.Restock);
                }
                if (numberOfUnload > 0)
                {
                    detailLogService.Log($"Unloading {numberOfUnload} items...");
                    itemsUnload = await UnloadItemsByTagIdV2(input.Unload);
                    //itemsActive.AddRange(itemsRestock);
                    var removedItems = itemsActive.RemoveAll(x => itemsUnload.Any(t => t.TagId == x.TagId));
                }

                currentTopup = await _topupRepository.GetAll()
                                          .Where(x => !x.EndDate.HasValue)
                                          .FirstOrDefaultAsync();

                detailLogService.Log($"Current Topup ID: {currentTopup.Id}");
                detailLogService.Log($"Total active Before: {itemsActive.Count} items...");

                if (_useCloud)
                {
                    if (numberOfRestock == 0 && numberOfUnload == 0)
                    {
                        detailLogService.Log($"There is no item to Restock/Unload");
                    }
                    else
                    {
                        var cloudDto = new RestockV2Dto();
                        var active = itemsActive.Select(x => new Inventory()
                        {
                            id = x.Id.ToString(),
                            price = x.Price,
                            productId = x.ProductId.ToString(),
                            state = Enums.TagState.Active,
                            tagId = x.TagId,
                            trayLevel = 0
                        });
                        cloudDto.inventory.AddRange(active);

                        var restock = itemsRestock.Select(x => new Inventory()
                        {
                            id = x.Id.ToString(),
                            price = x.Price,
                            productId = x.ProductId.ToString(),
                            state = Enums.TagState.Stocked,
                            tagId = x.TagId,
                            trayLevel = 0
                        });
                        cloudDto.inventory.AddRange(restock);

                        var unload = itemsUnload.Select(x => new Inventory()
                        {
                            id = x.Id.ToString(),
                            price = x.Price,
                            productId = x.ProductId.ToString(),
                            state = Enums.TagState.Unloaded,
                            tagId = x.TagId,
                            trayLevel = 0
                        });
                        cloudDto.inventory.AddRange(unload);
                        cloudDto.id = currentTopup.Id.ToString();
                        cloudDto.machineId = _machineId.ToString();
                        cloudDto.restockerName = input.RestockerName;
                        cloudDto.startDate = dtDateTime;

                        this.detailLogService.Log("Sync RestockV2 WebApi");

                        var url = $"{_cloudUrl}api/services/app/Inventories/Restock";
                        this.detailLogService.Log("RestockV2 SYNC Url: " + url);

                        using (var httpClient = new HttpClient())
                        {
                            using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                            {
                                request.Headers.TryAddWithoutValidation("accept", "text/plain");
                                // request.Headers.TryAddWithoutValidation("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJBc3BOZXQuSWRlbnRpdHkuU2VjdXJpdHlTdGFtcCI6IjJlOWQ3N2FmLTQwM2MtYzlkNS0zM2E0LTM5ZjMyMDU5Nzc3NiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiaHR0cDovL3d3dy5hc3BuZXRib2lsZXJwbGF0ZS5jb20vaWRlbnRpdHkvY2xhaW1zL3RlbmFudElkIjoiMSIsInN1YiI6IjIiLCJqdGkiOiI4MDQ2YzIxNy00ZDExLTQ2NmMtYjRmNS03NTI3MGQxZjdmYjQiLCJpYXQiOjE1ODQ5Mzg3MjUsInRva2VuX3ZhbGlkaXR5X2tleSI6ImZmMGFjNzlkLTVhNzMtNDM1ZC1iMjMwLWNjNjUwYzlmOGNkMyIsInVzZXJfaWRlbnRpZmllciI6IjJAMSIsIm5iZiI6MTU4NDkzODcyNSwiZXhwIjoxNTg1MDI1MTI1LCJpc3MiOiJLb25iaUNsb3VkIiwiYXVkIjoiS29uYmlDbG91ZCJ9.NBgrVNIg8rWCoBTOYoLywglOp0vh_Lw_j0c06COwfOU");

                                var unloadObjectJson = JsonConvert.SerializeObject(cloudDto);
                                this.detailLogService.Log("RestockV2 SYNC JSON: " + unloadObjectJson);

                                request.Content = new StringContent(unloadObjectJson);
                                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json-patch+json");

                                var response = await httpClient.SendAsync(request);
                                this.detailLogService.Log("RestockV2 SYNC response: " + response);

                                if (response.StatusCode != HttpStatusCode.OK)
                                {
                                    var data = response.Content.ReadAsStringAsync().Result;
                                    this.detailLogService.Log("RestockV2 SYNC response JSON: " + data);
                                    dynamic r = JsonConvert.DeserializeObject(data);

                                    throw new Exception("Restock Error!");
                                }

                            }
                        }
                    }
                }

                detailLogService.Log($"Restock/Unload done.");

                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                detailLogService.Log(ex.ToString());
                var errmess = ex.ToString();
                throw new UserFriendlyException(ex.Message, ex);
            }
        }

        public async Task CreateCloudRestockSession(RestockSession input)
        {
            try
            {
                this.detailLogService.Log("CreateCloudRestockSession");

                var url = $"{_cloudUrl}api/services/app/RestockSessions/CreateAndGetId?machineId={_machineId}&tenantId={_tenantId}";
                this.detailLogService.Log("CreateCloudRestockSession Url: " + url);

                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                    {
                        request.Headers.TryAddWithoutValidation("accept", "text/plain");
                        // request.Headers.TryAddWithoutValidation("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJBc3BOZXQuSWRlbnRpdHkuU2VjdXJpdHlTdGFtcCI6IjJlOWQ3N2FmLTQwM2MtYzlkNS0zM2E0LTM5ZjMyMDU5Nzc3NiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiaHR0cDovL3d3dy5hc3BuZXRib2lsZXJwbGF0ZS5jb20vaWRlbnRpdHkvY2xhaW1zL3RlbmFudElkIjoiMSIsInN1YiI6IjIiLCJqdGkiOiI4MDQ2YzIxNy00ZDExLTQ2NmMtYjRmNS03NTI3MGQxZjdmYjQiLCJpYXQiOjE1ODQ5Mzg3MjUsInRva2VuX3ZhbGlkaXR5X2tleSI6ImZmMGFjNzlkLTVhNzMtNDM1ZC1iMjMwLWNjNjUwYzlmOGNkMyIsInVzZXJfaWRlbnRpZmllciI6IjJAMSIsIm5iZiI6MTU4NDkzODcyNSwiZXhwIjoxNTg1MDI1MTI1LCJpc3MiOiJLb25iaUNsb3VkIiwiYXVkIjoiS29uYmlDbG91ZCJ9.NBgrVNIg8rWCoBTOYoLywglOp0vh_Lw_j0c06COwfOU");

                        var newInventoriesJson = JsonConvert.SerializeObject(input);
                        this.detailLogService.Log("CreateCloudRestockSession JSON: " + newInventoriesJson);

                        request.Content = new StringContent(newInventoriesJson);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json-patch+json");

                        var response = await httpClient.SendAsync(request);
                        this.detailLogService.Log("CreateCloudRestockSession response: " + response);
                    }
                }
            }
            catch (Exception ex)
            {
                this.detailLogService.Log("CreateCloudRestockSession error: " + ex.ToString());

            }
        }

        [AbpAllowAnonymous]
        public async Task<int> StartNewRestockSession(NewRestockSessionInputDto newRestockSessionInputDto)
        {
            try
            {
                var entity = new RestockSession
                {
                    StartDate = DateTime.Now,
                    RestockerName = newRestockSessionInputDto.RestockerName
                };

                var id = await _restockSessionRepository.InsertAndGetIdAsync(entity);
                await CurrentUnitOfWork.SaveChangesAsync();
                return id;
            }
            catch (Exception ex)
            {
                detailLogService.Log(ex.ToString());
                var errmess = ex.ToString();
                throw new UserFriendlyException(ex.Message, ex);
            }
        }



        [AbpAuthorize(AppPermissions.Pages_Inventories_Create)]
        private async Task Create(CreateOrEditInventoryDto input)
        {
            var inventory = ObjectMapper.Map<InventoryItem>(input);
            inventory.State = Enums.TagState.Stocked;//stocked

            if (AbpSession.TenantId != null)
            {
                inventory.TenantId = AbpSession.TenantId;
            }

            var currentTopup = await _topupRepository.GetAll().Where(x => !x.EndDate.HasValue)
                .OrderByDescending(x => x.StartDate).FirstOrDefaultAsync();
            if (currentTopup == null)
            {
                currentTopup = new Topup { StartDate = DateTime.Now };
                await this._topupRepository.InsertAsync(currentTopup);
            }
            inventory.Topup = currentTopup;

            await _inventoryRepository.InsertAsync(inventory);
            await CurrentUnitOfWork.SaveChangesAsync();

            // TrungPQ: Check use Cloud.
            if (_useCloud)
            {
                if (_sendMessageToCloudService.SendQueuedMsgToCloud(new KeyValueMessage()
                {
                    MachineId = _machineId,
                    Key = MessageKeys.Inventory,
                    Value = inventory
                }))
                {
                    inventory = inventory.MarkSync();
                }
            }
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

        public RestockSession GetCurrentRestockSession()
        {
            try
            {
                var currentTopup = GetCurrentTopupInfo().Result;


                if (currentTopup != null)
                {
                    return _restockSessionRepository.FirstOrDefault(x => x.Id == currentTopup.RestockSessionId);
                }

                return new RestockSession();
            }
            catch (Exception ex)
            {
                Logger.Error("Error", ex);
                throw new UserFriendlyException("Error", ex);
            }
        }

        public async Task<GetCurrentTopupDto> GetCurrentTopupInfo()
        {
            try
            {
                var currentTopup = await _topupRepository.GetAll()
                .Where(x => !x.EndDate.HasValue)
                .FirstOrDefaultAsync();

                if (currentTopup != null)
                {
                    var inventories = await _inventoryRepository.GetAll()
                        .Where(x => x.TopupId == currentTopup.Id)
                        .ToListAsync();

                    if (inventories.Any())
                    {
                        return new GetCurrentTopupDto()
                        {
                            StartTime = currentTopup.StartDate,
                            Total = inventories.Count,
                            Sold = inventories.Where(x => x.DetailTransactionId > 0).Count(),
                            RestockSessionId = currentTopup.RestockSession != null ? currentTopup.RestockSession.Id : 0
                        };
                    }
                }

                return new GetCurrentTopupDto();
            }
            catch (Exception ex)
            {
                Logger.Error("Error", ex);
                throw new UserFriendlyException("Error", ex);
            }
        }


        [AbpAllowAnonymous]
        public async Task<List<ProductInfo>> QueryProductByTag(TagsInput input)
        {
            var pInfoResult = new List<ProductInfo>();
            try
            {
                var cloudUrl = SettingManager.GetSettingValue(AppSettingNames.CloudApiUrl);
                var content = await Task.Run(() => JsonConvert.SerializeObject(input));
                var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                using (var httpClient = new HttpClient())
                using (var httpResponse = await httpClient.PostAsync($"{cloudUrl}/api/services/app/ProductTags/QueryProductByTag", httpContent))
                {
                    if (httpResponse.Content != null && httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var responseContent = await httpResponse.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<RestApiGenericResult<ProductInfo>>(responseContent);
                        pInfoResult = result.result;
                    }
                }

                return pInfoResult;
            }
            catch (Exception ex)
            {
                Logger.Error($"Get Product By Tag {ex.Message}", ex);
                return pInfoResult;
            }
        }

        public void SyncInventoriesToCloud()
        {
            var topUp = _topupRepository.FirstOrDefault(x => x.EndDate == null);
            if (topUp == null)
            {
                Logger.Error("Can not found any active topup");
                return;
            }

            var currentInventories = _inventoryRepository.GetAll().Where(x => x.TopupId == topUp.Id);

            if (_useCloud)
            {
                _sendMessageToCloudService.SendQueuedMsgToCloud(new KeyValueMessage()
                {
                    Key = MessageKeys.SyncInventoriesForActiveTopup,
                    MachineId = _machineId,
                    Value = currentInventories
                });
            }
        }
    }
}