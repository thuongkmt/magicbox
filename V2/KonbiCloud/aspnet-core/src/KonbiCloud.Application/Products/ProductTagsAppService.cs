using Abp.Application.Services.Dto;
using KonbiCloud.Authorization;
using Abp.Authorization;
using Abp.Domain.Repositories;
using System;
using System.Threading.Tasks;
using KonbiCloud.Products.Dtos;
using Abp.Collections.Extensions;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Abp.Domain.Uow;
using KonbiCloud.Machines;
using KonbiCloud.Enums;
using Abp.UI;
using Abp.Timing;

namespace KonbiCloud.Products
{
    [AbpAuthorize(AppPermissions.Pages_ProductTags)]
    public class ProductTagsAppService : KonbiCloudAppServiceBase, IProductTagsAppService
    {
        private readonly IRepository<ProductTag, Guid> _productTagRepository;
        private readonly IRepository<Product, Guid> _productRepository;

        private readonly IRepository<ProductMachinePrice> _productMachinePriceRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public ProductTagsAppService(IRepository<ProductTag, Guid> productTagRepository,
                                     IRepository<ProductMachinePrice> productMachinePriceRepository,
                                     IRepository<Machine, Guid> machineRepository,
                                     IRepository<Product, Guid> productRepository,
                                     IUnitOfWorkManager unitOfWorkManager)
        {
            _productTagRepository = productTagRepository;
            _productMachinePriceRepository = productMachinePriceRepository;
            _machineRepository = machineRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _productRepository = productRepository;
        }

        public async Task<PagedResultDto<GetProductTagForViewDto>> GetAll(GetAllProductTagsInput input)
        {

            var filter = _productTagRepository.GetAllIncluding(x => x.Product)
                                              .WhereIf(!string.IsNullOrEmpty(input.TagFilter), e => e.Name != null && e.Name.Contains(input.TagFilter.ToLower().Trim()))
                                              .WhereIf(!string.IsNullOrEmpty(input.ProductFilter), e => e.Product.Name != null && e.Product.Name.Contains(input.ProductFilter.ToLower().Trim()))
                                              .WhereIf(input.StateFilter != null, e => (int)e.State == input.StateFilter)
                                              .WhereIf(input.FromDateFilter.HasValue, e => e.CreationTime >= input.FromDateFilter)
                                              .WhereIf(input.ToDateFilter.HasValue, e => e.CreationTime < input.ToDateFilter.Value.AddDays(1));

            var query = (from o in filter
                         select new GetProductTagForViewDto
                         {
                             ProductTag = ObjectMapper.Map<ProductTagDto>(o)
                         });

            var totalCount = query.ToList().Count;
            //var totalCount = await query.CountAsync();
            List<GetProductTagForViewDto> productTags = new List<GetProductTagForViewDto>();

            if (input.Sorting != null)
            {
                productTags = await query.OrderBy(input.Sorting)
                                         .PageBy(input)
                                         .ToListAsync();
            }
            else
            {
                productTags = await query.OrderByDescending(x => x.ProductTag.CreationTime)
                                         .PageBy(input)
                                         .ToListAsync();
            }

            return new PagedResultDto<GetProductTagForViewDto>(
                totalCount,
                productTags
            );
        }

        public async Task<PagedResultDto<ProductTagForReportDto>> GetAllForReport(GetAllProductTagsInput input)
        {

            var filter = _productTagRepository.GetAllIncluding(x => x.Product)
                                              .Include("Product.ProductCategoryRelations.ProductCategory")
                                              .WhereIf(!string.IsNullOrEmpty(input.TagFilter), e => e.Name != null && e.Name.Contains(input.TagFilter.ToLower().Trim()))
                                              .WhereIf(!string.IsNullOrEmpty(input.ProductFilter), e => e.Product.Name != null && e.Product.Name.Contains(input.ProductFilter.ToLower().Trim()))
                                              .WhereIf(input.StateFilter != null, e => (int)e.State == input.StateFilter)
                                              .WhereIf(input.FromDateFilter.HasValue, e => e.CreationTime >= input.FromDateFilter)
                                              .WhereIf(input.ToDateFilter.HasValue, e => e.CreationTime < input.ToDateFilter.Value.AddDays(1));


            var totalCount = filter.ToList().Count;
            //var totalCount = await query.CountAsync();
            List<ProductTag> productTags = new List<ProductTag>();

            if (input.Sorting != null)
            {
                productTags = await filter.OrderBy(input.Sorting)
                                         .PageBy(input)
                                         .ToListAsync();
            }
            else
            {
                productTags = await filter.OrderByDescending(x => x.CreationTime)
                                         .PageBy(input)
                                         .ToListAsync();
            }

            var result = new List<ProductTagForReportDto>();
            foreach (var data in productTags)
            {
                var cateName = data.Product.ProductCategoryRelations.Select(c => c.ProductCategory.Name).ToList();
                var cate = string.Join(", ", cateName);
                result.Add(new ProductTagForReportDto()
                {
                    ProductName = data.Product.Name,
                    Category = cate,
                    Sku = data.Product.SKU,
                    TagId = data.Name,
                    State = data.State.ToString(),

                    CreationTime = data.CreationTime
                });
            }

            return new PagedResultDto<ProductTagForReportDto>(
                totalCount,
                result
            );
        }

        [AbpAuthorize(AppPermissions.Pages_ProductTags_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            var productTags = _productTagRepository.GetAll().Where(x => x.Id == input.Id);

            foreach (var item in productTags)
            {
                await _productTagRepository.DeleteAsync(item.Id);
            }
        }

        [AbpAllowAnonymous]
        public async Task<List<ProductInfo>> QueryProductByTag(TagsInput input)
        {
            var pInfoResult = new List<ProductInfo>();
            try
            {
                using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
                {
                    int? tenantId = null;
                    var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == input.MachineId);

                    if (machine != null)
                    {
                        tenantId = machine.TenantId;
                    }

                    var productTags = _productTagRepository.GetAllIncluding(x => x.Product).Where(x => input.Tags.Contains(x.Name) && x.TenantId == tenantId);
                    if (productTags.Count() == 0) return pInfoResult;

                    var machineProductPrices = await _productMachinePriceRepository.GetAllListAsync(x => x.MachineId == input.MachineId && x.TenantId == tenantId);

                    foreach (var tag in input.Tags)
                    {
                        var pt = await productTags.FirstOrDefaultAsync(x => x.Name.Equals(tag) && x.TenantId == tenantId);
                        if (pt == null) continue;
                        var productPrice = machineProductPrices.FirstOrDefault(x => x.ProductId == pt.ProductId);
                        var pInfo = new ProductInfo
                        {
                            ProductId = pt.Product?.Id,
                            ProductName = pt.Product?.Name,
                            Tag = pt.Name
                        };
                        if (productPrice != null)
                        {
                            pInfo.Price = productPrice.Price;
                        }
                        else if (pt.Product != null)
                        {
                            pInfo.Price = (decimal)pt.Product.Price;
                        }
                        pInfoResult.Add(pInfo);
                    }
                }

                return pInfoResult.OrderBy(x => x.ProductName).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error($"Get Product By Tag {ex.Message}", ex);
                return pInfoResult;
            }

        }


        [AbpAllowAnonymous]
        public async Task<List<ProductInfo>> QueryProductByTagV2(TagsInput input)
        {

            var pInfoResult = new List<ProductInfo>();
            try
            {

                using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
                {
                    int? tenantId = null;
                    var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == input.MachineId);

                    if (machine != null)
                    {
                        tenantId = machine.TenantId;
                    }

                    // Find product by tag prefix
                    var tags = input.Tags;
                    var matchProducts = await _productRepository.GetAll().Where(x => !string.IsNullOrEmpty(x.TagPrefix))
                                                                .Where(x => tags.Any(tag => tag.StartsWith(x.TagPrefix)) && x.TenantId == tenantId).ToListAsync();

                    // Insert matching product with tag
                    var insertData = new ListProductTagDto();
                    foreach (var tag in input.Tags)
                    {
                        var foundProducts = matchProducts.Where(x => tag.StartsWith(x.TagPrefix)).ToList();
                        if (foundProducts.Count == 1)
                        {
                            insertData.ListTags.Add(new ProductTagInputDto
                            {
                                Name = tag,
                                ProductId = foundProducts.First().Id.ToString()
                            });
                        }
                        else if (foundProducts.Count > 1)
                        {
                            throw new UserFriendlyException("Duplicate tag prefix: " + foundProducts.First().TagPrefix);
                        }
                    }
                    if (insertData.ListTags.Count > 0)
                    {
                        insertData.TenantId = tenantId;
                        await InsertTags(insertData);
                        await _unitOfWorkManager.Current.SaveChangesAsync();

                    }
                }
                return await QueryProductByTag(input);
            }
            catch (Exception ex)
            {
                Logger.Error($"Get Product By Tag Prefix {ex.Message}", ex);
                return pInfoResult;
            }

        }

        public async Task InsertTags(ListProductTagDto input)
        {
            using (var unitOfWork = _unitOfWorkManager.Begin())
            {
                using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
                {
                    foreach (var item in input.ListTags)
                    {
                        if (await _productTagRepository.GetAll().AnyAsync(x => x.Name == item.Name && x.TenantId == input.TenantId))
                        {
                            var oldProductTag = await _productTagRepository.FirstOrDefaultAsync(x => x.Name == item.Name && x.TenantId == input.TenantId);
                            if (oldProductTag != null)
                            {
                                oldProductTag.ProductId = new Guid(item.ProductId);
                                oldProductTag.TenantId = input.TenantId;
                                oldProductTag.CreationTime = Clock.Now;
                            }
                            await _productTagRepository.UpdateAsync(oldProductTag);
                        }
                        else
                        {
                            var productTag = new ProductTag
                            {
                                Name = item.Name,
                                ProductId = new Guid(item.ProductId),
                                State = ProductTagStateEnum.Mapped,
                                TenantId = input.TenantId
                            };

                            await _productTagRepository.InsertAsync(productTag);
                        }
                    }
                }

                await unitOfWork.CompleteAsync();
            }
        }


    }
}
