using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Products.Dtos;
using System;
using System.Threading.Tasks;

namespace KonbiCloud.Products
{
    public interface IProductAppService:IApplicationService
    {
        Task<ListResultDto<ProductDto>> GetAll(ProductListInput input);
        Task Create(CreateProductInput input);
        Task<ProductDto> GetDetail(EntityDto<Guid> input);
        Task Delete(EntityDto<Guid> input);
    }
}
