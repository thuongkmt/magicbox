using KonbiCloud.Plate;

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
using KonbiCloud.SignalR;
using Microsoft.EntityFrameworkCore;
using Abp.Domain.Uow;
using KonbiCloud.CloudSync;
using KonbiCloud.Machines;

namespace KonbiCloud.Plate
{
    [AbpAuthorize(AppPermissions.Pages_Discs)]
    public class DiscsAppService : KonbiCloudAppServiceBase, IDiscsAppService
    {
        private readonly IRepository<Disc, Guid> _discRepository;
        private readonly IDiscsExcelExporter _discsExcelExporter;
        private readonly IRepository<Plate, Guid> _plateRepository;
        private readonly IMessageCommunicator messageCommunicator;
        private readonly IRepository<DishMachineSyncStatus, Guid> _dishMachineSyncStatusRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;

        public DiscsAppService(IRepository<Disc, Guid> discRepository, IDiscsExcelExporter discsExcelExporter,
                               IRepository<Plate, Guid> plateRepository, IMessageCommunicator messageCommunicator,
                               IRepository<DishMachineSyncStatus, Guid> dishMachineSyncStatusRepository,
                               IRepository<Machine, Guid> machineRepository)
        {
            _discRepository = discRepository;
            _discsExcelExporter = discsExcelExporter;
            _plateRepository = plateRepository;
            this.messageCommunicator = messageCommunicator;
            _dishMachineSyncStatusRepository = dishMachineSyncStatusRepository;
            _machineRepository = machineRepository;
        }

        public async Task<PagedResultDto<GetDiscForView>> GetAll(GetAllDiscsInput input)
        {
            var filteredDiscs = _discRepository.GetAll()
                         .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Uid.Contains(input.Filter) || e.Code.Contains(input.Filter))
                         .WhereIf(!string.IsNullOrWhiteSpace(input.UidFilter), e => e.Uid.ToLower() == input.UidFilter.ToLower().Trim())
                         .WhereIf(!string.IsNullOrWhiteSpace(input.CodeFilter), e => e.Code.ToLower() == input.CodeFilter.ToLower().Trim())
                         .WhereIf(!string.IsNullOrWhiteSpace(input.PlateIdFilter), e => e.PlateId == new Guid(input.PlateIdFilter));


            var query = (from o in filteredDiscs
                         join o1 in _plateRepository.GetAll() on o.PlateId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         select new GetDiscForView()
                         {
                             Disc = ObjectMapper.Map<DiscDto>(o),
                             PlateName = s1 == null ? "" : s1.Name
                         });
                        //.WhereIf(!string.IsNullOrWhiteSpace(input.PlateNameFilter), e => e.PlateName.ToLower() == input.PlateNameFilter.ToLower().Trim());

            var totalCount = await query.CountAsync();

            var discs = await query
                .OrderBy(input.Sorting ?? "disc.id asc")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<GetDiscForView>(
                totalCount,
                discs
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Edit)]
        public async Task<GetDiscForEditOutput> GetDiscForEdit(DiscDto input)
        {
            var disc = await _discRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetDiscForEditOutput { Disc = ObjectMapper.Map<CreateOrEditDiscDto>(disc) };

            if (output.Disc.PlateId != null)
            {
                var plate = await _plateRepository.FirstOrDefaultAsync((Guid)output.Disc.PlateId);
                output.PlateName = plate.Name;
            }

            return output;
        }

        public async Task CreateOrEdit(List<CreateOrEditDiscDto> input)
        {
            //if(input.Id == null){
            await Create(input);
            //}
            //else{
            //	await Update(input);
            //}
        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Create)]
        private async Task Create(List<CreateOrEditDiscDto> input)
        {
            //TODO: need to find bulk insert DB
            foreach (var entity in input)
            {
                var disc = ObjectMapper.Map<Disc>(entity);
                if (AbpSession.TenantId != null)
                {
                    disc.TenantId = (int?)AbpSession.TenantId;
                }
                await _discRepository.InsertAsync(disc);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Edit)]
        private async Task Update(CreateOrEditDiscDto input)
        {
            var disc = await _discRepository.FirstOrDefaultAsync(input.Id.Value);
            ObjectMapper.Map(input, disc);
        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Delete)]
        public async Task Delete(DiscDto input)
        {
            await _discRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetDiscsToExcel(GetAllDiscsForExcelInput input)
        {

            var filteredDiscs = _discRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Uid.Contains(input.Filter) || e.Code.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UidFilter), e => e.Uid.ToLower() == input.UidFilter.ToLower().Trim())
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CodeFilter), e => e.Code.ToLower() == input.CodeFilter.ToLower().Trim());


            var query = (from o in filteredDiscs
                         join o1 in _plateRepository.GetAll() on o.PlateId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         select new GetDiscForView()
                         {
                             Disc = ObjectMapper.Map<DiscDto>(o),
                             PlateName = s1 == null ? "" : s1.Name
                         })
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PlateNameFilter), e => e.PlateName.ToLower() == input.PlateNameFilter.ToLower().Trim());


            var discListDtos = await query.ToListAsync();

            return _discsExcelExporter.ExportToFile(discListDtos);
        }


        [AbpAuthorize(AppPermissions.Pages_Discs)]
        public async Task<PagedResultDto<PlateLookupTableDto>> GetAllPlateForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _plateRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Name.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var plateList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<PlateLookupTableDto>();
            foreach (var plate in plateList)
            {
                lookupTableDtoList.Add(new PlateLookupTableDto
                {
                    Id = plate.Id.ToString(),
                    DisplayName = plate.Name,
                    Code = plate.Code
                });
            }

            return new PagedResultDto<PlateLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }


        public async Task TestSendSignalRMessage(List<CreateOrEditDiscDto> input)
        {
            var mess = "{\"type\":\"RFIDTable_DetectedDisc\",\"data\":[";
            int i = 0;
            foreach (var entity in input)
            {
                if (i == (input.Count - 1))
                {
                    mess += "{\"PlateCode\":\"" + entity.Code + "\",\"DiscUID\":\"" + entity.Uid + "\"}";
                }
                else
                {
                    mess += "{\"PlateCode\":\"" + entity.Code + "\",\"DiscUID\":\"" + entity.Uid + "\"},";
                }
                i++;
            }
            mess += "]}";
            await messageCommunicator.SendTestMessageToAllClient(new GeneralMessage { Message = mess });
            // await messageCommunicator.SendTestMessageToAllClient(new GeneralMessage { Message = "{\"type\":\"RFIDTable_DetectedDisc\",\"data\":[{\"code\":\"010203\",\"uid\":\"12345\"},{\"code\":\"010203\",\"uid\":\"98765\"}]}" });
        }

        //Reveive Dish from Machine
        [AbpAllowAnonymous]
        public async Task<bool> SyncDishData(SyncedItemData<Disc> syncData)
        {
            try
            {
                var existDishes = new List<Disc>();
                var plates = new List<Plate>();
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
                {
                    var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == syncData.MachineId);
                    if (machine == null)
                    {
                        Logger.Error($"Sync Dish: MachineId: {syncData.MachineId} does not exist");
                        return false;
                    }
                    else if (machine.IsDeleted)
                    {
                        Logger.Error($"Sync Dish: Machine with id: {syncData.MachineId} is deleted");
                        return false;
                    }
                    existDishes = await _discRepository.GetAllListAsync(x => x.TenantId == machine.TenantId);
                    plates = await _plateRepository.GetAllListAsync(x => x.TenantId == machine.TenantId);

                    var syncedList = new SyncedItemData<Guid> { MachineId = syncData.MachineId, SyncedItems = new List<Guid>() };
                    foreach (var d in syncData.SyncedItems)
                    {
                        if (d.PlateId != Guid.Empty)
                        {
                            var plate = plates.FirstOrDefault(x => x.Id == d.PlateId);
                            if (plate == null)
                                continue;
                        }
                        syncedList.SyncedItems.Add(d.Id);
                        if (d.Id != Guid.Empty)
                        {
                            var dish = await _discRepository.FirstOrDefaultAsync(x => x.Id.Equals(d.Id));
                            if (dish == null || dish.TenantId != machine.TenantId)
                            {
                                d.TenantId = AbpSession.TenantId;
                                await _discRepository.InsertAsync(d);
                            }
                            else
                            {
                                dish.IsDeleted = d.IsDeleted;
                                dish.TenantId = AbpSession.TenantId;
                                dish.Uid = d.Uid;
                                dish.Code = d.Code;
                                dish.PlateId = d.PlateId;
                            }
                        }
                        else
                        {
                            d.TenantId = AbpSession.TenantId;
                            await _discRepository.InsertAsync(d);
                        }
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();
                    await UpdateSyncStatus(syncedList);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return false;
            }
        }

        //Return dishes to machine
        [AbpAllowAnonymous]
        public async Task<IList<Disc>> GetDishes(EntityDto<Guid> machineInput)
        {
            try
            {
                var dishes = new List<Disc>();
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
                {
                    var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == machineInput.Id);
                    if (machine == null)
                    {
                        Logger.Error($"Sync Dish: MachineId: {machineInput.Id} does not exist");
                        return null;
                    }
                    else if (machine.IsDeleted)
                    {
                        Logger.Error($"Sync Dish: Machine with id: {machineInput.Id} is deleted");
                        return null;
                    }

                    var data = _discRepository.GetAllIncluding()
                                              .Where(x => x.TenantId == machine.TenantId)
                                              .Include(x => x.DishMachineSyncStatus);
                    if (data == null) return dishes;
                    foreach (var d in data)
                    {
                        var pm = d.DishMachineSyncStatus.FirstOrDefault(x => x.MachineId.Equals(machineInput.Id));
                        if (pm == null || d.LastModificationTime > pm.SyncDate || d.DeletionTime > pm.SyncDate)
                        {
                            dishes.Add(d);
                        }
                    }
                }
                return dishes;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
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
                        Logger.Error($"Sync Dish: MachineId: {syncData.MachineId} does not exist");
                        return false;
                    }
                    else if(machine.IsDeleted)
                    {
                        Logger.Error($"Sync Dish: Machine with id: {syncData.MachineId} is deleted");
                        return false;
                    }

                    var allPMs = await _dishMachineSyncStatusRepository.GetAllListAsync(x => x.MachineId == syncData.MachineId && x.TenantId == machine.TenantId);
                    foreach (var item in syncData.SyncedItems)
                    {
                        var pm = allPMs.FirstOrDefault(x => x.DiscId == item);
                        if (pm == null)
                        {
                            await _dishMachineSyncStatusRepository.InsertAsync(
                                new DishMachineSyncStatus
                                {
                                    Id = Guid.NewGuid(),
                                    DiscId = item,
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
                Logger.Error(ex.Message, ex);
                return false;
            }
        }
    }
}