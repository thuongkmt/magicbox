using KonbiCloud.Machines;
using KonbiCloud.Common.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using KonbiCloud.MachineSessions.Dtos;
using Abp.Application.Services.Dto;
using System.Threading.Tasks;
using Abp.Application.Services;

namespace KonbiCloud.MachineSessions
{
    public interface IMachineSessionAppServices : IApplicationService
    {
        Task<PageResultListDto<Session>> GetAll(MachineSessionListDto input);
        Task<Session> GetDetail(EntityDto<Guid> input);
        Task Create(CreateMachineSessionInput input);        
        Task<Session> Update(Session input);
        Task Delete(EntityDto<Guid> input);        
    }
}
