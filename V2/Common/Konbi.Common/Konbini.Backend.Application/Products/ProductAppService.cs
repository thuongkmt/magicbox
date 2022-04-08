using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using KonbiCloud.Common;
using KonbiCloud.Common.Dtos;
using KonbiCloud.Machines;
using KonbiCloud.Machines.Dtos;
using KonbiCloud.Messaging;
using KonbiCloud.Messaging.Events;
using KonbiCloud.Products.Dtos;
using Microsoft.EntityFrameworkCore;

namespace KonbiCloud.Products
{
    public class ProductAppService : KonbiCloudAppServiceBase, IProductAppService
    {
        private readonly IRepository<Product, Guid> _productRepository;
        //private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IEventBus _eventBus;
        public ProductAppService(IRepository<Product, Guid> productRepository,      
            IFileStorageService fileStorageService,
            IEventBus eventBus)
        {
            _productRepository = productRepository;
            //_machineRepository = machineRepository;
            _fileStorageService = fileStorageService;
            _eventBus = eventBus;
        }

        public async Task<ListResultDto<ProductDto>> GetAll(ProductListInput input)
        {
            var tenantId = AbpSession.TenantId ?? 0;
        

            try
            {
                var totalItem = await _productRepository
                    .GetAllIncluding(p => p.Categories)
                    .Where(x => x.TenantId == tenantId)
                    .Where(p => string.IsNullOrEmpty(input.CategoryId) ||
                                p.Categories.Any(c => c.CategoryId == new Guid(input.CategoryId)))
                    .Where(x => string.IsNullOrEmpty(input.ProductName) || x.Name.Contains(input.ProductName)).CountAsync();

                var products = await _productRepository
                    .GetAllIncluding(p => p.Categories)
                    .Where(x => x.TenantId == tenantId)
                    .Where(p => string.IsNullOrEmpty(input.CategoryId) ||
                                p.Categories.Any(c => c.CategoryId == new Guid(input.CategoryId)))
                    .Where(x => string.IsNullOrEmpty(input.ProductName) || x.Name.Contains(input.ProductName))
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
                    .OrderBy(x => x.Name)
                    .ToListAsync();

                var results = products.MapTo<List<ProductDto>>();
                foreach (var productDto in results)
                {
                    productDto.Categories = new List<Guid>();
                    var product = products.Single(x => x.Id == productDto.Id);
                    foreach (var productCat in product.Categories)
                    {
                        productDto.Categories.Add(productCat.CategoryId);
                    }
                }

                var output = new PageResultListDto<ProductDto>(results);
                output.TotalCount = totalItem;

                return output;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                return new ListResultDto<ProductDto>();
            }
        }

        public async Task Create(CreateProductInput input)
        {
            try
            {
                var newId = Guid.NewGuid();
                if (!string.IsNullOrEmpty(input.ImageUrl))
                {
                    var base64 = input.FileContent.Split(',')[1];
                    byte[] data = Convert.FromBase64String(base64);
                    using (var ms = new MemoryStream(data))
                    {
                        var fileType = Path.GetExtension(input.ImageUrl);
                        var url = await _fileStorageService.CreateOrReplace(newId.ToString(), fileType, ms);
                        input.ImageUrl = url;
                        input.ImageChecksum = base64.WeakHash();
                    }
                }

                var product = ObjectMapper.Map<Product>(input);
                product.SKU = product.SKU.Trim();

                product.TenantId = AbpSession.GetTenantId();
                product.Id = Guid.NewGuid();

                if (input.CategoryIds != null && input.CategoryIds.Count > 0)
                {
                    foreach (var categoryId in input.CategoryIds)
                    {
                        var prodCat = new ProductCategory
                        {
                            CategoryId = categoryId,
                            Product = product
                        };
                        product.Categories.Add(prodCat);
                    }
                }
                var evt = product.MapTo<ProductUpdatedIntegrationEvent>();
                _eventBus.Publish(evt);
                await _productRepository.InsertAsync(product);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
            }
        }

        public async Task<ProductDto> GetDetail(EntityDto<Guid> input)
        {
            var product = await _productRepository
                .GetAllIncluding(p => p.Categories)
                .Where(e => e.Id == input.Id)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                throw new UserFriendlyException("Could not found the category, maybe it's deleted.");
            }

            var productDto = product.MapTo<ProductDto>();
            productDto.Categories = new List<Guid>();
            foreach (var productCat in product.Categories)
            {
                productDto.Categories.Add(productCat.CategoryId);
            }
            return productDto;
        }

        public async Task<Product> Update(CreateProductInput input)
        {
            //CheckUpdatePermission();
            var product = await _productRepository.GetAllIncluding(x => x.Categories).SingleOrDefaultAsync(x => x.Id == Guid.Parse(input.Id));



            if (product != null)
            {

                ObjectMapper.Map(input, product);
                product.SKU = product.SKU.Trim();
                if (!string.IsNullOrEmpty(input.FileContent))
                {
                    var base64 = input.FileContent.Split(',')[1];
                    byte[] data = Convert.FromBase64String(base64);
                    using (MemoryStream ms = new MemoryStream(data))
                    {
                        var fileType = Path.GetExtension(input.ImageUrl);
                        string newImageFile = input.Id + DateTime.Now.Ticks;
                        var url = await _fileStorageService.CreateOrReplace(newImageFile, fileType, ms);
                        product.ImageUrl = url;
                        product.ImageChecksum = base64.WeakHash();
                    }
                }

                var existingCategories = product.Categories.Count > 0 ? product.Categories.Select(x => x.CategoryId).ToList() : new List<Guid>();
                var inputCategories = (input.CategoryIds != null && input.CategoryIds.Count > 0 ? input.CategoryIds : new List<Guid>());

                product.Categories = product.Categories.Where(x => !existingCategories.Except(inputCategories).ToList().Contains(x.CategoryId)).ToList();

                foreach (var iItem in inputCategories.Except(existingCategories))
                {
                    product.Categories.Add(new ProductCategory { CategoryId = iItem });
                }
                var evt = product.MapTo<ProductUpdatedIntegrationEvent>();
                _eventBus.Publish(evt);
                await _productRepository.UpdateAsync(product);
            }
            return product;
        }

        public async Task Delete(EntityDto<Guid> input)
        {
            var product = await _productRepository.FirstOrDefaultAsync(e => e.Id == input.Id);
            var evt = new ProductDeletedIntegrationEvent
            {
                Id = product.Id
            };
            _eventBus.Publish(evt);
            await _productRepository.DeleteAsync(product);
        }
    }
}
