using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Products.Dtos;

namespace KonbiCloud.Products
{
    public interface IProductMachinePriceAppService : IApplicationService
    {
        Task<PagedResultDto<ProductDto>> GetProductMachinePrices(GetProductMachinePriceInput input);

        Task<bool> UpdateProductMachinePrices(UpdateProductMachinePriceInput input);
    }
}
