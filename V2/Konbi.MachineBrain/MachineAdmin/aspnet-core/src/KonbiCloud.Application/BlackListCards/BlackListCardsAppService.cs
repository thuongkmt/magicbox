using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using KonbiCloud.Authorization;
using KonbiCloud.BlackListCards.Dto;
using KonbiCloud.Configuration;
using KonbiCloud.Transactions;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;

namespace KonbiCloud.BlackListCards
{
    [AbpAllowAnonymous]
    public class BlackListCardsAppService : KonbiCloudAppServiceBase, IBlackListCardsAppService
    {
        private readonly IRepository<BlackListCard, long> _blackListCardRepository;
        private readonly string _machineId;

        public BlackListCardsAppService(IRepository<BlackListCard,long> blackListCardRepository, 
            ISettingManager settingManager)
        {
            _blackListCardRepository = blackListCardRepository;
            _machineId = settingManager.GetSettingValue(AppSettingNames.MachineId);
        }

        public async Task<List<BlackListCardDto>> GetAllItems()
        {
            var query = (
                            from blc in _blackListCardRepository.GetAll()
                            select new BlackListCardDto()
                            {
                                Id = blc.Id,
                                CardLabel = blc.CardLabel,
                                CardNumber = blc.CardNumber,
                                UnpaidAmount = blc.UnpaidAmount
                            }
                        );


            var result = await query.ToListAsync();

            var a = result;

            return result;
        }

        public async Task<PagedResultDto<BlackListCardDto>> GetAll(PagedAndSortedResultRequestDto input)
        {
            var query = (
                            from blc in _blackListCardRepository.GetAll()
                            select new BlackListCardDto()
                            {
                                Id = blc.Id,
                                CardLabel = blc.CardLabel,
                                CardNumber = blc.CardNumber,
                                UnpaidAmount = blc.UnpaidAmount
                            }
                        );

            var totalCount = await query.CountAsync();

            var result = await query.OrderBy(input.Sorting ?? "blackListCard.cardLabel asc")
                                               .PageBy(input)
                                               .ToListAsync();

            var a = result;

            return new PagedResultDto<BlackListCardDto>(
                totalCount,
                result
            );
        }

        public async Task<BlackListCardDto> GetBlackListCardForEdit(long id)
        {
            var item = await _blackListCardRepository.FirstOrDefaultAsync(id);

            if (item == null) return null;

            return new BlackListCardDto
            {
                Id = item.Id,
                CardLabel = item.CardLabel,
                CardNumber = item.CardNumber,
                UnpaidAmount = item.UnpaidAmount
            };
        }

        public async Task<BlackListCard> Save(BlackListCardDto input)
        {
            BlackListCard blackListCard;
            if (input.Id == null  || input.Id == 0)
            {
                blackListCard = await Create(input);
            }
            else
            {
                blackListCard = await Update(input);
            }

            return blackListCard;
        }

        private async Task<BlackListCard> Create(BlackListCardDto input)
        {
            var blackListCard = 
                                await _blackListCardRepository.InsertAsync(new BlackListCard
                                {
                                    CardLabel = input.CardLabel,
                                    CardNumber = input.CardNumber,
                                    UnpaidAmount = input.UnpaidAmount
                                });

            return blackListCard;
        }

        private async Task<BlackListCard> Update(BlackListCardDto input)
        {
            var item = await _blackListCardRepository.FirstOrDefaultAsync(input.Id ?? long.MaxValue);
            item.CardLabel = input.CardLabel;
            item.CardNumber = input.CardNumber;
            item.UnpaidAmount = input.UnpaidAmount;

            var blackListCard = await _blackListCardRepository.UpdateAsync(item);

            return blackListCard;
        }

        public async Task Delete(long id)
        {
            await _blackListCardRepository.DeleteAsync(id);
        }

        public async Task<BlackListCardDto> GetDetail(long id)
        {
            var item = await _blackListCardRepository.FirstOrDefaultAsync(id);

            if (item == null) return null;

            return new BlackListCardDto {
                CardLabel = item.CardLabel,
                CardNumber = item.CardNumber,
                UnpaidAmount = item.UnpaidAmount
            };
        }
    }
}
