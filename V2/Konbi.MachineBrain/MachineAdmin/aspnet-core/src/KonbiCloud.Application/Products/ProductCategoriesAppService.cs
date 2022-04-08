
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using KonbiCloud.Products.Exporting;
using KonbiCloud.Products.Dtos;
using KonbiCloud.Dto;
using Abp.Application.Services.Dto;
using KonbiCloud.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Konbini.Messages.Services;
using Konbini.Messages;
using Konbini.Messages.Enums;
using KonbiCloud.Configuration;
using Abp.Configuration;
using Abp.UI;

namespace KonbiCloud.Products
{
	[AbpAuthorize(AppPermissions.Pages_ProductCategories)]
    public class ProductCategoriesAppService : KonbiCloudAppServiceBase, IProductCategoriesAppService
    {
        private readonly IRepository<ProductCategory, Guid> _productCategoryRepository;
        private readonly IProductCategoriesExcelExporter _productCategoriesExcelExporter;
        private readonly IRepository<ProductCategoryRelation, Guid> _productCategoryRelationRepository;
        private readonly ISendMessageToCloudService _sendMessageToCloudService;
        private readonly string _machineId;

        public ProductCategoriesAppService(IRepository<ProductCategory, Guid> productCategoryRepository, 
            IProductCategoriesExcelExporter productCategoriesExcelExporter, 
            IRepository<ProductCategoryRelation, Guid> productCategoryRelationRepository,
            ISendMessageToCloudService sendMessageToCloudService,
            ISettingManager settingManager)
        {
            _productCategoryRepository = productCategoryRepository;
            _productCategoriesExcelExporter = productCategoriesExcelExporter;
            _productCategoryRelationRepository = productCategoryRelationRepository;
            _sendMessageToCloudService = sendMessageToCloudService;
            _machineId = settingManager.GetSettingValue(AppSettingNames.MachineId);
        }

        public async Task<PagedResultDto<GetProductCategoryForViewDto>> GetAll(GetAllProductCategoriesInput input)
         {
            var filteredProductCategories = _productCategoryRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false 
                        || (e.Name != null && e.Name.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase))
                        || (e.Code != null && e.Code.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase))
                        || (e.Desc != null && e.Desc.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase)))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name != null && e.Name.Contains(input.NameFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CodeFilter), e => e.Code != null && e.Code.Contains(input.CodeFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.DescFilter), e => e.Desc != null && e.Desc.Contains(input.DescFilter.Trim(), StringComparison.OrdinalIgnoreCase));

            var query = (
                            from o in filteredProductCategories
                            select new GetProductCategoryForViewDto()
                            {
                                ProductCategory = ObjectMapper.Map<ProductCategoryDto>(o)
                            }
                        );

            var totalCount = await query.CountAsync();

            var productCategories = await query
                .OrderBy(input.Sorting ?? "productCategory.name asc")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<GetProductCategoryForViewDto>(
                totalCount,
                productCategories
            );
        }

        public async Task<GetProductCategoryForViewDto> GetProductCategoryForView(Guid id)
        {
            var productCategory = await _productCategoryRepository.GetAsync(id);

            var output = new GetProductCategoryForViewDto { ProductCategory = ObjectMapper.Map<ProductCategoryDto>(productCategory) };

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_ProductCategories_Edit)]
		 public async Task<GetProductCategoryForEditOutput> GetProductCategoryForEdit(EntityDto<Guid> input)
         {
            var productCategory = await _productCategoryRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetProductCategoryForEditOutput
            {
                ProductCategory = ObjectMapper.Map<CreateOrEditProductCategoryDto>(productCategory)
            };

            return output;
        }

		 public async Task CreateOrEdit(CreateOrEditProductCategoryDto input)
         {
            ProductCategory productCategory = null;
            if (input.Id == null)
            {
                productCategory = await Create(input);
            }
            else
            {
                productCategory = await Update(input);
            }
        }

		 [AbpAuthorize(AppPermissions.Pages_ProductCategories_Create)]
		 private async Task<ProductCategory> Create(CreateOrEditProductCategoryDto input)
         {
            var productCategory = ObjectMapper.Map<ProductCategory>(input);


            if (AbpSession.TenantId != null)
            {
                productCategory.TenantId = (int?)AbpSession.TenantId;
            }


            await _productCategoryRepository.InsertAsync(productCategory);

            return productCategory;
        }

		 [AbpAuthorize(AppPermissions.Pages_ProductCategories_Edit)]
		 private async Task<ProductCategory> Update(CreateOrEditProductCategoryDto input)
         {
            var productCategory = await _productCategoryRepository.FirstOrDefaultAsync((Guid)input.Id);
            ObjectMapper.Map(input, productCategory);

            return productCategory;
        }

		 [AbpAuthorize(AppPermissions.Pages_ProductCategories_Delete)]
         public async Task Delete(EntityDto<Guid> input)
         {
            var productCategoryRelations = _productCategoryRelationRepository.GetAll().
                                            Where(x => x.ProductCategoryId == input.Id);

            //foreach (var item in productCategoryRelations)
            //{
            //    await _productCategoryRelationRepository.DeleteAsync(item.Id);
            //}

            //Do not allow to delete category while it is being used by products
            if (productCategoryRelations.Any())
            {
                throw new UserFriendlyException("Can not delete this category because it is using by some products");
            }

            await _productCategoryRepository.DeleteAsync(input.Id);
        } 

		public async Task<FileDto> GetProductCategoriesToExcel(GetAllProductCategoriesForExcelInput input)
         {
            var filteredProductCategories = _productCategoryRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false 
                        || (e.Name != null && e.Name.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase))
                        || (e.Code != null && e.Code.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase))
                        || (e.Desc != null && e.Desc.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase)))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name != null && e.Name.Contains(input.NameFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CodeFilter), e => e.Code != null && e.Code.Contains(input.CodeFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.DescFilter), e => e.Desc != null && e.Desc.Contains(input.DescFilter.Trim(), StringComparison.OrdinalIgnoreCase));

            var query = (
                            from o in filteredProductCategories
                            select new GetProductCategoryForViewDto()
                            {
                                ProductCategory = ObjectMapper.Map<ProductCategoryDto>(o)
                            }
                        );

            var productCategoryListDtos = await query.ToListAsync();

            return _productCategoriesExcelExporter.ExportToFile(productCategoryListDtos);
        }

        public void SendSyncCategoriesRequest()
        {
            _sendMessageToCloudService.SendQueuedMsgToCloud(new KeyValueMessage()
            {
                Key = MessageKeys.ManuallySyncProductCategory,
                MachineId = Guid.Parse(_machineId),
            });
        }
    }
}