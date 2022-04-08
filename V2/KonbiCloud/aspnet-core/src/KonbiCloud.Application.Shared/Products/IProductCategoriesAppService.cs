using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Products.Dtos;
using KonbiCloud.Dto;

namespace KonbiCloud.Products
{
    public interface IProductCategoriesAppService : IApplicationService 
    {
        Task<PagedResultDto<ProductCategoryDto>> GetAll(GetAllProductCategoriesInput input);

        Task<ProductCategoryDto> GetProductCategoryForView(Guid id);

		Task<GetProductCategoryForEditOutput> GetProductCategoryForEdit(EntityDto<Guid> input);

		Task CreateOrEdit(CreateOrEditProductCategoryDto input);

		Task Delete(EntityDto<Guid> input);

		Task<FileDto> GetProductCategoriesToExcel(GetAllProductCategoriesForExcelInput input);

		
    }
}