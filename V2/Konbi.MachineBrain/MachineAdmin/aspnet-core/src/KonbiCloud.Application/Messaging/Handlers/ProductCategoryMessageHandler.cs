using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using KonbiCloud.Products;
using Konbini.Messages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KonbiCloud.Messaging.Handlers
{
    public class ProductCategoryMessageHandler : IProductCategoryMessageHandler, ITransientDependency
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<ProductCategory, Guid> _productCategoryRepository;
        private readonly IRepository<ProductCategoryRelation, Guid> _productCategoryRelationRepository;
        private readonly ILogger _logger;

        public ProductCategoryMessageHandler(
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<ProductCategory, Guid> productCategoryRepository,
            IRepository<ProductCategoryRelation, Guid> productCategoryRelationRepository)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _productCategoryRepository = productCategoryRepository;
            _productCategoryRelationRepository = productCategoryRelationRepository;
        }

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    var productCategory = JsonConvert.DeserializeObject<ProductCategory>(keyValueMessage.JsonValue);

                    var id = productCategory.Id;
                    if(productCategory.IsDeleted)
                    {
                        var productCategoryRelations = _productCategoryRelationRepository.GetAll().
                                            Where(x => x.ProductCategoryId == id);

                        foreach (var item in productCategoryRelations)
                        {
                            await _productCategoryRelationRepository.DeleteAsync(item.Id);
                        }

                        await _productCategoryRepository.DeleteAsync(id);
                    }
                    else
                    {
                        if (await _productCategoryRepository.GetAll().AnyAsync(x => x.Id == id))
                        {
                            var oldProductCategory = await _productCategoryRepository.SingleAsync(x => x.Id == id);
                            oldProductCategory.Name = productCategory.Name;
                            oldProductCategory.Desc = productCategory.Desc;
                            oldProductCategory.Code = productCategory.Code;

                            await _productCategoryRepository.UpdateAsync(oldProductCategory);
                        }
                        else
                        {
                            await _productCategoryRepository.InsertAsync(productCategory);
                        }
                    }

                    await unitOfWork.CompleteAsync();
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Error("Process ProductCategory", e);
                return false;
            }
        }
    }
}
