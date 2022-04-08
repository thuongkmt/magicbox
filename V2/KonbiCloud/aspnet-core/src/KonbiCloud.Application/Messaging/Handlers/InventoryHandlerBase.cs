using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using KonbiCloud.Inventories;
using KonbiCloud.Products;
using Konbini.Messages;
using Microsoft.EntityFrameworkCore;
using Abp.Timing;

namespace KonbiCloud.Messaging.Handlers
{
    public abstract class InventoryHandlerBase
    {
        protected readonly IRepository<InventoryItem, Guid> _inventoryRepository;
        protected readonly IRepository<Product, Guid> _productRepository;
        protected readonly IRepository<Topup, Guid> _topupRepository;



        protected InventoryHandlerBase(IRepository<InventoryItem, Guid> inventoryRepository, IRepository<Product, Guid> productRepository, IRepository<Topup, Guid> topupRepository)
        {
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _topupRepository = topupRepository;
        }

        protected async Task<bool> ProcessSingleInventory(Guid machineId, InventoryItem inventoryItem, Topup currentTopup, bool isUpdate = false)
        {
            inventoryItem.MachineId = machineId;
         

            if (currentTopup != null)
            {
                inventoryItem.Topup = currentTopup;
                //topup
                currentTopup.MachineId = inventoryItem.MachineId;
            }

            //product

            var existedproduct = await _productRepository.FirstOrDefaultAsync(x => x.Id == inventoryItem.ProductId);
            if(existedproduct == null)
            {
                return false;
            }
            inventoryItem.Product = existedproduct;

            //var product = inventoryItem.Product ?? await _productRepository.GetAsync(inventoryItem.ProductId);
            //if (product != null)
            //{
            //    var existedproduct = await _productRepository.GetAll().FirstOrDefaultAsync(x => x.Id == product.Id);
            //    if (existedproduct == null)
            //    {
            //        await _productRepository.InsertAsync(product);
            //    }
            //    else inventoryItem.Product = existedproduct;
            //}

            //inventoryItem table
            if (!isUpdate)
                await _inventoryRepository.InsertAsync(inventoryItem);
            else
            {
                var existed = await _inventoryRepository.FirstOrDefaultAsync(x => x.Id == inventoryItem.Id);
                if (existed == null) return false;
                existed.Topup = inventoryItem.Topup;
                await _inventoryRepository.UpdateAsync(existed);
            }
            return true;
        }

        protected async Task<Topup> ProcessInventoryTopup(Topup currentTopup, KeyValueMessage keyValueMessage)
        {

            var existedTopup = await _topupRepository.GetAll().FirstOrDefaultAsync(x => x.Id == currentTopup.Id);
            if (existedTopup == null)
            {
                var existedTopups =
                    await _topupRepository.GetAll().Where(x => x.MachineId == keyValueMessage.MachineId && x.EndDate == null)
                        .ToListAsync();
                foreach (var topup1 in existedTopups)
                {
                    topup1.EndDate = Clock.Now;
                    await _topupRepository.UpdateAsync(topup1);
                }

                await _topupRepository.InsertAsync(currentTopup);
            }
            else currentTopup = existedTopup;

            return currentTopup;
        }

    }
}
