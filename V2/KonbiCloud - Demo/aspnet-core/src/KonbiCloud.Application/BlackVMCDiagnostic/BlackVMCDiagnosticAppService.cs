using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using KonbiCloud.BlackVMCDiagnostic.Dtos;
using KonbiCloud.Common.Dtos;
using KonbiCloud.Machines;
using Microsoft.EntityFrameworkCore;

namespace KonbiCloud.BlackVMCDiagnostic
{
    public class BlackVMCDiagnosticAppService : KonbiCloudAppServiceBase, IBlackVMCDiagnosticAppService
    {
        private readonly IRepository<VMCDiagnostic, Guid> vmcRepo;
        private readonly IRepository<MachineStatus, Guid> MachineStatusRepo;
        private readonly IRepository<Machine, Guid> MachineRepo;

        public BlackVMCDiagnosticAppService(IRepository<VMCDiagnostic, Guid> vmcRepo, IRepository<Machine, Guid> machineRepo,
                                            IRepository<MachineStatus, Guid> mcStatusRepo)
        {
            this.vmcRepo = vmcRepo;
            MachineRepo = machineRepo;
            MachineStatusRepo = mcStatusRepo;
        }

        public async Task<ListResultDto<BlackVMCDiagnosticDto>> GetLevelDiagnostic(int maxResultCount, int skipCount, string machineId)
        {
            var tenantId = AbpSession.TenantId ?? 0;

            List<VMCDiagnostic> data = new List<VMCDiagnostic>();
            int totalItem = 0;
            try
            {
                var total = await vmcRepo
                    .GetAllListAsync(x => x.TenantId == tenantId && x.MachineId.Equals(machineId));
                totalItem = total.Count();

                data = total
                    .Skip(skipCount)
                    .Take(maxResultCount)
                    .OrderBy(x => x.LogTime).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Get Level Diagnostic Error", ex);
            }

            var results = data.MapTo<List<BlackVMCDiagnosticDto>>();
            var output = new PageResultListDto<BlackVMCDiagnosticDto>(results)
            {
                TotalCount = totalItem
            };

            return output;
        }

        [AbpAllowAnonymous]
        public async Task<bool> AddLevelDiagnostic(VMCDiagnostic vmc)
        {
            try
            {
                if (!string.IsNullOrEmpty(vmc.MachineId))
                {
                    var machineid = Guid.Parse(vmc.MachineId);
                    var machine = await MachineRepo.FirstOrDefaultAsync(x => x.Id == machineid);
                    if (machine != null)
                    {
                        var tenantid = machine.TenantId;
                        vmc.TenantId = tenantid;
                        await vmcRepo.InsertAsync(vmc);
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Add Level Diagnostic Error", ex);
                return false;
            }
        }

        public async Task<VmcMachineStatusDto> GetMachineStatus(string machineId)
        {
            var tenantId = AbpSession.TenantId ?? 0;

            var data = new MachineStatus();
            try
            {
                data = await MachineStatusRepo
                    .GetAll()
                    .LastOrDefaultAsync(x => x.TenantId == tenantId && x.MachineId.Equals(machineId));
            }
            catch (Exception ex)
            {
                Logger.Error("Get Machine Status Error", ex);
            }

            var dto = data.MapTo<VmcMachineStatusDto>();
            return dto;
        }

        [AbpAllowAnonymous]
        public async Task<bool> AddOrUpdateMachineStatus(MachineStatus mc)
        {
            try
            {
                if (!string.IsNullOrEmpty(mc.MachineId))
                {
                    var machineid = Guid.Parse(mc.MachineId);
                    var mcStatus = await MachineRepo.FirstOrDefaultAsync(x => x.Id == machineid);
                    if (mcStatus != null)
                    {
                        var tenantid = mcStatus.TenantId;
                        mc.TenantId = tenantid;
                        var existing = await MachineStatusRepo.FirstOrDefaultAsync(x => x.MachineId == mc.MachineId);
                        if(existing==null)
                            await MachineStatusRepo.InsertAsync(mc);
                        else
                        {
                            existing.A0 = mc.A0;
                            existing.A1 = mc.A1;
                            existing.A2 = mc.A2;
                            existing.A3 = mc.A3;
                            existing.A4 = mc.A4;
                            existing.A5 = mc.A5;
                            existing.A6 = mc.A6;
                            existing.A7 = mc.A7;
                            existing.A8 = mc.A8;
                            existing.A9 = mc.A9;  

                            existing.B0 = mc.B0;
                            existing.B1 = mc.B1;
                            existing.B2 = mc.B2;
                            existing.B3 = mc.B3;
                            existing.B4 = mc.B4;
                            existing.B5 = mc.B5;
                            existing.B6 = mc.B6;
                            existing.B7 = mc.B7;
                            existing.B8 = mc.B8;
                            existing.B9 = mc.B9;

                            existing.C0 = mc.C0;
                            existing.C1 = mc.C1;
                            existing.C2 = mc.C2;
                            existing.C3 = mc.C3;
                            existing.C4 = mc.C4;
                            existing.C5 = mc.C5;
                            existing.C6 = mc.C6;
                            existing.C7 = mc.C7;
                            existing.C8 = mc.C8;
                            existing.C9 = mc.C9;

                            existing.D0 = mc.D0;
                            existing.D1 = mc.D1;
                            existing.D2 = mc.D2;
                            existing.D3 = mc.D3;
                            existing.D4 = mc.D4;
                            existing.D5 = mc.D5;
                            existing.D6 = mc.D6;
                            existing.D7 = mc.D7;
                            existing.D8 = mc.D8;
                            existing.D9 = mc.D9;

                            existing.E0 = mc.E0;
                            existing.E1 = mc.E1;
                            existing.E2 = mc.E2;
                            existing.E3 = mc.E3;
                            existing.E4 = mc.E4;
                            existing.E5 = mc.E5;
                            existing.E6 = mc.E6;
                            existing.E7 = mc.E7;
                            existing.E8 = mc.E8;
                            existing.E9 = mc.E9;

                            existing.F0 = mc.F0;
                            existing.F1 = mc.F1;
                            existing.F2 = mc.F2;
                            existing.F3 = mc.F3;
                            existing.F4 = mc.F4;
                            existing.F5 = mc.F5;
                            existing.F6 = mc.F6;
                            existing.F7 = mc.F7;
                            existing.F8 = mc.F8;
                            existing.F9 = mc.F9;

                            await MachineStatusRepo.UpdateAsync(existing);
                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Add Machine Status Error", ex);
                return false;
            }
        }
    }
}
