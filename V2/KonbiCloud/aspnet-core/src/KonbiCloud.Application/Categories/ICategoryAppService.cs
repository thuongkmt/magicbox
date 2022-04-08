//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Abp.Application.Services;
//using Abp.Application.Services.Dto;
//using KonbiCloud.Categories.Dtos;
//using KonbiCloud.Products;

//namespace KonbiCloud.Categories
//{
//    public interface ICategoryAppService : IApplicationService
//    {
//        Task<ListResultDto<CategoryListDto>> GetAll(GetCategoryListInput input);
//        Task<Category> GetDetail(EntityDto<Guid> input);
//        Task Create(CreateCategoryInput input);
//        Task<Category> Update(CategoryListDto input);
//        Task Delete(EntityDto<Guid> input);
//    }
//}
