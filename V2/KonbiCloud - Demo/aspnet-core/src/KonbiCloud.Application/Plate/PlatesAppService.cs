using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using KonbiCloud.Authorization;
using KonbiCloud.CloudSync;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Dto;
using KonbiCloud.Machines;
using KonbiCloud.Plate.Dtos;
using KonbiCloud.Plate.Exporting;
using KonbiCloud.PlateMenus.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace KonbiCloud.Plate
{
    [AbpAuthorize(AppPermissions.Pages_Plates)]
    public class PlatesAppService : KonbiCloudAppServiceBase, IPlatesAppService
    {
        private readonly IRepository<Plate, Guid> _plateRepository;
        private readonly IPlatesExcelExporter _platesExcelExporter;
        private readonly IRepository<PlateCategory, int> _plateCategoryRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IRepository<Disc, Guid> _discRepository;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly string serverUrl;
        private const string noImage = "assets/common/images";
        private readonly IRepository<PlateMachineSyncStatus, Guid> _plateMachineSyncStatusRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;

        public PlatesAppService(IRepository<Plate, Guid> plateRepository, IRepository<Disc, Guid> discRepository,
            IPlatesExcelExporter platesExcelExporter, IRepository<PlateCategory, int> plateCategoryRepository,
            IFileStorageService fileStorageService, IHostingEnvironment env,
            IRepository<PlateMachineSyncStatus, Guid> plateMachineSyncStatusRepository,
            IRepository<Machine, Guid> machineRepository)
        {
            _plateRepository = plateRepository;
            _platesExcelExporter = platesExcelExporter;
            _plateCategoryRepository = plateCategoryRepository;
            _fileStorageService = fileStorageService;
            _discRepository = discRepository;
            _appConfiguration = env.GetAppConfiguration();
            serverUrl = _appConfiguration["App:ServerRootAddress"];
            if(serverUrl == null)
            {
                serverUrl = "";
            }
            _plateMachineSyncStatusRepository = plateMachineSyncStatusRepository;
            _machineRepository = machineRepository;
        }

        public async Task<PagedResultDto<GetPlateForView>> GetAll(GetAllPlatesInput input)
        {
            var filteredPlates = _plateRepository.GetAllIncluding()
                                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Name.Contains(input.Filter) || e.ImageUrl.Contains(input.Filter) || e.Desc.Contains(input.Filter) || e.Code.Contains(input.Filter) || e.Color.Contains(input.Filter))
                                .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name.ToLower() == input.NameFilter.ToLower().Trim())
                                .WhereIf(!string.IsNullOrWhiteSpace(input.ImageUrlFilter), e => e.ImageUrl.ToLower() == input.ImageUrlFilter.ToLower().Trim())
                                .WhereIf(!string.IsNullOrWhiteSpace(input.DescFilter), e => e.Desc.ToLower() == input.DescFilter.ToLower().Trim())
                                .WhereIf(!string.IsNullOrWhiteSpace(input.CodeFilter), e => e.Code.ToLower() == input.CodeFilter.ToLower().Trim())
                                .WhereIf(input.MinAvaiableFilter != null, e => e.Avaiable >= input.MinAvaiableFilter)
                                .WhereIf(input.MaxAvaiableFilter != null, e => e.Avaiable <= input.MaxAvaiableFilter)
                                .WhereIf(!string.IsNullOrWhiteSpace(input.ColorFilter), e => e.Color.ToLower() == input.ColorFilter.ToLower().Trim())
                                .Include(x => x.Discs);

            var query = (from o in filteredPlates
                         join o1 in _plateCategoryRepository.GetAll() on o.PlateCategoryId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         select new GetPlateForView()
                         {
                             Plate = ObjectMapper.Map<PlateDto>(o),
                             PlateCategoryName = s1 == null ? "" : s1.Name
                         })
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PlateCategoryNameFilter), e => e.PlateCategoryName.ToLower() == input.PlateCategoryNameFilter.ToLower().Trim());

            var totalCount = await query.CountAsync();

            var plates = await query
                .OrderBy(input.Sorting ?? "plate.name")
                .PageBy(input)
                .ToListAsync();

            var pathImage = Path.Combine(serverUrl, Const.ImageFolder, _appConfiguration[AppSettingNames.PlateImageFolder]);
            foreach (var p in plates)
            {
                if (p.Plate != null && p.Plate.ImageUrl != null)
                {
                    if (p.Plate.ImageUrl.Contains(noImage)) continue;

                    var arrImage = p.Plate.ImageUrl.Split("|");
                    var images = "";
                    for (int index = 0; index < arrImage.Length; index++)
                    {
                        if (index == arrImage.Length - 1)
                        {
                            images += Path.Combine(pathImage, arrImage[index]);
                        }
                        else
                        {
                            images += Path.Combine(pathImage, arrImage[index]) + '|';
                        }
                    }
                    p.Plate.ImageUrl = images;
                }
            }
            return new PagedResultDto<GetPlateForView>(
                totalCount,
                plates
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Plates_Edit)]
        public async Task<GetPlateForEditOutput> GetPlateForEdit(EntityDto<Guid> input)
        {
            var plate = await _plateRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetPlateForEditOutput { Plate = ObjectMapper.Map<CreateOrEditPlateDto>(plate) };

            if (output.Plate.PlateCategoryId != null)
            {
                var plateCategory = await _plateCategoryRepository.FirstOrDefaultAsync((int)output.Plate.PlateCategoryId);
                if (plateCategory != null)
                {
                    output.PlateCategoryName = plateCategory.Name;
                }
            }

            var dishes = _discRepository.GetAll()
                        .Where(e => e.PlateId == output.Plate.Id);
            output.Plate.Avaiable = await dishes.CountAsync();

            if (output.Plate.ImageUrl != null)
            {
                var pathImage = Path.Combine(serverUrl, Const.ImageFolder, _appConfiguration[AppSettingNames.PlateImageFolder]);

                var arrImage = output.Plate.ImageUrl.Split("|");
                var images = "";
                for (int index = 0; index < arrImage.Length; index++)
                {
                    if (index == arrImage.Length - 1)
                    {
                        images += Path.Combine(pathImage, arrImage[index]);
                    }
                    else
                    {
                        images += Path.Combine(pathImage, arrImage[index]) + '|';
                    }
                }
                output.Plate.ImageUrl = images;
            }

            return output;
        }

        public async Task<PlateMessage> CreateOrEdit(CreateOrEditPlateDto input)
        {
            if (input.Id == null)
            {
                var p = await _plateRepository.FirstOrDefaultAsync(x => x.Code.ToLower().Equals(input.Code.Trim().ToLower()));
                if (p != null)
                {
                    return new PlateMessage { Message = $"Plate code {input.Code} already existed, please use another code." };
                }
            }
            else
            {
                var p = await _plateRepository.FirstOrDefaultAsync(x => x.Id != input.Id.Value && x.Code.ToLower().Equals(input.Code.Trim().ToLower()));
                if (p != null)
                {
                    return new PlateMessage { Message = $"Plate code {input.Code} already existed, please use another code." };
                }
            }

            //if (!string.IsNullOrEmpty(input.ImageUrl) && input.ImageUrl.Contains("base64,"))
            //{
            //    if(input.Id != null)
            //    {
            //        var p = await _plateRepository.FirstOrDefaultAsync(x => x.Id == input.Id.Value);
            //        if(p != null)
            //        {
            //            _fileStorageService.DeleteFile(p.ImageUrl);
            //        }
            //    }
            //    var imgFolder = _appConfiguration[AppSettingNames.PlateImageFolder];
            //    input.ImageUrl = _fileStorageService.SaveImageToFolder(input.ImageUrl, imgFolder);
            //}

            if (input.Id == null)
            {
                await Create(input);
            }
            else
            {
                await Update(input);
            }
            return new PlateMessage { Message = null };
        }

        [AbpAuthorize(AppPermissions.Pages_Plates_Create)]
        private async Task Create(CreateOrEditPlateDto input)
        {
            var plate = ObjectMapper.Map<Plate>(input);

            if (AbpSession.TenantId != null)
            {
                plate.TenantId = AbpSession.TenantId;
            }

            await _plateRepository.InsertAsync(plate);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Plates_Edit)]
        private async Task Update(CreateOrEditPlateDto input)
        {
            if (input.ImageUrl != null)
            {
                var newPlateImage = "";
                var arrImage = input.ImageUrl.Split("|");
                for (int index = 0; index < arrImage.Length; index++)
                {
                    if (index == arrImage.Length - 1)
                    {
                        newPlateImage += Path.GetFileName(arrImage[index]);
                    }
                    else
                    {
                        newPlateImage += (Path.GetFileName(arrImage[index]) + '|');
                    }
                }
                input.ImageUrl = newPlateImage;
            }

            var plate = await _plateRepository.FirstOrDefaultAsync((Guid)input.Id);
            ObjectMapper.Map(input, plate);
        }

        [AbpAuthorize(AppPermissions.Pages_Plates_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            await _plateRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetPlatesToExcel(GetAllPlatesForExcelInput input)
        {

            var filteredPlates = _plateRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Name.Contains(input.Filter) || e.ImageUrl.Contains(input.Filter) || e.Desc.Contains(input.Filter) || e.Code.Contains(input.Filter) || e.Color.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name.ToLower() == input.NameFilter.ToLower().Trim())
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ImageUrlFilter), e => e.ImageUrl.ToLower() == input.ImageUrlFilter.ToLower().Trim())
                        .WhereIf(!string.IsNullOrWhiteSpace(input.DescFilter), e => e.Desc.ToLower() == input.DescFilter.ToLower().Trim())
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CodeFilter), e => e.Code.ToLower() == input.CodeFilter.ToLower().Trim())
                        .WhereIf(input.MinAvaiableFilter != null, e => e.Avaiable >= input.MinAvaiableFilter)
                        .WhereIf(input.MaxAvaiableFilter != null, e => e.Avaiable <= input.MaxAvaiableFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ColorFilter), e => e.Color.ToLower() == input.ColorFilter.ToLower().Trim());


            var query = (from o in filteredPlates
                         join o1 in _plateCategoryRepository.GetAll() on o.PlateCategoryId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         select new GetPlateForView()
                         {
                             Plate = ObjectMapper.Map<PlateDto>(o),
                             PlateCategoryName = s1 == null ? "" : s1.Name
                         })
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PlateCategoryNameFilter), e => e.PlateCategoryName.ToLower() == input.PlateCategoryNameFilter.ToLower().Trim());


            var plateListDtos = await query.ToListAsync();
            //foreach (var p in plateListDtos)
            //{
            //    if (p.Plate == null) continue;
            //    if (p.Plate.ImageUrl.Contains(oldImageUrl) || p.Plate.ImageUrl.Contains(noImage)) continue;
            //    p.Plate.ImageUrl = Path.Combine(serverUrl, p.Plate.ImageUrl);
            //}
            return _platesExcelExporter.ExportToFile(plateListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_Plates)]
        public async Task<PagedResultDto<PlateCategoryLookupTableDto>> GetAllPlateCategoryForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _plateCategoryRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Name.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var plateCategoryList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<PlateCategoryLookupTableDto>();
            foreach (var plateCategory in plateCategoryList)
            {
                lookupTableDtoList.Add(new PlateCategoryLookupTableDto
                {
                    Id = plateCategory.Id,
                    DisplayName = plateCategory.Name
                });
            }

            return new PagedResultDto<PlateCategoryLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        public async Task<ImportResult> ImportPlate(List<CreateOrEditPlateDto> input)
        {
            try
            {
                var listError = new List<string>();
                var listSuccess = new List<CreateOrEditPlateDto>();

                for (int i = 0; i < input.Count; i++)
                {
                    if (input[i].Code != null)
                    {
                        var p = await _plateRepository.FirstOrDefaultAsync(x => x.Code.Equals(input[i].Code.Trim()));
                        if (p == null)
                        {
                            var plate = ObjectMapper.Map<Plate>(input[i]);

                            if (plate.ImageUrl != null)
                            {
                                var newPlateImage = "";
                                var arrImage = plate.ImageUrl.Split("|");
                                for (int index = 0; index < arrImage.Length; index++)
                                {
                                    if (index == arrImage.Length - 1)
                                    {
                                        newPlateImage += Path.GetFileName(arrImage[index]);
                                    }
                                    else
                                    {
                                        newPlateImage += (Path.GetFileName(arrImage[index]) + '|');
                                    }
                                }
                                plate.ImageUrl = newPlateImage;
                            }

                            if (AbpSession.TenantId != null)
                            {
                                plate.TenantId = (int?)AbpSession.TenantId;
                            }
                            await _plateRepository.InsertAsync(plate);
                            listSuccess.Add(input[i]);
                        }
                        else
                        {
                            listError.Add("Row " + (i + 2) + "- plate code " + input[i].Code + " is available");
                            var errMess = "Import plate error: plate code " + input[i].Code + " is available";
                            Logger.Error(errMess);
                        }
                    }
                    else
                    {
                        listError.Add("Row " + (i + 2) + "- plate code null");
                    }
                }
                if (listSuccess.Any())
                {
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                var result = new ImportResult
                {
                    ErrorCount = listError.Count,
                    SuccessCount = listSuccess.Count
                };
                if (listError.Count > 100)
                {
                    result.ErrorList = string.Join(", ", listError.Take(100).Select(x => x.ToString()).ToArray()) + "...";
                }
                else
                {
                    result.ErrorList = string.Join(", ", listError.Select(x => x.ToString()).ToArray());
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Import Plate Error", ex);
                return new ImportResult
                {
                    ErrorCount = 0,
                    SuccessCount = 0,
                    ErrorList = "Error"
                };
            }
        }

        [AbpAllowAnonymous]
        public async Task<IList<Plate>> GetPlates(EntityDto<Guid> machineInput)
        {
            try
            {
                var plates = new List<Plate>();
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
                {
                    var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == machineInput.Id);
                    if (machine == null)
                    {
                        Logger.Error($"Sync Plate: MachineId: {machineInput.Id} does not exist");
                        return null;
                    }
                    else if (machine.IsDeleted)
                    {
                        Logger.Error($"Sync Plate: Machine with id: {machineInput.Id} is deleted");
                        return null;
                    }

                    var allPlates = _plateRepository.GetAllIncluding()
                                    .Where(x => x.TenantId == machine.TenantId)
                                    .Include(x => x.PlateCategory)
                                    .Include(x => x.PlateMachineSyncStatus);
                    if (allPlates == null) return plates;

                    foreach (var p in allPlates)
                    {
                        var pm = p.PlateMachineSyncStatus.FirstOrDefault(x => x.MachineId.Equals(machineInput.Id));
                        if (pm == null || p.LastModificationTime > pm.SyncDate || p.DeletionTime > pm.SyncDate)
                        {
                            plates.Add(new Plate
                            {
                                Id = p.Id,
                                Name = p.Name,
                                ImageUrl = p.ImageUrl,
                                Desc = p.Desc,
                                Code = p.Code,
                                Avaiable = p.Avaiable,
                                Color = p.Color,
                                PlateCategoryId = p.PlateCategoryId,
                                PlateCategory = p.PlateCategory == null ? null : new PlateCategory
                                {
                                    Id = p.PlateCategory.Id,
                                    Name = p.PlateCategory.Name,
                                    Desc = p.PlateCategory.Desc
                                },
                                IsDeleted = p.IsDeleted
                            });
                        }
                    }
                }
                return plates;
            }
            catch (Exception ex)
            {
                Logger.Error("Get Plate for Sync Error", ex);
                return null;
            }
        }

        [AbpAllowAnonymous]
        public async Task<bool> UpdateSyncStatus(SyncedItemData<Guid> syncData)
        {
            try
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
                {
                    var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == syncData.MachineId);
                    if (machine == null)
                    {
                        Logger.Error($"Sync Plate: MachineId: {syncData.MachineId} does not exist");
                        return false;
                    }
                    else if (machine.IsDeleted)
                    {
                        Logger.Error($"Sync Plate: Machine with id: {syncData.MachineId} is deleted");
                        return false;
                    }

                    var allPMs = await _plateMachineSyncStatusRepository.GetAllListAsync(x => x.TenantId == machine.TenantId &&
                                                                                  x.MachineId == syncData.MachineId);
                    foreach (var item in syncData.SyncedItems)
                    {
                        var pm = allPMs.FirstOrDefault(x => x.PlateId == item);
                        if (pm == null)
                        {
                            await _plateMachineSyncStatusRepository.InsertAsync(
                                new PlateMachineSyncStatus
                                {
                                    Id = Guid.NewGuid(),
                                    PlateId = item,
                                    MachineId = syncData.MachineId,
                                    IsSynced = true,
                                    SyncDate = DateTime.Now
                                });
                            continue;
                        }
                        pm.SyncDate = DateTime.Now;
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Update Plate Sync Status Error", ex);
                return false;
            }
            
        }
    }
}