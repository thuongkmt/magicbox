using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using KonbiCloud.Diagnostic.Dtos;
using KonbiCloud.Machines;
using KonbiCloud.Utils;
using Microsoft.EntityFrameworkCore;

namespace KonbiCloud.Diagnostic
{
    public class MachineDiagnosticAppService : KonbiCloudAppServiceBase, IMachineDiagnosticAppService
    {
        private readonly IRepository<VendingHistory, long> vendingHistoryRepository;
        private readonly IRepository<Machine, Guid> machiRepository;
        private readonly IRepository<MachineError> machineErrorRepository;
        private readonly IRepository<VendingStatus> vendingStatusRepository;

        public MachineDiagnosticAppService(IRepository<VendingHistory, long> vendingHistoryRepository, IRepository<Machine, Guid> machiRepository, IRepository<MachineError> machineErrorRepository, IRepository<VendingStatus> vendingStatusRepository)
        {
            this.vendingHistoryRepository = vendingHistoryRepository;
            this.machiRepository = machiRepository;
            this.machineErrorRepository = machineErrorRepository;
            this.vendingHistoryRepository = vendingHistoryRepository;
            this.vendingStatusRepository = vendingStatusRepository;
        }

        public async Task<List<VmcHistoryDto>> VmcHistory()
        {
            LocaTimeUtils localTimeUtils = new LocaTimeUtils();
            var date = localTimeUtils.getStartTodayLocalTime();
            var histories = await vendingHistoryRepository.GetAll().Include(x=>x.Machine)
                .Where(x => x.CreationTime >= date).ToListAsync();
            //.Where(x => x.Machine.Id == machineId && x.CreationTime >= date).ToListAsync();
            
            return histories.Select(x => new VmcHistoryDto()
            {
                MinutesOfDay = (int) ((x.CreationTime - date).TotalMinutes / 5),
                Level = x.VmcLevel,
                MachineId = x.Machine.Id,
                MachineName = x.Machine.Name,
                Temperature = x.Temperature
            }).ToList();
        }

        public async Task<bool> AddVmcHistory(VmcHistoryDto dto)
        {
            try
            {
                var machine = await machiRepository.SingleAsync(x => x.Id == dto.MachineId);
                if(machine != null)
                {
                    var vmcHistory = new VendingHistory()
                    {
                        Machine = machine,
                        CreationTime = DateTime.Now,
                        Temperature = dto.Temperature,
                        VmcLevel = dto.Level,
                        TenantId = machine.TenantId
                    };
                    await vendingHistoryRepository.InsertAsync(vmcHistory);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else
                {
                    Logger.Error("Invalid AddVmcHistory, Machine do not exist");
                }
                
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message,e);
                return false;
            }
          
        }

        public async Task<ListResultDto<MachineErrorDto>> GetAllMachineErrors(PagedAndSortedResultRequestDto input)
        {
            List<MachineErrorDto> result = new List<MachineErrorDto>();
           
            var errors = await machineErrorRepository.GetAll().Include(e=>e.Machine).OrderByDescending(e=>e.CreationTime).ToListAsync();
            List<Guid> machineIDs = errors.Where(e=>e.Machine !=null).Select(e => e.Machine.Id).Distinct().ToList();
            result = errors.Select(e => new MachineErrorDto { MachineId = e.Machine.Id,
                MachineName = e.Machine.Name,
                Message = e.Message,
                Solution = e.Solution,
                MachineErrorCode = e.MachineErrorCode,
                Time = e.CreationTime.ToString()
            }).ToList();
            


            return new ListResultDto<MachineErrorDto>(result);

        }

        public async Task<bool> AddMachineError(MachineErrorDto dto)
        {
            try
            {
                var machine = await machiRepository.SingleAsync(x => x.Id == dto.MachineId);
                if(machine != null)
                {
                    var item = new MachineError()
                    {
                        Machine = machine,
                        MachineErrorCode = dto.MachineErrorCode,
                        Message = dto.Message,
                        Solution = dto.Solution,
                        TenantId = machine.TenantId
                    };

                    await machineErrorRepository.InsertAsync(item);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else
                {
                    Logger.Error("Invalid AddMachineError, Machine do not exist");
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                return false;
            }

        }

        public async Task<bool> AddVendingStatus(VendingStatusDto dto)
        {
            try
            {
                var machine = await machiRepository.SingleAsync(x => x.Id == dto.MachineID);
                if(machine != null)
                {
                    var item = new VendingStatus()
                    {
                        MachineID = machine.Id.ToString(),
                        //  MachineErrorCode = dto.MachineErrorCode,
                        VmcLevel = dto.VmcLevel,
                        VmcOk = dto.VmcOk,
                        IucOk = dto.IucOk,
                        CykloneOk = dto.CykloneOk,
                        MdbOk = dto.MdbOk,
                        Temperature = dto.Temperature,
                        SnapshotUrl = dto.SnapshotUrl,
                        TenantId = machine.TenantId
                    };

                    await vendingStatusRepository.InsertAsync(item);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else
                {
                    Logger.Error("Invalid AddVendingStatus, Machine do not exist");
                }

                
                return true;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                return false;
            }
        }
    }
}
