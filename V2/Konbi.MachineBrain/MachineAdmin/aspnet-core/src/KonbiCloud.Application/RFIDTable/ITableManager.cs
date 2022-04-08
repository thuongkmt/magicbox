using Abp.Dependency;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.RFIDTable
{
    public interface ITableManager: ISingletonDependency
    {
        HashSet<ClientInfo> Clients { get;  }
        bool OnSale { get; }
        TransactionInfo Transaction { get; set; }
        Task<SessionInfo> GetSessionInfoAsync();
        Task<TransactionInfo> ProcessTransactionAsync(List<PlateReadingInput> plates);
        void NotifyOnTransactionChanged();
        Task GenerateSaleTransactionAsync();
        Task GetTableDeviceSettingsAsync();
        Task CancelTransactionAsync();
    }
}
