using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using KonbiCloud.Products.Exporting;
using KonbiCloud.Products.Dtos;
using KonbiCloud.Dto;
using Abp.Application.Services.Dto;
using KonbiCloud.Authorization;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Konbini.Messages.Services;
using Abp.UI;
using KonbiCloud.Machines;

namespace KonbiCloud.Products
{
    [AbpAuthorize(AppPermissions.Pages_Products)]
    public class ProductsAppService : KonbiCloudAppServiceBase, IProductsAppService
    {
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IProductsExcelExporter _productsExcelExporter;
        private readonly IRepository<ProductCategoryRelation, Guid> _productCategoryRelationRepository;
        private readonly IRepository<ProductCategory, Guid> _productCategory;
        private readonly IRepository<ProductTag, Guid> _productTagRepository;
        private readonly ISendMessageToMachineClientService _sendMessageToMachineService;
        private readonly IRepository<Machine, Guid> _machineRepository;

        public ProductsAppService(IRepository<Product, Guid> productRepository,
              IProductsExcelExporter productsExcelExporter,
              IRepository<ProductCategoryRelation, Guid> productCategoryRelationRepository,
              IRepository<ProductCategory, Guid> productCategory,
              ISendMessageToMachineClientService sendMessageToMachineService,
              IRepository<ProductTag, Guid> productTagRepository,
              IRepository<Machine, Guid> machineRepository)
        {
            _productRepository = productRepository;
            _productsExcelExporter = productsExcelExporter;
            _productCategoryRelationRepository = productCategoryRelationRepository;
            _productCategory = productCategory;
            _sendMessageToMachineService = sendMessageToMachineService;
            _productTagRepository = productTagRepository;
            _machineRepository = machineRepository;
        }

        public async Task<PagedResultDto<ProductDto>> GetAll(GetAllProductsInput input)
        {
            var filteredProducts = _productRepository.GetAll()
                    .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    e => false
                    || (e.Name != null && e.Name.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase))
                    || (e.SKU != null && e.SKU.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase))
                    || (e.Barcode != null && e.Barcode.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase))
                    || (e.Tag != null && e.Tag.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase)))
                    .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name != null && e.Name.Contains(input.NameFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                    .WhereIf(!string.IsNullOrWhiteSpace(input.SKUFilter), e => e.SKU != null && e.SKU.Contains(input.SKUFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                    .WhereIf(!string.IsNullOrWhiteSpace(input.BarcodeFilter), e => e.Barcode != null && e.Barcode.Contains(input.BarcodeFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                    .WhereIf(!string.IsNullOrWhiteSpace(input.TagFilter), e => e.Tag != null && e.Tag.Contains(input.TagFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                    .WhereIf(input.MinPriceFilter != null, e => e.Price >= input.MinPriceFilter)
                    .WhereIf(input.MaxPriceFilter != null, e => e.Price <= input.MaxPriceFilter)
                    .WhereIf(input.CategoryFilter != null && input.CategoryFilter != Guid.Empty, e => e.ProductCategoryRelations != null && e.ProductCategoryRelations.Any(x => x.ProductCategoryId == input.CategoryFilter));

            var query = from o in filteredProducts select ObjectMapper.Map<ProductDto>(o);

            var totalCount = await query.CountAsync();

            List<ProductDto> products = await query
                .OrderBy(input.Sorting ?? "name asc")
                .PageBy(input)
                .ToListAsync();

            foreach (var item in products)
            {
                var productCategoryNames = (
                                                from p in _productRepository.GetAll().Where(x => x.Id == item.Id)
                                                join pcr in _productCategoryRelationRepository.GetAll() on p.Id equals pcr.ProductId
                                                join pc in _productCategory.GetAll() on pcr.ProductCategoryId equals pc.Id
                                                select pc.Name
                                            ).ToList();

                item.CategoriesName = productCategoryNames;
            }


            return new PagedResultDto<ProductDto>(
                totalCount,
                products
            );
        }
        public async Task<PagedResultDto<ProductDto>> GetAllNoFilter()
        {
            var filteredProducts = await _productRepository.GetAllListAsync();

            var resutls = filteredProducts.Select(x => ObjectMapper.Map<ProductDto>(x)).OrderBy(x => x.Name);

            return new PagedResultDto<ProductDto>(
                0,
                resutls.ToList()
            );
        }

        public async Task<FileDto> GetProductsToExcel(GetAllProductsForExcelInput input)
        {
            var filteredProducts = _productRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                        e => false
                        || (e.SKU != null && e.SKU.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase))
                        || (e.Name != null && e.Name.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase)))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.SKUFilter), e => e.SKU != null && e.SKU.ToLower() == input.SKUFilter.ToLower().Trim())
                        .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name != null && e.Name.ToLower() == input.NameFilter.ToLower().Trim())
                        .WhereIf(input.MinPriceFilter != null, e => e.Price >= input.MinPriceFilter)
                        .WhereIf(input.MaxPriceFilter != null, e => e.Price <= input.MaxPriceFilter);

            var query = from o in filteredProducts select ObjectMapper.Map<ProductDto>(o);

            var productListDtos = await query.ToListAsync();
            return _productsExcelExporter.ExportToFile(productListDtos);
        }

        public async Task<ProductDto> GetProductForView(Guid id)
        {
            var product = await _productRepository.GetAsync(id);

            var output = ObjectMapper.Map<ProductDto>(product);

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_Products_Edit)]
        public async Task<GetProductForEditOutput> GetProductForEdit(EntityDto<Guid> input)
        {
            var product = await _productRepository.FirstOrDefaultAsync(input.Id);
            var editProductCategories = _productCategoryRelationRepository.GetAll()
                .Where(x => x.ProductId == input.Id)
                .Select(x => x.ProductCategoryId).ToList();

            var editProduct = new CreateOrEditProductDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Barcode = product.Barcode,
                ShortDesc = product.ShortDesc,
                Desc = product.Desc,
                Tag = product.Tag,
                TagPrefix = product.TagPrefix,
                ImageUrl = product.ImageUrl,
                Price = product.Price,
                CategoryIds = editProductCategories
            };

            var result = new GetProductForEditOutput
            {
                Product = editProduct
            };

            return result;
        }

        public async Task CreateOrEdit(CreateOrEditProductDto input)
        {
            Product product = null;

            if(!string.IsNullOrEmpty(input.TagPrefix))
            {
                // Validate prefix
                var p = _productRepository.GetAll().Where(x => x.TagPrefix != null)
                                                    .Where(x => x.TagPrefix.StartsWith(input.TagPrefix)).FirstOrDefault();
                if (p != null)
                {
                    throw new UserFriendlyException($"RFID Tag Prefix \"{input.TagPrefix}\" is already used for product \"{p.Name}\"");
                }
            }

            if (input.Id == null)
            {
                product = await Create(input);
            }
            else
            {
                product = await Update(input);
            }

            var categories = _productCategory.GetAll().Where(x => input.CategoryIds.Contains(x.Id)).Select(x => new SyncProductCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                Desc = x.Desc
            }).ToList();

            int? tenantId = null;
            if (AbpSession.TenantId != null)
            {
                tenantId = (int?)AbpSession.TenantId;
            }

            var machines = _machineRepository.GetAll().Where(x => x.TenantId == tenantId);

            if (machines.Any())
            {
                foreach (var machine in machines)
                {
                    _sendMessageToMachineService.SendQueuedMsgToMachines(new KeyValueMessage
                    {
                        Key = MessageKeys.Product,
                        MachineId = machine.Id,
                        Value = new SyncProductDto
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
                        }
                    }, CloudToMachineType.ToMachineId);
                }
            }

        }

        [AbpAuthorize(AppPermissions.Pages_Products_Create)]
        private async Task<Product> Create(CreateOrEditProductDto input)
        {
            int? tenantId = null;
            if (AbpSession.TenantId != null)
            {
                tenantId = (int?)AbpSession.TenantId;
            }

            var skuProduct = _productRepository.GetAll().Where(x => x.SKU == input.SKU);
            if (skuProduct.Any())
            {
                throw new UserFriendlyException("There are an existing product with SKU : " + input.SKU);
            }

            if (!String.IsNullOrEmpty(input.Name))
            {
                var nameProduct = _productRepository.GetAll().Where(x => x.Name == input.Name);
                if (nameProduct.Any())
                {
                    throw new UserFriendlyException("There are an existing product with name : " + input.Name);
                }
            }

            if (!String.IsNullOrEmpty(input.Barcode))
            {
                var barcodeProduct = _productRepository.GetAll().Where(x => x.Barcode == input.Barcode);
                if (barcodeProduct.Any())
                {
                    throw new UserFriendlyException("There are an existing product with barcode : " + input.Barcode);
                }
            }

            var product = ObjectMapper.Map<Product>(input);
            product.TenantId = tenantId;

            var insertProductResult = await _productRepository.InsertAsync(product);

            if (input.CategoryIds != null && input.CategoryIds.Count > 0)
            {
                foreach (var categoryId in input.CategoryIds)
                {
                    var prodCat = new ProductCategoryRelation
                    {
                        ProductId = insertProductResult.Id,
                        ProductCategoryId = categoryId,
                        TenantId = tenantId
                    };
                    await _productCategoryRelationRepository.InsertAsync(prodCat);
                }
            }

            return product;
        }

        [AbpAuthorize(AppPermissions.Pages_Products_Edit)]
        private async Task<Product> Update(CreateOrEditProductDto input)
        {
            var skuProduct = _productRepository.GetAll().Where(x => x.SKU == input.SKU && x.Id != input.Id);

            if (skuProduct.Any())
            {
                throw new UserFriendlyException("There are an existing product with SKU : " + input.SKU);
            }

            if (!String.IsNullOrEmpty(input.Name))
            {
                var nameProduct = _productRepository.GetAll().Where(x => x.Name == input.Name && x.Id != input.Id);
                if (nameProduct.Any())
                {
                    throw new UserFriendlyException("There are an existing product with name : " + input.Name);
                }
            }

            if (!String.IsNullOrEmpty(input.Barcode))
            {
                var barcodeProduct = _productRepository.GetAll().Where(x => x.Barcode == input.Barcode && x.Id != input.Id);
                if (barcodeProduct.Any())
                {
                    throw new UserFriendlyException("There are an existing product with barcode : " + input.Barcode);
                }
            }

            var product = await _productRepository.FirstOrDefaultAsync((Guid)input.Id);
            var productCategoryRelations = _productCategoryRelationRepository.GetAll().
                                            Where(x => x.ProductId == input.Id);

            foreach (var item in productCategoryRelations)
            {
                await _productCategoryRelationRepository.DeleteAsync(item.Id);
            }

            if (input.CategoryIds != null && input.CategoryIds.Any())
            {
                foreach (var categoryId in input.CategoryIds)
                {
                    var prodCat = new ProductCategoryRelation
                    {
                        ProductId = (Guid)input.Id,
                        ProductCategoryId = categoryId
                    };

                    if (AbpSession.TenantId != null)
                    {
                        prodCat.TenantId = (int?)AbpSession.TenantId;
                    }

                    await _productCategoryRelationRepository.InsertAsync(prodCat);
                }
            }

            ObjectMapper.Map(input, product);
            return product;
        }

        [AbpAuthorize(AppPermissions.Pages_Products_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            if (await _productTagRepository.GetAll().AnyAsync(x => x.ProductId == input.Id))
            {
                throw new UserFriendlyException("Can not delete this product because it is having a tag");
            }

            var productCategoryRelations = _productCategoryRelationRepository.GetAll().Where(x => x.ProductId == input.Id);

            foreach (var item in productCategoryRelations)
            {
                await _productCategoryRelationRepository.DeleteAsync(item.Id);
            }
            await _productRepository.DeleteAsync(input.Id);

            int? tenantId = null;
            if (AbpSession.TenantId != null)
            {
                tenantId = (int?)AbpSession.TenantId;
            }

            var machines = _machineRepository.GetAll().Where(x => x.TenantId == tenantId);

            if (machines.Any())
            {
                foreach (var machine in machines)
                {
                    _sendMessageToMachineService.SendQueuedMsgToMachines(new KeyValueMessage
                    {
                        Key = MessageKeys.Product,
                        Value = new SyncProductDto { Id = input.Id, IsDeleted = true },
                        MachineId = machine.Id
                    }, CloudToMachineType.ToMachineId);
                }
            }
        }

        [AbpAllowAnonymous]
        public async Task Import(List<CreateOrEditProductDto> items)
        {
            foreach (var item in items)
            {
                if (!String.IsNullOrEmpty(item.Name) && items.Count(x => x.Name == item.Name) > 1)
                {
                    throw new UserFriendlyException("Dulicate product with name : " + item.Name + " in the import file");
                }

                if (!String.IsNullOrEmpty(item.Barcode) && items.Count(x => x.Barcode == item.Barcode) > 1)
                {
                    throw new UserFriendlyException("Dulicate barcode : " + item.Barcode + " in the import file");
                }

                if (!String.IsNullOrEmpty(item.SKU) && items.Count(x => x.SKU == item.SKU) > 1)
                {
                    throw new UserFriendlyException("Dulicate SKU : " + item.SKU + " in the import file");
                }

                if (item.Price == null)
                {
                    throw new UserFriendlyException("Please enter price for product : " + item.Name);
                }

                if (!String.IsNullOrEmpty(item.CategoryNames))
                {
                    var categoryNames = item.CategoryNames.Split(";");
                    if (categoryNames.Any())
                    {
                        foreach (var categoryName in categoryNames)
                        {
                            var category = _productCategory.FirstOrDefault(x => x.Name.ToLower() == categoryName.Trim().ToLower());
                            if (category != null)
                            {
                                item.CategoryIds.Add(category.Id);
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(item.SKU) && item.SKU.StartsWith('_'))
                {
                    item.SKU = item.SKU.Remove(0, 1);
                }

                if (!String.IsNullOrEmpty(item.Barcode) && item.Barcode.StartsWith('_'))
                {
                    item.Barcode = item.Barcode.Remove(0, 1);
                }

                await CreateOrEdit(item);
            }
        }
    }
}