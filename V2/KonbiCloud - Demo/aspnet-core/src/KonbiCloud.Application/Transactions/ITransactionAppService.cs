using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Transactions.Dtos;
using System.Threading.Tasks;

namespace KonbiCloud.Transactions
{
    public interface ITransactionAppService : IApplicationService
    {
        Task<PagedResultDto<TransactionDto>> GetAllTransactions(TransactionInput input);
    }
}