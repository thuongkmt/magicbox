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
    public class ProductCategoriesAppService : ProxyAppServiceBase, IProductCategoriesAppService
    {
        public async Task CreateOrEdit(CreateOrEditProductCategoryDto input)
        {
            throw new NotImplementedException();
        }

        public async Task Delete(EntityDto<Guid> input)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResultDto<ProductCategoryDto>> GetAll(GetAllProductCategoriesInput input)
        {
            return await ApiClient.GetAsync<PagedResultDto<ProductCategoryDto>>(GetEndpoint(nameof(GetAll)), input);
        }

        public async Task<FileDto> GetProductCategoriesToExcel(GetAllProductCategoriesForExcelInput input)
        {
            throw new NotImplementedException();
        }

        public async Task<GetProductCategoryForEditOutput> GetProductCategoryForEdit(EntityDto<Guid> input)
        {
            throw new NotImplementedException();
        }

        public async Task<ProductCategoryDto> GetProductCategoryForView(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
