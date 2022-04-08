using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Products.Dtos;
using KonbiCloud.Dto;

namespace KonbiCloud.Products
{
    public interface IProductsAppService : IApplicationService 
    {
        Task<PagedResultDto<GetProductForViewDto>> GetAll(GetAllProductsInput input);

        Task<GetProductForViewDto> GetProductForView(Guid id);

		Task<GetProductForEditOutput> GetProductForEdit(EntityDto<Guid> input);

		Task CreateOrEdit(CreateOrEditProductDto input);

		Task Delete(EntityDto<Guid> input);

		Task<FileDto> GetProductsToExcel(GetAllProductsForExcelInput input);

		
    }
}