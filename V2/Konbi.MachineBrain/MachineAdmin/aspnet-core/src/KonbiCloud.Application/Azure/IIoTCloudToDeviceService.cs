using System;
using System.Threading.Tasks;
using KonbiCloud.Common;
using Microsoft.Azure.Devices;

namespace KonbiCloud.Azure
{
    public interface IIoTCloudToDeviceService : IDisposable
    {
        /// <summary>
        /// Send command enum to device
        /// </summary>
        /// <param name="machineId">Machine that receive the command</param>
        /// <param name="command">command enum (int)</param>
        /// <param name="commandObject">Command object, convert to json</param>
        /// <returns>True if machine acknowledge receive command, otherwise false</returns>
        Task<bool> SendDevice(string machineId, IoTHubCommands command, dynamic commandObject = null);

        Task<Device> CheckDeviceOnline(string machineId);
    }
}