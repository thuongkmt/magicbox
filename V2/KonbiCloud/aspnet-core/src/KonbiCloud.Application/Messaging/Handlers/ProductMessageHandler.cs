using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using KonbiCloud.Machines;
using KonbiCloud.Products;
using Konbini.Messages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KonbiCloud.Messaging
{
    public class ProductMessageHandler : IProductMessageHandler  , ITransientDependency
    {
        //private readonly IUnitOfWorkManager _unitOfWorkManager;
        //private readonly IRepository<Machine, Guid> _machineRepository;
        //private readonly IRepository<Product, Guid> _productRepository;
        //private readonly IRepository<ProductCategoryRelation, Guid> _productCategoryRelationRepository;
        //private readonly ILogger _logger;

        //public ProductMessageHandler(IUnitOfWorkManager unitOfWorkManager, IRepository<Machine, Guid> machineRepository, IRepository<Product, Guid> productRepository, IRepository<ProductCategoryRelation, Guid> productCategoryRelationRepository)
        //{
        //    _unitOfWorkManager = unitOfWorkManager;
        //    _machineRepository = machineRepository;
        //    _productRepository = productRepository;
        //    _productCategoryRelationRepository = productCategoryRelationRepository;
        //}

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            throw new NotImplementedException();
            //try
            //{
            //    using (var unitOfWork = _unitOfWorkManager.Begin())
            //    {
            //        var product = JsonConvert.DeserializeObject<Product>(keyValueMessage.JsonValue);
            //        var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == keyValueMessage.MachineId);
            //        if (machine == null) return true;
            //        product.TenantId = machine.TenantId;
            //        var id = product.Id;
            //        if (await _productRepository.GetAll().AnyAsync(x => x.Id == id))
            //        {
            //            var oldProduct = await _productRepository.SingleAsync(x => x.Id == id);
            //            oldProduct.Name = product.Name;
            //            oldProduct.Desc = product.Desc;
            //            oldProduct.SKU = product.SKU;
            //            oldProduct.Price = product.Price;
            //            oldProduct.Barcode = product.Barcode;
            //            oldProduct.Tag = product.Tag;
            //            oldProduct.ShortDesc = product.ShortDesc;
            //            oldProduct.ImageUrl = product.ImageUrl;

            //            await _productRepository.UpdateAsync(oldProduct);

            //            if(await _productCategoryRelationRepository.GetAll().AnyAsync(x => x.ProductId == id))
            //            {
            //                var productCategoryRelations = _productCategoryRelationRepository.GetAll().Where(x => x.ProductId == id);

            //                foreach (var item in productCategoryRelations)
            //                {
            //                    await _productCategoryRelationRepository.DeleteAsync(item.Id);
            //                }
            //            }

            //            foreach(var relation in product.ProductCategoryRelations)
            //            {
            //                await _productCategoryRelationRepository.InsertAsync(relation);
            //            }
                        
            //        }
            //        else
            //        {
            //            await _productRepository.InsertAsync(product);
            //            foreach (var relation in product.ProductCategoryRelations)
            //            {
            //                await _productCategoryRelationRepository.InsertAsync(relation);
            //            }
            //        }

            //        await unitOfWork.CompleteAsync();
            //        return true;
            //    }
            //}
            //catch (Exception e)
            //{
            //    _logger.Error("ProcessProduct", e);
            //    return false;
            //}
        }
    }
}
