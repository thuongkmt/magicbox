using Abp.Dependency;
using KonbiBrain.Common.Messages.Payment;
using KonbiCloud.Common;
using KonbiCloud.Enums;
using Konbini.Messages.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.RFIDTable
{
    public interface ITableSettingsManager : ISingletonDependency
    {
        string TableProcessName { get; }
        bool IsServiceRunning { get; set; }

        Task<CommandState> GetServiceInfosAsync();
        event EventHandler<CommandEventArgs> DeviceFeedBack;

        Task GetSettingsAsync();
        bool StopService();
        bool StartService(string port);
        Task<bool> ForceToReadPlates();
    }
}
