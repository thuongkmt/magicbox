using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using KonbiCloud.Machines;
using KonbiCloud.Products;
using Konbini.Messages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace KonbiCloud.Messaging.Handlers
{
    public class ProductCategoryMessageHandler : IProductCategoryMessageHandler , ITransientDependency
    {
        //private readonly IUnitOfWorkManager _unitOfWorkManager;
        //private readonly IRepository<Machine, Guid> _machineRepository;
        //private readonly IRepository<ProductCategory, Guid> _productCategoryRepository;
        //private readonly ILogger _logger;

        //public ProductCategoryMessageHandler(
        //    IUnitOfWorkManager unitOfWorkManager, 
        //    IRepository<Machine, Guid> machineRepository, 
        //    IRepository<ProductCategory, Guid> productCategoryRepository)
        //{
        //    _unitOfWorkManager = unitOfWorkManager;
        //    _machineRepository = machineRepository;
        //    _productCategoryRepository = productCategoryRepository;
        //}

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            throw new NotImplementedException();
            //try
            //{
            //    using (var unitOfWork = _unitOfWorkManager.Begin())
            //    {
            //        var productCategory = JsonConvert.DeserializeObject<ProductCategory>(keyValueMessage.JsonValue);
            //        var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == keyValueMessage.MachineId);
            //        if (machine == null) return true;
            //        productCategory.TenantId = machine.TenantId;
            //        var id = productCategory.Id;
            //        if (await _productCategoryRepository.GetAll().AnyAsync(x => x.Id == id))
            //        {
            //            var oldProductCategory = await _productCategoryRepository.SingleAsync(x => x.Id == id);
            //            oldProductCategory.Name = productCategory.Name;
            //            oldProductCategory.Desc = productCategory.Desc;

            //            await _productCategoryRepository.UpdateAsync(oldProductCategory);
            //        }
            //        else
            //        {
            //            await _productCategoryRepository.InsertAsync(productCategory);
            //        }

            //        await unitOfWork.CompleteAsync();
            //        return true;
            //    }
            //}
            //catch (Exception e)
            //{
            //    _logger.Error("Process ProductCategory", e);
            //    return false;
            //}
        }
    }
}
