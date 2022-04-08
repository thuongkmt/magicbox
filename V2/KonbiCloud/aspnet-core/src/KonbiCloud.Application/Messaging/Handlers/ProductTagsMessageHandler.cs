using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Timing;
using Castle.Core.Logging;
using KonbiCloud.Dto;
using KonbiCloud.Enums;
using KonbiCloud.Products;
using KonbiCloud.SignalR;
using Konbini.Messages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KonbiCloud.Messaging.Handlers
{
    public class ProductTagsMessageHandler : IProductTagsMessageHandler, ITransientDependency
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<ProductTag, Guid> _productTagRepository;
        private readonly IMagicBoxMessageCommunicator _magicBoxMessage;

        public ProductTagsMessageHandler( 
            IUnitOfWorkManager unitOfWorkManager, 
            IRepository<ProductTag, Guid> productTagRepository,  
            ILogger logger,
            IMagicBoxMessageCommunicator magicBoxMessage) 
        {
            _unitOfWorkManager = unitOfWorkManager;
            _productTagRepository = productTagRepository;
            _logger = logger;
            _magicBoxMessage = magicBoxMessage;
        }

     
        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
                    {
                        var productMaps = JsonConvert.DeserializeObject<ProductTagsDto>(keyValueMessage.JsonValue); ;
                        var data = productMaps.TagProductMaps;
                        foreach (var item in data)
                        {
                            if (await _productTagRepository.GetAll().AnyAsync(x => x.Name == item.Key && x.TenantId == keyValueMessage.TenantId))
                            {
                                var oldProductTag = await _productTagRepository.FirstOrDefaultAsync(x => x.Name == item.Key && x.TenantId == keyValueMessage.TenantId);
                                if (oldProductTag != null)
                                {
                                    oldProductTag.ProductId = item.Value;
                                    oldProductTag.TenantId = keyValueMessage.TenantId;
                                }
                                await _productTagRepository.UpdateAsync(oldProductTag);
                            }
                            else
                            {
                                var productTag = new ProductTag
                                {
                                    Id = Guid.NewGuid(),
                                    ProductId = item.Value,
                                    Name = item.Key,
                                    State = ProductTagStateEnum.Mapped,
                                    CreationTime = Clock.Now,
                                    TenantId = keyValueMessage.TenantId
                                };

                                await _productTagRepository.InsertAsync(productTag);
                            }
                        }

                        await unitOfWork.CompleteAsync();
                    }

                    //Notify Client
                    await _magicBoxMessage.SendMessageToAllClient(
                                               new MagicBoxMessage
                                               {
                                                   MessageType = MagicBoxMessageType.ProductTag,
                                               });

                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Error("Process Product Tags Message Handler", e);
                return false;
            }
        }
    }
}
