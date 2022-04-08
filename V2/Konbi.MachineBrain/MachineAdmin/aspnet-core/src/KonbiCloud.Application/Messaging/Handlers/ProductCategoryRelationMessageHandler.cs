using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using KonbiCloud.Products;
using Konbini.Messages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KonbiCloud.Messaging.Handlers
{
    public class ProductCategoryRelationMessageHandler : IProductCategoryRelationMessageHandler
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<ProductCategoryRelation, Guid> _productCategoryRelationRepository;
        private readonly IRepository<ProductCategory, Guid> _productCategoryRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly ILogger _logger;

        public ProductCategoryRelationMessageHandler(
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<ProductCategory, Guid> productCategoryRepository,
            IRepository<ProductCategoryRelation, Guid> productCategoryRelationRepository,
            IRepository<Product,Guid> productRepository)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _productCategoryRelationRepository = productCategoryRelationRepository;
            _productCategoryRepository = productCategoryRepository;
            _productRepository = productRepository;
        }

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    var productCategoryRelations = JsonConvert.DeserializeObject<List<ProductCategoryRelation>>(keyValueMessage.JsonValue);
                    if (productCategoryRelations.Any())
                    {
                        var pdc = productCategoryRelations.FirstOrDefault();
                        var currentProductCategoryRelations = _productCategoryRelationRepository.GetAll().Where(x => x.ProductId == pdc.ProductId);
                        foreach(var item in currentProductCategoryRelations)
                        {
                            await _productCategoryRelationRepository.DeleteAsync(item.Id);
                        }

                        if(!pdc.IsDeleted)
                        {
                            foreach(var item in productCategoryRelations)
                            {
                                if(await _productRepository.GetAll().AnyAsync(x => x.Id == item.ProductId) && await _productCategoryRepository.GetAll().AnyAsync(x => x.Id == item.ProductCategoryId))
                                {
                                    await _productCategoryRelationRepository.InsertAsync(new ProductCategoryRelation {
                                        ProductId = item.ProductId,
                                        ProductCategoryId = item.ProductCategoryId
                                    });
                                }
                            }
                        }
                        
                    }

                    await unitOfWork.CompleteAsync();
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Error("ProcessProduct", e);
                return false;
            }
        }
    }
}
