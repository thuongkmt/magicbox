using Abp.Application.Services.Dto;
using KonbiCloud.Products;
using KonbiCloud.Products.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.Product
{
    public class ProductTagsAppService : ProxyAppServiceBase, IProductTagsAppService
    {
        public async Task Delete(EntityDto<Guid> input)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResultDto<GetProductTagForViewDto>> GetAll(GetAllProductTagsInput input)
        {
            return await ApiClient.GetAsync<PagedResultDto<GetProductTagForViewDto>>(GetEndpoint(nameof(GetAll)), input);
        }

        public Task<PagedResultDto<ProductTagForReportDto>> GetAllForReport(GetAllProductTagsInput input)
        {
            throw new NotImplementedException();
        }

        public async Task InsertTags(ListProductTagDto input)
        {
            await ApiClient.PostAsync(GetEndpoint(nameof(InsertTags)), input);
        }
    }
}
