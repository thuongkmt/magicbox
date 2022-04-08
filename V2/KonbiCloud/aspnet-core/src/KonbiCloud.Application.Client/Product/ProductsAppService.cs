using Abp.Application.Services.Dto;
using KonbiCloud.Dto;
using KonbiCloud.Products;
using KonbiCloud.Products.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.Product
{
    public class ProductsAppService : ProxyAppServiceBase, IProductsAppService
    {
        public async Task CreateOrEdit(CreateOrEditProductDto input)
        {
            throw new NotImplementedException();
        }

        public async Task Delete(EntityDto<Guid> input)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResultDto<ProductDto>> GetAll(GetAllProductsInput input)
        {
            return await ApiClient.GetAsync<PagedResultDto<ProductDto>>(GetEndpoint(nameof(GetAll)), input);
        }

        public async Task<GetProductForEditOutput> GetProductForEdit(EntityDto<Guid> input)
        {
            throw new NotImplementedException();
        }

        public async Task<ProductDto> GetProductForView(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<FileDto> GetProductsToExcel(GetAllProductsForExcelInput input)
        {
            throw new NotImplementedException();
        }
    }
}
