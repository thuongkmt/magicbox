using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Transactions.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.Transactions
{
    public interface ITransactionDetailsAppService: IApplicationService
    {
        Task<PagedResultDto<TransactionDetailDto>> GetTransactionDetailForView(long TransactionId);
    }
}
