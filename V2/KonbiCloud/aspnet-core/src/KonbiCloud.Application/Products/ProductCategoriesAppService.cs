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
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Konbini.Messages.Services;
using Abp.UI;
using System.Collections.Generic;
using KonbiCloud.Machines;

namespace KonbiCloud.Products
{
	[AbpAuthorize(AppPermissions.Pages_ProductCategories)]
    public class ProductCategoriesAppService : KonbiCloudAppServiceBase, IProductCategoriesAppService
    {
		private readonly IRepository<ProductCategory, Guid> _productCategoryRepository;
		private readonly IProductCategoriesExcelExporter _productCategoriesExcelExporter;
        private readonly IRepository<ProductCategoryRelation, Guid> _productCategoryRelationRepository;
        private readonly ISendMessageToMachineClientService _sendMessageToMachineService;
        private readonly IRepository<Machine, Guid> _machineRepository;

        public ProductCategoriesAppService(IRepository<ProductCategory, Guid> productCategoryRepository, 
            IProductCategoriesExcelExporter productCategoriesExcelExporter, 
            IRepository<ProductCategoryRelation, Guid> productCategoryRelationRepository,
            ISendMessageToMachineClientService sendMessageToMachineService,
            IRepository<Machine, Guid> machineRepository) 
		{
			_productCategoryRepository = productCategoryRepository;
			_productCategoriesExcelExporter = productCategoriesExcelExporter;
            _productCategoryRelationRepository = productCategoryRelationRepository;
            _sendMessageToMachineService = sendMessageToMachineService;
            _machineRepository = machineRepository;
        }

		 public async Task<PagedResultDto<ProductCategoryDto>> GetAll(GetAllProductCategoriesInput input)
         {
             var filteredProductCategories = _productCategoryRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false 
                        || (e.Name != null && e.Name.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase))
                        || (e.Code != null && e.Code.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase)) 
                        || (e.Desc != null && e.Desc.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase)))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name != null && e.Name.Contains(input.NameFilter.Trim(),StringComparison.OrdinalIgnoreCase))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CodeFilter), e => e.Code != null && e.Code.Contains(input.CodeFilter.Trim(),StringComparison.OrdinalIgnoreCase))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.DescFilter), e => e.Desc != null && e.Desc.Contains(input.DescFilter.Trim(),StringComparison.OrdinalIgnoreCase));

            var query = from o in filteredProductCategories select ObjectMapper.Map<ProductCategoryDto>(o);

            var totalCount = await query.CountAsync();

            var productCategories = await query
                .OrderBy(input.Sorting ?? "name asc")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<ProductCategoryDto>(
                totalCount,
                productCategories
            );
         }

        public async Task<ProductCategoryDto> GetProductCategoryForView(Guid id)
        {
            var productCategory = await _productCategoryRepository.GetAsync(id);
			
            return ObjectMapper.Map<ProductCategoryDto>(productCategory);
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
            if(input.Id == null)
            {
                productCategory = await Create(input);
			}
			else
            {
                productCategory = await Update(input);
			}

            int? tenantId = null;
            if (AbpSession.TenantId != null)
            {
                tenantId = (int?)AbpSession.TenantId;
            }

            var machines = _machineRepository.GetAll().Where(x => x.TenantId == tenantId);

            if (machines.Any())
            {
                foreach(var machine in machines)
                {
                    _sendMessageToMachineService.SendQueuedMsgToMachines(new KeyValueMessage
                    {
                        Key = MessageKeys.ProductCategory,
                        Value = new SyncProductCategoryDto {
                            Id = productCategory.Id,
                            Name = productCategory.Name,
                            Code = productCategory.Code,
                            Desc = productCategory.Desc
                        },
                        MachineId = machine.Id
                    }, CloudToMachineType.ToMachineId);
                }
            }
        }

		 [AbpAuthorize(AppPermissions.Pages_ProductCategories_Create)]
		 private async Task<ProductCategory> Create(CreateOrEditProductCategoryDto input)
         {
            var productCategory = ObjectMapper.Map<ProductCategory>(input);

			
			if (AbpSession.TenantId != null)
			{
				productCategory.TenantId = (int?) AbpSession.TenantId;
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

            int? tenantId = null;
            if (AbpSession.TenantId != null)
            {
                tenantId = (int?)AbpSession.TenantId;
            }

            var machines = _machineRepository.GetAll().Where(x => x.TenantId == tenantId);

            if (machines.Any())
            {
                foreach (var machine in machines)
                {
                    _sendMessageToMachineService.SendQueuedMsgToMachines(new KeyValueMessage
                    {
                        Key = MessageKeys.ProductCategory,
                        Value = new ProductCategory { Id = input.Id, IsDeleted = true },
                        MachineId = machine.Id
                    }, CloudToMachineType.ToMachineId);
                }
            }
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

            var query = from o in filteredProductCategories select ObjectMapper.Map<ProductCategoryDto>(o);

            return _productCategoriesExcelExporter.ExportToFile(await query.ToListAsync());
        }

        public async Task SyncProductCategoriesToMachine()
        {
            var categories = await _productCategoryRepository.GetAll().ToListAsync();
            var data = new List<ProductCategory>();

            foreach (var category in categories)
            {
                data.Add(new ProductCategory
                {
                    Id = category.Id,
                    Name = category.Name,
                    Code = category.Code,
                    Desc = category.Desc
                });
            }

            _sendMessageToMachineService.SendQueuedMsgToMachines(new KeyValueMessage
            {
                Key = MessageKeys.ManuallySyncProductCategory,
                Value = data
            }, CloudToMachineType.AllMachines);
        }
    }
}