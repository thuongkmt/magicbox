
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
using Abp.Domain.Uow;
using KonbiCloud.Machines;

namespace KonbiCloud.Plate
{
    [AbpAuthorize(AppPermissions.Pages_PlateCategories)]
    public class PlateCategoriesAppService : KonbiCloudAppServiceBase, IPlateCategoriesAppService
    {
        private readonly IRepository<PlateCategory> _plateCategoryRepository;
        private readonly IRepository<Plate, Guid> _plateRepository;
        private readonly IPlateCategoriesExcelExporter _plateCategoriesExcelExporter;
        private readonly IRepository<Machine, Guid> _machineRepository;

        public PlateCategoriesAppService(IRepository<PlateCategory> plateCategoryRepository,
            IPlateCategoriesExcelExporter plateCategoriesExcelExporter,
            IRepository<Plate, Guid> plateRepository,
            IRepository<Machine, Guid> machineRepository)
        {
            _plateCategoryRepository = plateCategoryRepository;
            _plateCategoriesExcelExporter = plateCategoriesExcelExporter;
            _plateRepository = plateRepository;
            _machineRepository = machineRepository;
        }

        public async Task<PagedResultDto<GetPlateCategoryForView>> GetAll(GetAllPlateCategoriesInput input)
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

        [AbpAuthorize(AppPermissions.Pages_PlateCategories_Edit)]
        public async Task<GetPlateCategoryForEditOutput> GetPlateCategoryForEdit(EntityDto input)
        {
            var plateCategory = await _plateCategoryRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetPlateCategoryForEditOutput { PlateCategory = ObjectMapper.Map<CreateOrEditPlateCategoryDto>(plateCategory) };

            return output;
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
            var plateCategory = ObjectMapper.Map<PlateCategory>(input);


            if (AbpSession.TenantId != null)
            {
                plateCategory.TenantId = (int?)AbpSession.TenantId;
            }


            await _plateCategoryRepository.InsertAsync(plateCategory);
        }

        [AbpAuthorize(AppPermissions.Pages_PlateCategories_Edit)]
        private async Task Update(CreateOrEditPlateCategoryDto input)
        {
            var plateCategory = await _plateCategoryRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, plateCategory);
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
            await _plateCategoryRepository.DeleteAsync(input.Id);
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

        [AbpAllowAnonymous]
        public async Task<IList<PlateCategory>> GetCategories(EntityDto<Guid> machineInput)
        {
            var plateCategories = new List<PlateCategory>();
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
            {
                var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == machineInput.Id);
                if (machine == null)
                {
                    Logger.Error($"Sync Plate Category: MachineId: {machineInput.Id} does not exist");
                    return null;
                }
                else if (machine.IsDeleted)
                {
                    Logger.Error($"Sync Plate Category: Machine with id: {machineInput.Id} is deleted");
                    return null;
                }

                plateCategories = await _plateCategoryRepository.GetAllListAsync(x => x.TenantId == machine.TenantId);
            }
            return plateCategories;
        }
    }
}