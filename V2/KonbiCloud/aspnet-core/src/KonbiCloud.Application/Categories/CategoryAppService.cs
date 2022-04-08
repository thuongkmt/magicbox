//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Linq.Dynamic.Core;
//using System.Linq.Expressions;
//using System.Threading.Tasks;
//using Abp.Application.Services.Dto;
//using Abp.AutoMapper;
//using Abp.Domain.Repositories;
//using Abp.Runtime.Session;
//using Abp.UI;
//using KonbiCloud.Categories.Dtos;
//using KonbiCloud.Common;
//using KonbiCloud.Machines;
//using KonbiCloud.Machines.Dtos;
//using KonbiCloud.MultiTenancy;
//using KonbiCloud.Products;
//using Microsoft.EntityFrameworkCore;

//namespace KonbiCloud.Categories
//{
//    public class CategoryAppService : KonbiCloudAppServiceBase, ICategoryAppService
//    {
//        private readonly IRepository<Category, Guid> categoryRepository;
//        private readonly IFileStorageService fileStorageService;
//        private readonly IRepository<Machine, Guid> machineRepository;

//        public CategoryAppService(IRepository<Category, Guid> categoryRepository, IFileStorageService fileStorageService, IRepository<Tenant> tenantRepository, IRepository<Machine, Guid> machineRepository)
//        {
//            this.categoryRepository = categoryRepository;
//            this.fileStorageService = fileStorageService;
//            this.machineRepository = machineRepository;
//        }
//        public async Task<ListResultDto<CategoryListDto>> GetAll(GetCategoryListInput input)
//        {
//            try
//            {
//                var tenantId = AbpSession.TenantId ?? 0;
//                if (tenantId == 0 && input.MachineId.HasValue)
//                {
//                    var machine = await machineRepository.FirstOrDefaultAsync(x => x.Id == input.MachineId.Value);
//                    tenantId = machine?.TenantId ?? 0;
//                }
//                var allCategories = await categoryRepository.GetAllListAsync(x => x.TenantId == tenantId);
//                var categories = allCategories.OrderBy(e => e.Name).ToList();

//                return new ListResultDto<CategoryListDto>(categories.MapTo<List<CategoryListDto>>());
//            }
//            catch(Exception ex)
//            {
//                Logger.Error(ex.Message,ex);
//                return new ListResultDto<CategoryListDto>();
//            }
            
//        }

//        public async Task<Category> GetDetail(EntityDto<Guid> input)
//        {
//            var category = await categoryRepository.FirstOrDefaultAsync(e => e.Id == input.Id);

//            if (category == null)
//            {
//                throw new UserFriendlyException("Could not found the category, maybe it's deleted.");
//            }

//            return category;
//        }

//        public async Task Create(CreateCategoryInput input)
//        {
//            var newId = Guid.NewGuid();
//            if (!string.IsNullOrEmpty(input.ImageUrl))
//            {
//                var base64 = input.FileContent.Split(',')[1];
//                byte[] data = System.Convert.FromBase64String(base64);
//                using (MemoryStream ms = new MemoryStream(data))
//                {
//                    var fileType = Path.GetExtension(input.ImageUrl);
//                    var url = await fileStorageService.CreateOrReplace(newId.ToString(), fileType, ms);
//                    input.ImageUrl = url;
//                }

//            }
//            var cat = new Category()
//            {
//                Id = newId,
//                ImageUrl = input.ImageUrl,
//                TenantId = AbpSession.GetTenantId(), // temporal reserve                    
//                Name = input.Name
//            };
//            await categoryRepository.InsertAsync(cat);
//        }

//        public async Task<Category> Update(CategoryListDto input)
//        {
//            //CheckUpdatePermission();
//            if (!string.IsNullOrEmpty(input.FileContent))
//            {
//                var base64 = input.FileContent.Split(',')[1];
//                byte[] data = System.Convert.FromBase64String(base64);
//                using (MemoryStream ms = new MemoryStream(data))
//                {
//                    var fileType = Path.GetExtension(input.ImageUrl);
//                    string newImageFile = input.Id + DateTime.Now.Ticks.ToString();
//                    var url = await fileStorageService.CreateOrReplace(newImageFile, fileType, ms);
//                    input.ImageUrl = url;
//                }
//            }

//            var category = await categoryRepository.FirstOrDefaultAsync(e => e.Id == input.Id);

//            //ObjectMapper.Map(input, category);
//            category.Name = input.Name;
//            category.ImageUrl = input.ImageUrl;

//            await categoryRepository.UpdateAsync(category);
//            return category;
//        }

//        public  async Task Delete(EntityDto<Guid> input)
//        {
            
//            var category = await categoryRepository.FirstOrDefaultAsync(e => e.Id == input.Id);            
//            await categoryRepository.DeleteAsync(category);
//        }
//    }
//}
