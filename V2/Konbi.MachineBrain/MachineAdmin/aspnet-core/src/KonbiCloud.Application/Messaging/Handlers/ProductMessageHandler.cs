using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using KonbiCloud.Products;
using KonbiCloud.Products.Dtos;
using Konbini.Messages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;


namespace KonbiCloud.Messaging
{
    public class ProductMessageHandler : IProductMessageHandler
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<ProductCategory, Guid> _productCategoryRepository;
        private readonly IRepository<ProductCategoryRelation, Guid> _productCategoryRelationRepository;
        private readonly ILogger _logger;

        public ProductMessageHandler(
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<Product, Guid> productRepository,
            IRepository<ProductCategory, Guid> productCategoryRepository,
            IRepository<ProductCategoryRelation, Guid> productCategoryRelationRepository)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _productCategoryRelationRepository = productCategoryRelationRepository;
        }

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    var product = JsonConvert.DeserializeObject<SyncProductDto>(keyValueMessage.JsonValue);
                    var id = product.Id;

                    if(product.IsDeleted)
                    {
                        var productCategoryRelations = _productCategoryRelationRepository.GetAll().
                                            Where(x => x.ProductId == id);

                        foreach (var item in productCategoryRelations)
                        {
                            await _productCategoryRelationRepository.DeleteAsync(item.Id);
                        }

                        await _productRepository.DeleteAsync(id);
                    }
                    else
                    {
                        if (await _productRepository.GetAll().AnyAsync(x => x.Id == id))
                        {
                            var oldProduct = await _productRepository.SingleAsync(x => x.Id == id);
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
                            await _productRepository.InsertAsync(new Product {
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

                        var currentProductCategoryRelations = _productCategoryRelationRepository.GetAll().Where(x => x.ProductId == id);
                        foreach (var item in currentProductCategoryRelations)
                        {
                            await _productCategoryRelationRepository.DeleteAsync(item.Id);
                        }

                        foreach (var category in product.Categories)
                        {
                            if (await _productCategoryRepository.GetAll().AnyAsync(x => x.Id == category.Id))
                            {
                                await _productCategoryRelationRepository.InsertAsync(new ProductCategoryRelation
                                {
                                    ProductId = id,
                                    ProductCategoryId = category.Id
                                });
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

                                await _productCategoryRelationRepository.InsertAsync(new ProductCategoryRelation
                                {
                                    ProductId = id,
                                    ProductCategoryId = newCategory.Id
                                });
                            }
                        }
                    }

                    await unitOfWork.CompleteAsync();
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error when sync product: ", e);
                return false;
            }
        }
    }
}
