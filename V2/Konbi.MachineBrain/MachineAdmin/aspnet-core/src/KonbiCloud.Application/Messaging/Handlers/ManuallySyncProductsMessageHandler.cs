using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using KonbiCloud.Products;
using KonbiCloud.Products.Dtos;
using Konbini.Messages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KonbiCloud.Messaging.Handlers
{
    public class ManuallySyncProductsMessageHandler : IManuallySyncProductsMessageHandler
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<ProductCategoryRelation, Guid> _productCategoryRelationRepository;
        private readonly IRepository<ProductCategory, Guid> _productCategoryRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly ILogger _logger;

        public ManuallySyncProductsMessageHandler(
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<ProductCategory, Guid> productCategoryRepository,
            IRepository<ProductCategoryRelation, Guid> productCategoryRelationRepository,
            IRepository<Product, Guid> productRepository,
            ILogger logger
            )
        {
            _unitOfWorkManager = unitOfWorkManager;
            _productCategoryRelationRepository = productCategoryRelationRepository;
            _productCategoryRepository = productCategoryRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    var products = JsonConvert.DeserializeObject<List<SyncProductDto>>(keyValueMessage.JsonValue);

                    //First insert categories
                    var productCategories = products.SelectMany(x => x.Categories).GroupBy(x => x.Id).Select(x => x.FirstOrDefault());

                    foreach (var category in productCategories)
                    {
                        using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                        {
                            if (await _productCategoryRepository.GetAll().AnyAsync(x => x.Id == category.Id))
                            {
                                var oldCategory = await _productCategoryRepository.SingleAsync(x => x.Id == category.Id);
                                oldCategory.IsDeleted = false;
                                oldCategory.Name = category.Name;
                                oldCategory.Code = category.Code;
                                oldCategory.Desc = category.Desc;

                                await _productCategoryRepository.UpdateAsync(oldCategory);
                            }
                            else
                            {
                                var newCategory = await _productCategoryRepository.InsertAsync(new ProductCategory
                                {
                                    Id = category.Id,
                                    Name = category.Name,
                                    Desc = category.Desc,
                                    Code = category.Code
                                });
                            }
                        }
                    }

                    //Then insert product
                    foreach (var product in products)
                    {
                        var id = product.Id;

                        using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                        {
                            if (await _productRepository.GetAll().AnyAsync(x => x.Id == id))
                            {
                                var oldProduct = await _productRepository.SingleAsync(x => x.Id == id);
                                oldProduct.IsDeleted = false;
                                oldProduct.Name = product.Name;
                                oldProduct.Desc = product.Desc;
                                oldProduct.SKU = product.SKU;
                                oldProduct.Price = product.Price;
                                oldProduct.Barcode = product.Barcode;
                                oldProduct.Tag = product.Tag;
                                oldProduct.ShortDesc = product.ShortDesc;
                                oldProduct.ImageUrl = product.ImageUrl;

                                await _productRepository.UpdateAsync(oldProduct);
                            }
                            else
                            {
                                await _productRepository.InsertAsync(new Product
                                {
                                    Id = id,
                                    Name = product.Name,
                                    SKU = product.SKU,
                                    Barcode = product.Barcode,
                                    Price = product.Price,
                                    ShortDesc = product.ShortDesc,
                                    Desc = product.Desc,
                                    Tag = product.Tag,
                                    ImageUrl = product.ImageUrl
                                });
                            }
                        }

                        var currentProductCategoryRelations = _productCategoryRelationRepository.GetAll().Where(x => x.ProductId == id);
                        foreach (var item in currentProductCategoryRelations)
                        {
                            await _productCategoryRelationRepository.DeleteAsync(item.Id);
                        }

                        foreach (var category in product.Categories)
                        {
                            await _productCategoryRelationRepository.InsertAsync(new ProductCategoryRelation
                            {
                                ProductId = id,
                                ProductCategoryId = category.Id
                            });
                        }
                    }

                    await unitOfWork.CompleteAsync();
                    return true;
                }
            }
            catch(Exception ex)
            {
                _logger.Error("Error when process synchronize products send from cloud to machine: ", ex);
                return false;
            }
        }
    }
}
