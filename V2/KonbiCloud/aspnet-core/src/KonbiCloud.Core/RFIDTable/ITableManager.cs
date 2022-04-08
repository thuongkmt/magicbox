using Abp.Dependency;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.RFIDTable
{
    public interface ITableManager: ISingletonDependency
    {
        Task<SessionInfo> GetSessionInfo();
        TransactionInfo ProcessTransaction(IEnumerable<PlateReadingInput> plates);
    }
}
