
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using KonbiCloud.Authorization;
using KonbiCloud.CloudSync;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Dto;
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
        private readonly IPlateSyncService _plateSyncService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IDetailLogService _detailLogService;

        private string _serverUrl;
        public string ServerUrl
        {
            get
            {
                if(SettingManager != null && string.IsNullOrEmpty(_serverUrl))
                {
                    _serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                    if(_serverUrl == null)
                    {
                        _serverUrl = "";
                    }
                }
                return _serverUrl;
            }
        }

        public PlatesAppService(IRepository<Plate, Guid> plateRepository, IRepository<Disc, Guid> discRepository,
            IPlatesExcelExporter platesExcelExporter, IRepository<PlateCategory, int> plateCategoryRepository,
            IFileStorageService fileStorageService, IPlateSyncService plateSyncService,
            IUnitOfWorkManager unitOfWorkManager, IHostingEnvironment env, IDetailLogService detailLog)
        {
            _plateRepository = plateRepository;
            _platesExcelExporter = platesExcelExporter;
            _plateCategoryRepository = plateCategoryRepository;
            _fileStorageService = fileStorageService;
            _discRepository = discRepository;
            _plateSyncService = plateSyncService;
            _unitOfWorkManager = unitOfWorkManager;
            _appConfiguration = env.GetAppConfiguration();
            _detailLogService = detailLog;
        }

        public async Task<PagedResultDto<GetPlateForView>> GetAll(GetAllPlatesInput input)
        {
            try
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

                var pathImage = Path.Combine(ServerUrl, Const.ImageFolder, _appConfiguration[AppSettingNames.PlateImageFolder]);
                foreach (var p in plates)
                {
                    if (p.Plate != null && p.Plate.ImageUrl != null)
                    {
                        if (p.Plate.ImageUrl.Contains(Const.NoImage)) continue;

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
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new PagedResultDto<GetPlateForView>(0, new List<GetPlateForView>());
            }
        }


        [AbpAuthorize(AppPermissions.Pages_Plates_Edit)]
        public async Task<GetPlateForEditOutput> GetPlateForEdit(EntityDto<Guid> input)
        {
            try
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
                    var pathImage = Path.Combine(ServerUrl, Const.ImageFolder, _appConfiguration[AppSettingNames.PlateImageFolder]);

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
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new GetPlateForEditOutput();
            }
        }

        public async Task<PlateMessage> CreateOrEdit(CreateOrEditPlateDto input)
        {
            try
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
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new PlateMessage { Message = $"An error occurred, please try again." };
            }
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
            try
            {
                await _plateRepository.DeleteAsync(input.Id);
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }

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
            return _platesExcelExporter.ExportToFile(plateListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_Plates)]
        public async Task<PagedResultDto<PlateCategoryLookupTableDto>> GetAllPlateCategoryForLookupTable(GetAllForLookupTableInput input)
        {
            try
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
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new PagedResultDto<PlateCategoryLookupTableDto>(0, new List<PlateCategoryLookupTableDto>());
            }
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
                    SuccessCount = listSuccess.Count,
                    ErrorList = string.Join(", ", listError.Take(100).Select(x => x.ToString()).ToArray())
                };
                if (listError.Count > 100)
                {
                    result.ErrorList = string.Join(", ", listError.Take(100).Select(x => x.ToString()).ToArray()) + "...";
                }
                else
                {
                    result.ErrorList = string.Join(", ", listError.Take(100).Select(x => x.ToString()).ToArray());
                }
                return result;
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new ImportResult
                {
                    ErrorCount = 0,
                    SuccessCount = 0,
                    ErrorList = "Error"
                };
            }
        }

        [AbpAllowAnonymous]
        public async Task<bool> SyncPlateData()
        {
            var mId = Guid.Empty;
            Guid.TryParse(await SettingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);
            if (mId == Guid.Empty)
            {
                throw new UserFriendlyException("Machine configuration error");
            }

            var syncedList = new SyncedItemData<Guid> { MachineId = mId, SyncedItems = new List<Guid>() };
            var categories = new List<PlateCategory>();
            var existPlates = new List<Plate>();
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
            {
                categories = await _plateCategoryRepository.GetAllListAsync();
                existPlates = await _plateRepository.GetAllListAsync();
            }

            var plates = await _plateSyncService.Sync(mId);
            if (plates == null)
            {
                throw new UserFriendlyException("Cannot get Plates from server");
            }

            try
            {
                foreach (var p in plates)
                {
                    var ep = existPlates.FirstOrDefault(x => x.Id == p.Id);
                    if (ep == null)
                    {
                        if (p.PlateCategory != null && !categories.Any(x => x.Id == p.PlateCategory.Id))
                        {
                            await _plateCategoryRepository.InsertAsync(p.PlateCategory);
                            p.PlateCategory = null;
                        }
                        p.TenantId = AbpSession.TenantId;
                        await _plateRepository.InsertAsync(p);
                    }
                    else
                    {
                        ep.IsDeleted = p.IsDeleted;
                        ep.Name = p.Name;
                        ep.ImageUrl = p.ImageUrl;
                        ep.Desc = p.Desc;
                        ep.Code = p.Code;
                        ep.Avaiable = p.Avaiable;
                        ep.Color = p.Color;
                        ep.PlateCategoryId = p.PlateCategoryId;
                    }

                    syncedList.SyncedItems.Add(p.Id);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
                //Update sync status to server
                await _plateSyncService.UpdateSyncStatus(syncedList);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw new UserFriendlyException("Sync Plate failed");
            }
        }
    }
}