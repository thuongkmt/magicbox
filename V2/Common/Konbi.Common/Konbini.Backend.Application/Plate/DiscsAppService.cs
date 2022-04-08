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
using KonbiCloud.CloudSync;

namespace KonbiCloud.Plate
{
    [AbpAuthorize(AppPermissions.Pages_Discs)]
    public class DiscsAppService : KonbiCloudAppServiceBase, IDiscsAppService
    {
        private readonly IRepository<Disc> _discRepository;
        private readonly IDiscsExcelExporter _discsExcelExporter;
        private readonly IRepository<Plate, Guid> _plateRepository;
        private readonly IMessageCommunicator messageCommunicator;
        private readonly IDishSyncService dishSyncService;


        public DiscsAppService(IRepository<Disc> discRepository, IDiscsExcelExporter discsExcelExporter,
                               IRepository<Plate, Guid> plateRepository, IMessageCommunicator messageCommunicator,
                               IDishSyncService dishSyncService)
        {
            _discRepository = discRepository;
            _discsExcelExporter = discsExcelExporter;
            _plateRepository = plateRepository;
            this.messageCommunicator = messageCommunicator;
            this.dishSyncService = dishSyncService;
        }

        public async Task<PagedResultDto<GetDiscForView>> GetAll(GetAllDiscsInput input)
        {
            var filteredDiscs = _discRepository.GetAll()
                         //.WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Uid.Contains(input.Filter) || e.Code.Contains(input.Filter))
                         //.WhereIf(!string.IsNullOrWhiteSpace(input.UidFilter), e => e.Uid.ToLower() == input.UidFilter.ToLower().Trim())
                         //.WhereIf(!string.IsNullOrWhiteSpace(input.CodeFilter), e => e.Code.ToLower() == input.CodeFilter.ToLower().Trim())
                         .WhereIf(!string.IsNullOrWhiteSpace(input.PlateIdFilter), e => e.PlateId == new Guid(input.PlateIdFilter));


            var query = (from o in filteredDiscs
                         join o1 in _plateRepository.GetAll() on o.PlateId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         select new GetDiscForView()
                         {
                             Disc = ObjectMapper.Map<DiscDto>(o),
                             PlateName = s1 == null ? "" : s1.Name.ToString()
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
        public async Task<GetDiscForEditOutput> GetDiscForEdit(EntityDto input)
        {
            var disc = await _discRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetDiscForEditOutput { Disc = ObjectMapper.Map<CreateOrEditDiscDto>(disc) };

            if (output.Disc.PlateId != null)
            {
                var plate = await _plateRepository.FirstOrDefaultAsync((Guid)output.Disc.PlateId);
                output.PlateName = plate.Name.ToString();
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
                    disc.TenantId = AbpSession.TenantId;
                }
                try
                {
                    var success = await dishSyncService.Sync(disc);
                    if (success)
                    {
                        disc.IsSynced = true;
                        disc.SyncDate = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message, ex);
                }

                await _discRepository.InsertAsync(disc);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Edit)]
        private async Task Update(CreateOrEditDiscDto input)
        {
            var disc = await _discRepository.FirstOrDefaultAsync((int)input.Id);
            if(disc != null)
            {
                ObjectMapper.Map(input, disc);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Delete)]
        public async Task Delete(EntityDto input)
        {
            var disc = await _discRepository.FirstOrDefaultAsync(input.Id);
            if (disc != null)
            {
                await _discRepository.DeleteAsync(disc);
                await CurrentUnitOfWork.SaveChangesAsync();
                disc.IsDeleted = true;
                await dishSyncService.Sync(disc);
            }
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
                             PlateName = s1 == null ? "" : s1.Name.ToString()
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
                  e => e.Name.ToString().Contains(input.Filter)
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
                    DisplayName = plate.Name.ToString(),
                    Code = plate.Code.ToString()
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

    }
}