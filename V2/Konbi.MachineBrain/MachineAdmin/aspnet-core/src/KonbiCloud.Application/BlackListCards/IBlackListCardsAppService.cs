using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.BlackListCards.Dto;
using KonbiCloud.Transactions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.BlackListCards
{
    public interface IBlackListCardsAppService : IApplicationService
    {
        Task<PagedResultDto<BlackListCardDto>> GetAll(PagedAndSortedResultRequestDto input);
        Task<BlackListCard> Save(BlackListCardDto input);
        Task<BlackListCardDto> GetDetail(long id);
        Task Delete(long id);
    }
}
