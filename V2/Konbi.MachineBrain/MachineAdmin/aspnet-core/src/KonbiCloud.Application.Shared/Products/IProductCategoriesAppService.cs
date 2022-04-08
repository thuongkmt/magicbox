using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Products.Dtos;
using KonbiCloud.Dto;
using System.Collections.Generic;

namespace KonbiCloud.Products
{
    public interface IProductCategoriesAppService : IApplicationService
    {
        Task<PagedResultDto<GetProductCategoryForViewDto>> GetAll(GetAllProductCategoriesInput input);

        Task<GetProductCategoryForViewDto> GetProductCategoryForView(Guid id);

        Task<GetProductCategoryForEditOutput> GetProductCategoryForEdit(EntityDto<Guid> input);

        Task CreateOrEdit(CreateOrEditProductCategoryDto input);

        Task Delete(EntityDto<Guid> input);

        Task<FileDto> GetProductCategoriesToExcel(GetAllProductCategoriesForExcelInput input);

    }
}