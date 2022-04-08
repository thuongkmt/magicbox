using Abp.Application.Services;
using KonbiCloud.GetStarted.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.Dashboard
{
    public interface IGetStartedAppService : IApplicationService
    {
        Task<List<GetStartedDataOutput>> getGetStartedStatus();
    }
}
