using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbi.Common.Interfaces
{
    public interface ILogService
    {
        void LogException(string message);
        void LogException(Exception ex);
        void LogInfo(string message);
        void LogPerformanceDebug(string message);
    }

    public interface IKonbiBrainLogService : ILogService
    {
        void LogWhiteVmc(string msg);
        void LogLockerError(Exception ex);
        void LogHopperPayoutError(Exception ex);
        void LogMdbError(Exception ex);
        void LogTemperatureDeviceError(Exception ex);
        void LogVMCError(Exception ex);
        void LogVMCInfo(string message);
        void LogIucApi(string message);
        void LogHopperPayoutError(string message);
        void LogMdbError(string message);
        void LogTemperatureDeviceError(string message);
        void LogVMCError(string message);

        Guid LogCloudError(Exception ex);
        //void LogCloudInfo(string message);
        void LogHopperPayoutEvents(string message);
        void LogViewModelNavigation(string message);
        void NotifyHopperPayoutError(string message);
       
        void LogCyklone(string message);
        void LogEventHub(string message);
        //void LogRequestInfo(string message);
        void LogScbInfo(string info);
        void LogBbposInfo(string info);

        void LogCloudSyncInfo(string message);
        void LogEmailInfo(string info);
        void LogHidRfidInfo(string info);
        void LogRawInputInfo(string info);

    }

}
