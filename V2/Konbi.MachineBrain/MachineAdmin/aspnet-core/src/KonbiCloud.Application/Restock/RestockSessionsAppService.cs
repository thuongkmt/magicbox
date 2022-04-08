

using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using KonbiCloud.Restock.Exporting;
using KonbiCloud.Restock.Dtos;
using KonbiCloud.Dto;
using Abp.Application.Services.Dto;
using KonbiCloud.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;

namespace KonbiCloud.Restock
{
	[AbpAuthorize(AppPermissions.Pages_RestockSessions)]
    public class RestockSessionsAppService : KonbiCloudAppServiceBase, IRestockSessionsAppService
    {
		 private readonly IRepository<RestockSession> _restockSessionRepository;
		 private readonly IRestockSessionsExcelExporter _restockSessionsExcelExporter;
		 

		  public RestockSessionsAppService(IRepository<RestockSession> restockSessionRepository, IRestockSessionsExcelExporter restockSessionsExcelExporter ) 
		  {
			_restockSessionRepository = restockSessionRepository;
			_restockSessionsExcelExporter = restockSessionsExcelExporter;
			
		  }

		 public async Task<PagedResultDto<GetRestockSessionForViewDto>> GetAll(GetAllRestockSessionsInput input)
         {
			
			var filteredRestockSessions = _restockSessionRepository.GetAll()
						.WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false  || e.RestockerName.Contains(input.Filter))
						.WhereIf(input.MinStartDateFilter != null, e => e.StartDate >= input.MinStartDateFilter)
						.WhereIf(input.MaxStartDateFilter != null, e => e.StartDate <= input.MaxStartDateFilter)
						.WhereIf(input.MinEndDateFilter != null, e => e.EndDate >= input.MinEndDateFilter)
						.WhereIf(input.MaxEndDateFilter != null, e => e.EndDate <= input.MaxEndDateFilter)
						.WhereIf(input.MinTotalFilter != null, e => e.Total >= input.MinTotalFilter)
						.WhereIf(input.MaxTotalFilter != null, e => e.Total <= input.MaxTotalFilter)
						.WhereIf(input.MinLeftOverFilter != null, e => e.LeftOver >= input.MinLeftOverFilter)
						.WhereIf(input.MaxLeftOverFilter != null, e => e.LeftOver <= input.MaxLeftOverFilter)
						.WhereIf(input.MinSoldFilter != null, e => e.Sold >= input.MinSoldFilter)
						.WhereIf(input.MaxSoldFilter != null, e => e.Sold <= input.MaxSoldFilter)
						.WhereIf(input.MinErrorFilter != null, e => e.Error >= input.MinErrorFilter)
						.WhereIf(input.MaxErrorFilter != null, e => e.Error <= input.MaxErrorFilter)
						.WhereIf(input.IsProcessingFilter > -1,  e => (input.IsProcessingFilter == 1 && e.IsProcessing) || (input.IsProcessingFilter == 0 && !e.IsProcessing) )
						.WhereIf(!string.IsNullOrWhiteSpace(input.RestockerNameFilter),  e => e.RestockerName == input.RestockerNameFilter);

			var pagedAndFilteredRestockSessions = filteredRestockSessions
                .OrderBy(input.Sorting ?? "id desc")
                .PageBy(input);

			var restockSessions = from o in pagedAndFilteredRestockSessions
                         select new GetRestockSessionForViewDto() {
							RestockSession = new RestockSessionDto
							{
                                StartDate = o.StartDate,
                                EndDate = o.EndDate,
                                Total = o.Total,
                                LeftOver = o.LeftOver,
                                Sold = o.Sold,
                                Error = o.Error,
                                IsProcessing = o.IsProcessing,
                                RestockerName = o.RestockerName,
                                Restocked = o.Restocked,
                                Unloaded = o.Unloaded,
                                Id = o.Id
							}
						};

            var totalCount = await filteredRestockSessions.CountAsync();

            return new PagedResultDto<GetRestockSessionForViewDto>(
                totalCount,
                await restockSessions.ToListAsync()
            );
         }
		 
		 public async Task<GetRestockSessionForViewDto> GetRestockSessionForView(int id)
         {
            var restockSession = await _restockSessionRepository.GetAsync(id);

            var output = new GetRestockSessionForViewDto { RestockSession = ObjectMapper.Map<RestockSessionDto>(restockSession) };
			
            return output;
         }
		 
		 [AbpAuthorize(AppPermissions.Pages_RestockSessions_Edit)]
		 public async Task<GetRestockSessionForEditOutput> GetRestockSessionForEdit(EntityDto input)
         {
            var restockSession = await _restockSessionRepository.FirstOrDefaultAsync(input.Id);
           
		    var output = new GetRestockSessionForEditOutput {RestockSession = ObjectMapper.Map<CreateOrEditRestockSessionDto>(restockSession)};
			
            return output;
         }

		 public async Task CreateOrEdit(CreateOrEditRestockSessionDto input)
         {
            if(input.Id == null){
				await Create(input);
			}
			else{
				await Update(input);
			}
         }

		 [AbpAuthorize(AppPermissions.Pages_RestockSessions_Create)]
		 protected virtual async Task Create(CreateOrEditRestockSessionDto input)
         {
            var restockSession = ObjectMapper.Map<RestockSession>(input);

			

            await _restockSessionRepository.InsertAsync(restockSession);
         }

		 [AbpAuthorize(AppPermissions.Pages_RestockSessions_Edit)]
		 protected virtual async Task Update(CreateOrEditRestockSessionDto input)
         {
            var restockSession = await _restockSessionRepository.FirstOrDefaultAsync((int)input.Id);
             ObjectMapper.Map(input, restockSession);
         }

		 [AbpAuthorize(AppPermissions.Pages_RestockSessions_Delete)]
         public async Task Delete(EntityDto input)
         {
            await _restockSessionRepository.DeleteAsync(input.Id);
         } 

		public async Task<FileDto> GetRestockSessionsToExcel(GetAllRestockSessionsForExcelInput input)
         {
			
			var filteredRestockSessions = _restockSessionRepository.GetAll()
						.WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false  || e.RestockerName.Contains(input.Filter))
						.WhereIf(input.MinStartDateFilter != null, e => e.StartDate >= input.MinStartDateFilter)
						.WhereIf(input.MaxStartDateFilter != null, e => e.StartDate <= input.MaxStartDateFilter)
						.WhereIf(input.MinEndDateFilter != null, e => e.EndDate >= input.MinEndDateFilter)
						.WhereIf(input.MaxEndDateFilter != null, e => e.EndDate <= input.MaxEndDateFilter)
						.WhereIf(input.MinTotalFilter != null, e => e.Total >= input.MinTotalFilter)
						.WhereIf(input.MaxTotalFilter != null, e => e.Total <= input.MaxTotalFilter)
						.WhereIf(input.MinLeftOverFilter != null, e => e.LeftOver >= input.MinLeftOverFilter)
						.WhereIf(input.MaxLeftOverFilter != null, e => e.LeftOver <= input.MaxLeftOverFilter)
						.WhereIf(input.MinSoldFilter != null, e => e.Sold >= input.MinSoldFilter)
						.WhereIf(input.MaxSoldFilter != null, e => e.Sold <= input.MaxSoldFilter)
						.WhereIf(input.MinErrorFilter != null, e => e.Error >= input.MinErrorFilter)
						.WhereIf(input.MaxErrorFilter != null, e => e.Error <= input.MaxErrorFilter)
						.WhereIf(input.IsProcessingFilter > -1,  e => (input.IsProcessingFilter == 1 && e.IsProcessing) || (input.IsProcessingFilter == 0 && !e.IsProcessing) )
						.WhereIf(!string.IsNullOrWhiteSpace(input.RestockerNameFilter),  e => e.RestockerName == input.RestockerNameFilter);

			var query = (from o in filteredRestockSessions
                         select new GetRestockSessionForViewDto() { 
							RestockSession = new RestockSessionDto
							{
                                StartDate = o.StartDate,
                                EndDate = o.EndDate,
                                Total = o.Total,
                                LeftOver = o.LeftOver,
                                Sold = o.Sold,
                                Error = o.Error,
                                IsProcessing = o.IsProcessing,
                                RestockerName = o.RestockerName,
                                Restocked = o.Restocked,
                                Unloaded = o.Unloaded,
                                Id = o.Id
							}
						 });


            var restockSessionListDtos = await query.ToListAsync();

            return _restockSessionsExcelExporter.ExportToFile(restockSessionListDtos);
         }


    }
}