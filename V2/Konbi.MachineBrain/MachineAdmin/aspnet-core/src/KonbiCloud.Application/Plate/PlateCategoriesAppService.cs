
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using KonbiCloud.Plate.Exporting;
using KonbiCloud.Plate.Dtos;
using KonbiCloud.Dto;
using Abp.Application.Services.Dto;
using KonbiCloud.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using KonbiCloud.CloudSync;
using Abp.Domain.Uow;
using Abp.UI;
using KonbiCloud.Configuration;
using KonbiCloud.Common;

namespace KonbiCloud.Plate
{
    [AbpAuthorize(AppPermissions.Pages_PlateCategories)]
    public class PlateCategoriesAppService : KonbiCloudAppServiceBase, IPlateCategoriesAppService
    {
        private readonly IRepository<PlateCategory> _plateCategoryRepository;
        private readonly IRepository<Plate, Guid> _plateRepository;
        private readonly IPlateCategoriesExcelExporter _plateCategoriesExcelExporter;
        private readonly IPlateCategorySyncService _plateCategorySyncService;
        private readonly IDetailLogService _detailLogService;

        public PlateCategoriesAppService(IRepository<PlateCategory> plateCategoryRepository,
            IPlateCategoriesExcelExporter plateCategoriesExcelExporter,
            IRepository<Plate, Guid> plateRepository,
            IPlateCategorySyncService categorySyncService, IDetailLogService detailLog)
        {
            _plateCategoryRepository = plateCategoryRepository;
            _plateCategoriesExcelExporter = plateCategoriesExcelExporter;
            _plateRepository = plateRepository;
            _plateCategorySyncService = categorySyncService;
            _detailLogService = detailLog;
        }

        public async Task<PagedResultDto<GetPlateCategoryForView>> GetAll(GetAllPlateCategoriesInput input)
        {
            try
            {
                var filteredPlateCategories = _plateCategoryRepository.GetAllIncluding()
                            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Name.Contains(input.Filter) || e.Desc.Contains(input.Filter))
                            .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name.ToLower() == input.NameFilter.ToLower().Trim())
                            .WhereIf(!string.IsNullOrWhiteSpace(input.DescFilter), e => e.Desc.ToLower() == input.DescFilter.ToLower().Trim());
                //.Include(x => x.Plates);


                var query = (from o in filteredPlateCategories
                             select new GetPlateCategoryForView()
                             {
                                 PlateCategory = ObjectMapper.Map<PlateCategoryDto>(o)
                             });

                var totalCount = await query.CountAsync();

                var plateCategories = await query
                    .OrderBy(input.Sorting ?? "plateCategory.name")
                    .PageBy(input)
                    .ToListAsync();

                return new PagedResultDto<GetPlateCategoryForView>(
                    totalCount,
                    plateCategories
                );
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new PagedResultDto<GetPlateCategoryForView>(0, new List<GetPlateCategoryForView>());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateCategories_Edit)]
        public async Task<GetPlateCategoryForEditOutput> GetPlateCategoryForEdit(EntityDto input)
        {
            try
            {
                var plateCategory = await _plateCategoryRepository.FirstOrDefaultAsync(input.Id);

                var output = new GetPlateCategoryForEditOutput { PlateCategory = ObjectMapper.Map<CreateOrEditPlateCategoryDto>(plateCategory) };

                return output;
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new GetPlateCategoryForEditOutput();
            }
        }

        public async Task CreateOrEdit(CreateOrEditPlateCategoryDto input)
        {
            if (input.Id == null)
            {
                await Create(input);
            }
            else
            {
                await Update(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateCategories_Create)]
        private async Task Create(CreateOrEditPlateCategoryDto input)
        {
            try
            {
                var plateCategory = ObjectMapper.Map<PlateCategory>(input);
                if (AbpSession.TenantId != null)
                {
                    plateCategory.TenantId = (int?)AbpSession.TenantId;
                }
                await _plateCategoryRepository.InsertAsync(plateCategory);
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateCategories_Edit)]
        private async Task Update(CreateOrEditPlateCategoryDto input)
        {
            try
            {
                var plateCategory = await _plateCategoryRepository.FirstOrDefaultAsync((int)input.Id);
                ObjectMapper.Map(input, plateCategory);
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateCategories_Delete)]
        public async Task Delete(EntityDto input)
        {
            //check category has plate
            //var totalPlate = await _plateRepository.GetAll().Where(e => e.PlateCategoryId == input.Id).CountAsync();
            //if(totalPlate > 0)
            //{
            //    return "Can not delete category has plate";

            //}
            try
            {
                await _plateCategoryRepository.DeleteAsync(input.Id);
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
            //return "Delete success !";
        }

        public async Task<FileDto> GetPlateCategoriesToExcel(GetAllPlateCategoriesForExcelInput input)
        {

            var filteredPlateCategories = _plateCategoryRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Name.Contains(input.Filter) || e.Desc.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name.ToLower() == input.NameFilter.ToLower().Trim())
                        .WhereIf(!string.IsNullOrWhiteSpace(input.DescFilter), e => e.Desc.ToLower() == input.DescFilter.ToLower().Trim());


            var query = (from o in filteredPlateCategories
                         select new GetPlateCategoryForView()
                         {
                             PlateCategory = ObjectMapper.Map<PlateCategoryDto>(o)
                         });


            var plateCategoryListDtos = await query.ToListAsync();

            return _plateCategoriesExcelExporter.ExportToFile(plateCategoryListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_PlateCategories_Sync)]
        public async Task<bool> SyncPlateCategoryData()
        {
            var mId = Guid.Empty;
            Guid.TryParse(await SettingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);
            if (mId == Guid.Empty)
            {
                throw new UserFriendlyException("Machine configuration error");
            }
            var categories = await _plateCategorySyncService.Sync(mId);
            if (categories == null)
            {
                throw new UserFriendlyException("Cannot get Plate Category from server");
            }
            var existCategories = new List<PlateCategory>();
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                existCategories = await _plateCategoryRepository.GetAllListAsync();
            }
            
            try
            {
                foreach (var category in categories)
                {
                    var oldCategory = existCategories.FirstOrDefault(x => x.Id == category.Id);
                    if (oldCategory == null)
                    {
                        category.TenantId = AbpSession.TenantId;
                        await _plateCategoryRepository.InsertAsync(category);
                    }
                    else
                    {
                        oldCategory.IsDeleted = category.IsDeleted;
                        oldCategory.Name = category.Name;
                        oldCategory.Desc = category.Desc;
                        await _plateCategoryRepository.UpdateAsync(oldCategory);
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw new UserFriendlyException("Sync Plate Category failed");
            }
        }
    }
}