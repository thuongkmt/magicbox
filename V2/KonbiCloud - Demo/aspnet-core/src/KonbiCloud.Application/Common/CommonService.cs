using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Castle.Core.Logging;
using KonbiCloud.Diagnostic;
using KonbiCloud.Machines;
using KonbiCloud.Machines.Dtos;
using Microsoft.EntityFrameworkCore;

namespace KonbiCloud.Common
{
    public class CommonService: KonbiCloudAppServiceBase, ICommonService
    {
        private readonly IRepository<Machine, Guid> machineRepository;
        private readonly IRepository<MachineError> machineErrorRepository;

        public ILogger Logger { get; set; }

        public CommonService(IRepository<MachineError> machineErrorRepository, IRepository<Machine, Guid> machineRepository)
        {
            this.machineErrorRepository = machineErrorRepository;
            this.machineRepository = machineRepository;
        }

    

    }
}
