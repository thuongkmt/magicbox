using Konbini.Messages.Commands.RFIDTable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.RFIDTable
{
    public interface IRfidTableSignalRMessageCommunicator
    {
        Task UpdateTransactionInfo(TransactionInfo transaction);
        Task SendAdminDetectedPlates(IEnumerable<Konbini.Messages.Commands.RFIDTable.PlateInfo> plates);
        Task UpdateTableSettings(string selectedPort, List<string> availablePorts, bool isServiceRunning);
        Task UpdateSessionInfo(SessionInfo sessionInfo);
    }
}
