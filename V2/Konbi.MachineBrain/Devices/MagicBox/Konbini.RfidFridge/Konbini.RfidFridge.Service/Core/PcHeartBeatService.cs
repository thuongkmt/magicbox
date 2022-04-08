using Konbini.RfidFridge.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ObjectQuery = System.Management.ObjectQuery;

namespace Konbini.RfidFridge.Service.Core
{
    public class PcHeartBeatService
    {
        //private static PerformanceCounter CPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private LogService LogService;
        public PcHeartBeatService(LogService logService)
        {
            LogService = logService;
        }
        public PcHeartBeartStatus GetCurrentHeartBeartStatus()
        {

            var data = new PcHeartBeartStatus();
            try
            {
                var totalRam = GetRamSize();
                var ramUsed = totalRam - GetAvailableRam();
                var ramUsage = (ramUsed / totalRam) * 100;

                var diskSize = GetDiskSize();
                var diskSpace = GetTotalDiskFreeSpace();
                var diskUsed = diskSize - diskSpace;
                var diskUsage = (diskUsed / diskSize) * 100;

                data = new PcHeartBeartStatus
                {
                    CpuSpeed = Math.Round(GetCpuSpeed(), 2),
                    CpuUsage = Math.Round(decimal.Parse(GetCpuUsage().ToString()), 2),
                    MemoryUsage = Math.Round(ramUsage, 2),
                    MemoryTotal = Math.Round(GetRamSize(), 2),
                    MemoryUsed = Math.Round(ramUsed, 2),
                    DiskTotal = Math.Round(diskSize, 2),
                    DiskFree = Math.Round(diskSpace, 2),
                    DiskUsage = Math.Round(diskUsage, 2),
                    DiskDrivers = GetDiskDrivers(),
                    LocalIps = GetAllLocalIPv4()
                };
                return data;
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return data;
        }
        public static object GetCpuUsage()
        {
            PerformanceCounter CPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            CPUCounter.NextValue();
            Thread.Sleep(1000);
            return CPUCounter.NextValue();
        }


        public static decimal GetCpuSpeed()
        {
            ObjectQuery wql = new ObjectQuery("SELECT * FROM Win32_Processor");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql);
            ManagementObjectCollection results = searcher.Get();
            foreach (var o in results)
            {
                var result = (ManagementObject)o;
                var cpuSpeed = decimal.Parse(result["CurrentClockSpeed"].ToString());
                var pros = result.Properties;
                return cpuSpeed;
            }
            return 0;
        }

        private static decimal GetRamSize()
        {
            ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (var o in moc)
            {
                var item = (ManagementObject)o;
                return (decimal)Math.Round(Convert.ToDouble(item.Properties["TotalPhysicalMemory"].Value) / 1048576,
                    0);
            }

            return 0;
        }

        public static List<DiskDriver> GetDiskDrivers()
        {
            var drivers = new List<DiskDriver>();
            foreach (var d in System.IO.DriveInfo.GetDrives().Where(x => x.IsReady))
            {
                drivers.Add(new DiskDriver
                {
                    Name = $"{d.VolumeLabel} ({d.Name})",
                    TotalSize = d.TotalSize / 1048576 / 1024,
                    AvaibleSize = (d.AvailableFreeSpace) / 1048576 / 1024
                });
            }
            return drivers;
        }

        private static decimal GetTotalDiskFreeSpace()
        {
            return System.IO.DriveInfo.GetDrives().Where(x => x.IsReady).Sum(drive => drive.TotalFreeSpace);
        }

        private static decimal GetDiskSize()
        {
            return System.IO.DriveInfo.GetDrives().Where(x => x.IsReady).Sum(drive => drive.TotalSize);
        }

        public static decimal GetAvailableRam()
        {
            PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            return decimal.Parse(ramCounter.NextValue().ToString(CultureInfo.InvariantCulture));
        }

        public static List<Tuple<string, string>> GetAllLocalIPv4()
        {
            List<Tuple<string, string>> ipAddrList =  new List<Tuple<string, string>>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType != NetworkInterfaceType.Loopback && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {

                            ipAddrList.Add(new Tuple<string, string>(ip.Address.ToString(), item.NetworkInterfaceType.ToString()));
                        }
                    }
                }
                
            }
            return ipAddrList;
        }
    }
}
