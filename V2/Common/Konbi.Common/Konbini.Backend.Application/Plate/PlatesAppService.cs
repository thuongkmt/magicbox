using KonbiCloud.Plate;

using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using KonbiCloud.Plate.Exporting;
using KonbiCloud.Plate.Dtos;
using KonbiCloud.Dto;
using Abp.Application.Services.Dto;
using KonbiCloud.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using System.IO;
using KonbiCloud.Common;
using System.ComponentModel.DataAnnotations;
using Abp.UI;

namespace KonbiCloud.Plate
{
	[AbpAuthorize(AppPermissions.Pages_Plates)]
    public class PlatesAppService : KonbiCloudAppServiceBase, IPlatesAppService
    {
		 private readonly IRepository<Plate, Guid> _plateRepository;
		 private readonly IPlatesExcelExporter _platesExcelExporter;
		 private readonly IRepository<PlateCategory,int> _plateCategoryRepository;
         private readonly IFileStorageService _fileStorageService;
         private readonly IRepository<Disc> _discRepository;

        public PlatesAppService(IRepository<Plate, Guid> plateRepository, IRepository<Disc> discRepository, IPlatesExcelExporter platesExcelExporter , IRepository<PlateCategory, int> plateCategoryRepository, IFileStorageService fileStorageService) 
		  {
			_plateRepository = plateRepository;
			_platesExcelExporter = platesExcelExporter;
			_plateCategoryRepository = plateCategoryRepository;
            _fileStorageService = fileStorageService;
            _discRepository = discRepository;
        }

		 public async Task<PagedResultDto<GetPlateForView>> GetAll(GetAllPlatesInput input)
         {
			
			var filteredPlates = _plateRepository.GetAllIncluding()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false  || e.Name.Contains(input.Filter) || e.ImageUrl.Contains(input.Filter) || e.Desc.Contains(input.Filter) || e.Code.Contains(input.Filter) || e.Color.Contains(input.Filter))
						.WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter),  e => e.Name.ToLower() == input.NameFilter.ToLower().Trim())
						.WhereIf(!string.IsNullOrWhiteSpace(input.ImageUrlFilter),  e => e.ImageUrl.ToLower() == input.ImageUrlFilter.ToLower().Trim())
						.WhereIf(!string.IsNullOrWhiteSpace(input.DescFilter),  e => e.Desc.ToLower() == input.DescFilter.ToLower().Trim())
						.WhereIf(!string.IsNullOrWhiteSpace(input.CodeFilter),  e => e.Code.ToLower() == input.CodeFilter.ToLower().Trim())
						.WhereIf(input.MinAvaiableFilter != null, e => e.Avaiable >= input.MinAvaiableFilter)
						.WhereIf(input.MaxAvaiableFilter != null, e => e.Avaiable <= input.MaxAvaiableFilter)
						.WhereIf(!string.IsNullOrWhiteSpace(input.ColorFilter),  e => e.Color.ToLower() == input.ColorFilter.ToLower().Trim())
                        .Include(x => x.Discs);

            var query = (from o in filteredPlates
                         join o1 in _plateCategoryRepository.GetAll() on o.PlateCategoryId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()
                         
                         select new GetPlateForView() {
							Plate = ObjectMapper.Map<PlateDto>(o),
                         	PlateCategoryName = s1 == null ? "" : s1.Name.ToString()
						})
						.WhereIf(!string.IsNullOrWhiteSpace(input.PlateCategoryNameFilter), e => e.PlateCategoryName.ToLower() == input.PlateCategoryNameFilter.ToLower().Trim());

            var totalCount = await query.CountAsync();

            var plates = await query
                .OrderBy(input.Sorting ?? "plate.name")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<GetPlateForView>(
                totalCount,
                plates
            );
         }
		 
		 [AbpAuthorize(AppPermissions.Pages_Plates_Edit)]
		 public async Task<GetPlateForEditOutput> GetPlateForEdit(EntityDto<Guid> input)
         {
            var plate = await _plateRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetPlateForEditOutput { Plate = ObjectMapper.Map<CreateOrEditPlateDto>(plate) };

            if (output.Plate.PlateCategoryId != null)
            {
                var plateCategory = await _plateCategoryRepository.FirstOrDefaultAsync((int)output.Plate.PlateCategoryId);
                output.PlateCategoryName = plateCategory.Name.ToString();
            }

            var dishes = _discRepository.GetAll()
                        .Where(e => e.PlateId == output.Plate.Id);
            output.Plate.Avaiable = await dishes.CountAsync();

            return output;
        }

		 public async Task<PlateMessage> CreateOrEdit(CreateOrEditPlateDto input)
         {
            if (input.Id == null)
            {
                var p = await _plateRepository.FirstOrDefaultAsync(x => x.Code.ToLower().Equals(input.Code.Trim().ToLower()));
                if (p != null)
                {
                    return new PlateMessage { Message = $"Plate code {input.Code} already existed, please use another code." };
                }
            }
            else
            {
                var p = await _plateRepository.FirstOrDefaultAsync(x => x.Id != input.Id.Value && x.Code.ToLower().Equals(input.Code.Trim().ToLower()));
                if (p != null)
                {
                    return new PlateMessage { Message = $"Plate code {input.Code} already existed, please use another code." };
                }
            }

            if (!string.IsNullOrEmpty(input.ImageUrl) && input.ImageUrl.Contains("base64,"))
            {
                var newId = Guid.NewGuid();
                var base64Arr = input.ImageUrl.Split(',');
                var base64 = base64Arr[1];
                byte[] data = Convert.FromBase64String(base64);
                using (var ms = new MemoryStream(data))
                {
                    var fileType = ".jpg"; //TODO: get ext from base64 - Path.GetExtension(input.ImageUrl);
                    var url = await _fileStorageService.CreateOrReplace(newId.ToString(), fileType, ms);
                    input.ImageUrl = url;
                    //input.ImageChecksum = base64.WeakHash();
                }
            }

            if (input.Id == null){
				await Create(input);
			}
			else{
				await Update(input);
			}
            return new PlateMessage { Message = null};
        }

		 [AbpAuthorize(AppPermissions.Pages_Plates_Create)]
		 private async Task Create(CreateOrEditPlateDto input)
         {
            var plate = ObjectMapper.Map<Plate>(input);

			if (AbpSession.TenantId != null)
			{
				plate.TenantId = AbpSession.TenantId;
			}

            await _plateRepository.InsertAsync(plate);
         }

		 [AbpAuthorize(AppPermissions.Pages_Plates_Edit)]
		 private async Task Update(CreateOrEditPlateDto input)
         {
            var plate = await _plateRepository.FirstOrDefaultAsync((Guid)input.Id);
             ObjectMapper.Map(input, plate);
         }

		 [AbpAuthorize(AppPermissions.Pages_Plates_Delete)]
         public async Task Delete(EntityDto<Guid> input)
         {
            await _plateRepository.DeleteAsync(input.Id);
         } 

		public async Task<FileDto> GetPlatesToExcel(GetAllPlatesForExcelInput input)
         {
			
			var filteredPlates = _plateRepository.GetAll()
						.WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false  || e.Name.Contains(input.Filter) || e.ImageUrl.Contains(input.Filter) || e.Desc.Contains(input.Filter) || e.Code.Contains(input.Filter) || e.Color.Contains(input.Filter))
						.WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter),  e => e.Name.ToLower() == input.NameFilter.ToLower().Trim())
						.WhereIf(!string.IsNullOrWhiteSpace(input.ImageUrlFilter),  e => e.ImageUrl.ToLower() == input.ImageUrlFilter.ToLower().Trim())
						.WhereIf(!string.IsNullOrWhiteSpace(input.DescFilter),  e => e.Desc.ToLower() == input.DescFilter.ToLower().Trim())
						.WhereIf(!string.IsNullOrWhiteSpace(input.CodeFilter),  e => e.Code.ToLower() == input.CodeFilter.ToLower().Trim())
						.WhereIf(input.MinAvaiableFilter != null, e => e.Avaiable >= input.MinAvaiableFilter)
						.WhereIf(input.MaxAvaiableFilter != null, e => e.Avaiable <= input.MaxAvaiableFilter)
						.WhereIf(!string.IsNullOrWhiteSpace(input.ColorFilter),  e => e.Color.ToLower() == input.ColorFilter.ToLower().Trim());


			var query = (from o in filteredPlates
                         join o1 in _plateCategoryRepository.GetAll() on o.PlateCategoryId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()
                         
                         select new GetPlateForView() { 
							Plate = ObjectMapper.Map<PlateDto>(o),
                         	PlateCategoryName = s1 == null ? "" : s1.Name.ToString()
						 })
						.WhereIf(!string.IsNullOrWhiteSpace(input.PlateCategoryNameFilter), e => e.PlateCategoryName.ToLower() == input.PlateCategoryNameFilter.ToLower().Trim());


            var plateListDtos = await query.ToListAsync();

            return _platesExcelExporter.ExportToFile(plateListDtos);
         }



		[AbpAuthorize(AppPermissions.Pages_Plates)]
         public async Task<PagedResultDto<PlateCategoryLookupTableDto>> GetAllPlateCategoryForLookupTable(GetAllForLookupTableInput input)
         {
             var query = _plateCategoryRepository.GetAll().WhereIf(
                    !string.IsNullOrWhiteSpace(input.Filter),
                   e=> e.Name.ToString().Contains(input.Filter)
                );

            var totalCount = await query.CountAsync();

            var plateCategoryList = await query
                .PageBy(input)
                .ToListAsync();

			var lookupTableDtoList = new List<PlateCategoryLookupTableDto>();
			foreach(var plateCategory in plateCategoryList){
				lookupTableDtoList.Add(new PlateCategoryLookupTableDto
				{
					Id = plateCategory.Id,
					DisplayName = plateCategory.Name.ToString()
				});
			}

            return new PagedResultDto<PlateCategoryLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
         }

        public async Task ImportPlate(List<CreateOrEditPlateDto> input)
        {
            try
            {
                for (int i = 0; i < input.Count; i++)
                {
                    if (input[i].Code != null)
                    {
                        var p = await _plateRepository.FirstOrDefaultAsync(x => x.Code.Equals(input[i].Code.Trim()));
                        if (p == null)
                        {
                            var plate = ObjectMapper.Map<Plate>(input[i]);
                            await _plateRepository.InsertAsync(plate);
                        }
                        else
                        {
                            var errMess = "Import plate error: plate code " + input[i].Code + " is available";
                            Logger.Error(errMess);
                        }
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.Error("Import Plate Error", ex);
            }
        }
    }
}