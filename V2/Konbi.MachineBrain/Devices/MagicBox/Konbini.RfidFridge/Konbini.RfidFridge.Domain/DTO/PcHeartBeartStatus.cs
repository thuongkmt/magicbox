using System;
using System.Collections.Generic;
using System.Text;

namespace Konbini.RfidFridge.Domain.DTO
{
    public class PcHeartBeartStatus
    {
        public decimal CpuUsage { get; set; }
        public decimal CpuSpeed { get; set; }
        public decimal MemoryUsage { get; set; }
        public decimal MemoryTotal { get; set; }
        public decimal MemoryUsed { get; set; }
        public List<DiskDriver> DiskDrivers { get; set; }
        public decimal DiskUsage { get; set; }
        public decimal DiskTotal { get; set; }
        public decimal DiskFree { get; set; }
        public List<Tuple<string, string>> LocalIps { get; set; }
        public string LocalIpString
        {
            get
            {
                if (LocalIps != null)
                {
                    var listIp = new List<string>();
                    foreach (var ip in LocalIps)
                    {
                        listIp.Add($"{ip.Item1}({ip.Item2})");
                    }
                    var listIpStr = string.Join(",", listIp);
                    return listIpStr;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        public override string ToString()
        {

            return $"CpuUsage: {CpuUsage} | CpuSpeed: {CpuSpeed} | MemoryUsage: {MemoryUsage} | MemoryTotal: {MemoryTotal} | MemoryUsed: {MemoryUsed} | DiskUsage: {DiskUsage} | DiskTotal: {DiskTotal} | DiskFree: {DiskFree} | IP: {LocalIpString}";
        }
    }
}
