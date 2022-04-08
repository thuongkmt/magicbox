using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using KonbiCloud.Products;
using Konbini.Messages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.Messaging.Handlers
{
    public class ManuallySyncProductCategoriesMessageHandler : IManuallySyncProductCategoriesMessageHandler
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<ProductCategory, Guid> _productCategoryRepository;
        private readonly ILogger _logger;

        public ManuallySyncProductCategoriesMessageHandler(
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<ProductCategory, Guid> productCategoryRepository,
            ILogger logger)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _productCategoryRepository = productCategoryRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    var productCategories = JsonConvert.DeserializeObject<List<ProductCategory>>(keyValueMessage.JsonValue);

                    foreach(var productCategory in productCategories)
                    {
                        var id = productCategory.Id;

                        using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                        {
                            if (await _productCategoryRepository.GetAll().AnyAsync(x => x.Id == id))
                            {
                                var oldProductCategory = await _productCategoryRepository.SingleAsync(x => x.Id == id);
                                oldProductCategory.IsDeleted = false;
                                oldProductCategory.Name = productCategory.Name;
                                oldProductCategory.Code = productCategory.Code;
                                oldProductCategory.Desc = productCategory.Desc;

                                await _productCategoryRepository.UpdateAsync(oldProductCategory);
                            }
                            else
                            {
                                await _productCategoryRepository.InsertAsync(productCategory);
                            }
                        }
                    }
                    
                    await unitOfWork.CompleteAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error when synchronize products from cloud to machine ", ex);
                return false;
            }
        }
    }
}
