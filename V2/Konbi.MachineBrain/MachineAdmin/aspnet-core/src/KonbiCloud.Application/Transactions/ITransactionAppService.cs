using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Transactions.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.Transactions
{
    public interface ITransactionAppService : IApplicationService 
    {
        Task<PagedResultDto<TransactionDto>> GetAllTransactions(TransactionInput input);
        Task UpdateSyncStatus(IList<long> tranIds);

        Task AddTransaction(FridgeTransactionDto transaction);
    }
}