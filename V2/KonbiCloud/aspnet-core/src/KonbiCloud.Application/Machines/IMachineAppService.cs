using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
//using KonbiCloud.Categories.Dtos;
using KonbiCloud.Common.Dtos;
using KonbiCloud.Machines.Dtos;
using KonbiCloud.RedisCache;

namespace KonbiCloud.Machines
{
    public interface IMachineAppService:IApplicationService
    {
       
        Task<PageResultListDto<MachineListDto>> GetAll(MachineInputListDto input);
        Task<Machine> GetDetail(EntityDto<Guid> input);
        Task Create(CreateMachineInput input);
      
        Task<Machine> Update(Machine input);
        Task Delete(EntityDto<Guid> input);

        //Task SaveCurrentMachineLoadout(CurrentMachineLoadout currentLoadout);
        Task<SendRemoteCommandOutput> SendCommandToMachine(SendRemoteCommandInput input);
        Task<ListResultDto<MachineComboboxDto>> GetMachinesForComboBox();
    }
}
