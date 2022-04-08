using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Konbi.Common.Utilities
{
    public class NetworkChecker
    {
        public static bool IsAvailableNetworkActive()
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NetworkInterface[] interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                return (from face in interfaces
                        where face.OperationalStatus == OperationalStatus.Up
                        where (face.NetworkInterfaceType != NetworkInterfaceType.Tunnel) && (face.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                        select face.GetIPv4Statistics()
                        ).Any(statistics => (statistics.BytesReceived > 0) && (statistics.BytesSent > 0));
            }
            return false;
        }

    }
}
