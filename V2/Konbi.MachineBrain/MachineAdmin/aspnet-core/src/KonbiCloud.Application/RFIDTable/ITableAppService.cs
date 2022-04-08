using KonbiCloud.RFIDTable.Cache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;

namespace KonbiCloud.RFIDTable
{
    public interface ITableAppService:ITransientDependency
    {
        Task<SaleSessionCacheItem> GetSaleSessionInternalAsync();
        Task<long> GenerateTransactionAsync(TransactionInfo transactionInfo);
        string Validate(List<PlateReadingInput> plates, SaleSessionCacheItem cacheItem);
    }
}
