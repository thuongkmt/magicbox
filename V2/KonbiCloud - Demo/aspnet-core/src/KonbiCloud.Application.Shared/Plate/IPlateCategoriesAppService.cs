using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Plate.Dtos;
using KonbiCloud.Dto;

namespace KonbiCloud.Plate
{
    public interface IPlateCategoriesAppService : IApplicationService 
    {
        Task<PagedResultDto<GetPlateCategoryForView>> GetAll(GetAllPlateCategoriesInput input);

		Task<GetPlateCategoryForEditOutput> GetPlateCategoryForEdit(EntityDto input);

		Task CreateOrEdit(CreateOrEditPlateCategoryDto input);

		Task Delete(EntityDto input);

		Task<FileDto> GetPlateCategoriesToExcel(GetAllPlateCategoriesForExcelInput input);

		
    }
}