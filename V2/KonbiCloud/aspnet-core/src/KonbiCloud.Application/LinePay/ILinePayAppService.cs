using Abp.Application.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.LinePay
{
    public interface ILinePayAppService : IApplicationService
    {
        void GetConfirmLinePay(Guid machineId, Int64 transactionId, string regkey);
        Task GetFinishLinePay(Guid machineId, string regkey, Int64 transactionId, int amount, string productName);
    }
}
