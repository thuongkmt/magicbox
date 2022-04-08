using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Products.Dtos;
using System;
using System.Threading.Tasks;

namespace KonbiCloud.Products
{
    public interface IProductTagsAppService : IApplicationService
    {
        Task<PagedResultDto<GetProductTagForViewDto>> GetAll(GetAllProductTagsInput input);
        Task Delete(EntityDto<Guid> input);
        Task InsertTags(ListProductTagDto input);

        Task<PagedResultDto<ProductTagForReportDto>> GetAllForReport(GetAllProductTagsInput input);

         
    }
}
