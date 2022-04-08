using KonbiCloud.Credit;

using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using KonbiCloud.Credit.Exporting;
using KonbiCloud.Credit.Dtos;
using KonbiCloud.Dto;
using Abp.Application.Services.Dto;
using KonbiCloud.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;

namespace KonbiCloud.Credit
{
	[AbpAuthorize(AppPermissions.Pages_CreditHistories)]
    public class CreditHistoriesAppService : KonbiCloudAppServiceBase, ICreditHistoriesAppService
    {
		 private readonly IRepository<CreditHistory, Guid> _creditHistoryRepository;
		 private readonly ICreditHistoriesExcelExporter _creditHistoriesExcelExporter;
		 private readonly IRepository<UserCredit,Guid> _userCreditRepository;
		 

		  public CreditHistoriesAppService(IRepository<CreditHistory, Guid> creditHistoryRepository, ICreditHistoriesExcelExporter creditHistoriesExcelExporter , IRepository<UserCredit, Guid> userCreditRepository) 
		  {
			_creditHistoryRepository = creditHistoryRepository;
			_creditHistoriesExcelExporter = creditHistoriesExcelExporter;
			_userCreditRepository = userCreditRepository;
		
		  }

		 public async Task<PagedResultDto<GetCreditHistoryForViewDto>> GetAll(GetAllCreditHistoriesInput input)
         {
			
			var filteredCreditHistories = _creditHistoryRepository.GetAll()
						.WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false  || e.Message.Contains(input.Filter) || e.Hash.Contains(input.Filter))
						.WhereIf(input.MinValueFilter != null, e => e.Value >= input.MinValueFilter)
						.WhereIf(input.MaxValueFilter != null, e => e.Value <= input.MaxValueFilter)
						.WhereIf(!string.IsNullOrWhiteSpace(input.MessageFilter),  e => e.Message.ToLower() == input.MessageFilter.ToLower().Trim())
						.WhereIf(!string.IsNullOrWhiteSpace(input.HashFilter),  e => e.Hash.ToLower() == input.HashFilter.ToLower().Trim());


			var query = (from o in filteredCreditHistories
                         join o1 in _userCreditRepository.GetAll() on o.UserCreditId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()
                         
                         select new GetCreditHistoryForViewDto() {
							CreditHistory = ObjectMapper.Map<CreditHistoryDto>(o),
                         	UserCreditUserId = s1 == null ? "" : s1.UserId.ToString()
						})
						.WhereIf(!string.IsNullOrWhiteSpace(input.UserCreditUserIdFilter), e => e.UserCreditUserId.ToLower() == input.UserCreditUserIdFilter.ToLower().Trim());

            var totalCount = await query.CountAsync();

            var creditHistories = await query
                .OrderBy(input.Sorting ?? "creditHistory.id asc")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<GetCreditHistoryForViewDto>(
                totalCount,
                creditHistories
            );
         }
		 
		 public async Task<GetCreditHistoryForViewDto> GetCreditHistoryForView(Guid id)
         {
            var creditHistory = await _creditHistoryRepository.GetAsync(id);

            var output = new GetCreditHistoryForViewDto { CreditHistory = ObjectMapper.Map<CreditHistoryDto>(creditHistory) };

		    if (output.CreditHistory.UserCreditId != null)
            {
                var userCredit = await _userCreditRepository.FirstOrDefaultAsync((Guid)output.CreditHistory.UserCreditId);
                output.UserCreditUserId = userCredit.UserId.ToString();
            }
			
            return output;
         }
		 
		 [AbpAuthorize(AppPermissions.Pages_CreditHistories_Edit)]
		 public async Task<GetCreditHistoryForEditOutput> GetCreditHistoryForEdit(EntityDto<Guid> input)
         {
            var creditHistory = await _creditHistoryRepository.FirstOrDefaultAsync(input.Id);
           
		    var output = new GetCreditHistoryForEditOutput {CreditHistory = ObjectMapper.Map<CreateOrEditCreditHistoryDto>(creditHistory)};

		    if (output.CreditHistory.UserCreditId != null)
            {
                var userCredit = await _userCreditRepository.FirstOrDefaultAsync((Guid)output.CreditHistory.UserCreditId);
                output.UserCreditUserId = userCredit.UserId.ToString();
            }
			
            return output;
         }

		 public async Task CreateOrEdit(CreateOrEditCreditHistoryDto input)
         {
            if(input.Id == null){
				await Create(input);
			}
			else{
				await Update(input);
			}
         }

		 [AbpAuthorize(AppPermissions.Pages_CreditHistories_Create)]
		 private async Task Create(CreateOrEditCreditHistoryDto input)
         {
            var creditHistory = ObjectMapper.Map<CreditHistory>(input);

			

            await _creditHistoryRepository.InsertAsync(creditHistory);
         }

		 [AbpAuthorize(AppPermissions.Pages_CreditHistories_Edit)]
		 private async Task Update(CreateOrEditCreditHistoryDto input)
         {
            var creditHistory = await _creditHistoryRepository.FirstOrDefaultAsync((Guid)input.Id);
             ObjectMapper.Map(input, creditHistory);
         }

		 [AbpAuthorize(AppPermissions.Pages_CreditHistories_Delete)]
         public async Task Delete(EntityDto<Guid> input)
         {
            await _creditHistoryRepository.DeleteAsync(input.Id);
         } 

		public async Task<FileDto> GetCreditHistoriesToExcel(GetAllCreditHistoriesForExcelInput input)
         {
			
			var filteredCreditHistories = _creditHistoryRepository.GetAll()
						.WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false  || e.Message.Contains(input.Filter) || e.Hash.Contains(input.Filter))
						.WhereIf(input.MinValueFilter != null, e => e.Value >= input.MinValueFilter)
						.WhereIf(input.MaxValueFilter != null, e => e.Value <= input.MaxValueFilter)
						.WhereIf(!string.IsNullOrWhiteSpace(input.MessageFilter),  e => e.Message.ToLower() == input.MessageFilter.ToLower().Trim())
						.WhereIf(!string.IsNullOrWhiteSpace(input.HashFilter),  e => e.Hash.ToLower() == input.HashFilter.ToLower().Trim());


			var query = (from o in filteredCreditHistories
                         join o1 in _userCreditRepository.GetAll() on o.UserCreditId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()
                         
                         select new GetCreditHistoryForViewDto() { 
							CreditHistory = ObjectMapper.Map<CreditHistoryDto>(o),
                         	UserCreditUserId = s1 == null ? "" : s1.UserId.ToString()
						 })
						.WhereIf(!string.IsNullOrWhiteSpace(input.UserCreditUserIdFilter), e => e.UserCreditUserId.ToLower() == input.UserCreditUserIdFilter.ToLower().Trim());


            var creditHistoryListDtos = await query.ToListAsync();

            return _creditHistoriesExcelExporter.ExportToFile(creditHistoryListDtos);
         }



		[AbpAuthorize(AppPermissions.Pages_CreditHistories)]
         public async Task<PagedResultDto<UserCreditLookupTableDto>> GetAllUserCreditForLookupTable(GetAllForLookupTableInput input)
         {
             var query = _userCreditRepository.GetAll().WhereIf(
                    !string.IsNullOrWhiteSpace(input.Filter),
                   e=> e.UserId.ToString().Contains(input.Filter)
                );

            var totalCount = await query.CountAsync();

            var userCreditList = await query
                .PageBy(input)
                .ToListAsync();

			var lookupTableDtoList = new List<UserCreditLookupTableDto>();
			foreach(var userCredit in userCreditList){
				lookupTableDtoList.Add(new UserCreditLookupTableDto
				{
					Id = userCredit.Id.ToString(),
					DisplayName = userCredit.UserId?.ToString()
				});
			}

            return new PagedResultDto<UserCreditLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
         }
    }
}