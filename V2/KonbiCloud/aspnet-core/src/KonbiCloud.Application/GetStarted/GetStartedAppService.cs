using Abp.Domain.Repositories;
using KonbiCloud.Common;
using KonbiCloud.GetStarted.Dtos;
using KonbiCloud.Machines;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.Dashboard
{
    public class GetStartedAppService : KonbiCloudAppServiceBase, IGetStartedAppService
    {
        private readonly IRepository<Session, Guid> _sessionRepository;
        private readonly IDetailLogService _detailLogService;

        public GetStartedAppService(
            IRepository<Session, Guid> sessionRepository,
            IDetailLogService detailLog)
        {
            _sessionRepository = sessionRepository;
            _detailLogService = detailLog;
        }

        public async Task<List<GetStartedDataOutput>> getGetStartedStatus()
        {
            var listResult = new List<GetStartedDataOutput>();

            var session = _sessionRepository.GetAll();
            var totalSession = await session.CountAsync();
            listResult.Add(new GetStartedDataOutput() { StepId = 2, StepName = "Session", StepTitle = "Add Session", StepSubTitle = "Click Create to navigate Session manager screen", StepActionUrl = "/app/main/machines/sessions", StepDoneFlg = totalSession });

            var step6Count = 0;
            if(totalSession > 0)
            {
                step6Count = 1;
            }

            listResult.Add(new GetStartedDataOutput() { StepId = 6, StepName = "SyncDataFromServerMachine", StepTitle = "<div>1. Sync initial data from Server to 2 machines</div><div>2. Scan all plates at machine 1 to manage inventory</div><div>3. Sync InventoryItem from machine to server database</div>", StepSubTitle = "", StepActionUrl = "", StepDoneFlg = step6Count });
            return listResult;
        }
    }
}
