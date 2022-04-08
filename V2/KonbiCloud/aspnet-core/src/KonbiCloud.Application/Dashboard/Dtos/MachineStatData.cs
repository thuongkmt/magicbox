using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Dashboard.Dtos
{
    public class MachineStatData
    {
        public string MachineName { get; set; }
        public int TotalTransaction { get; set; }
        public decimal TotalSale { get; set; }

        public MachineStatData(string machineName, int totalTransaction, decimal totalSale)
        {
            MachineName = machineName;
            TotalTransaction = totalTransaction;
            TotalSale = totalSale;
        }
    }

    public class MachineStatDataOutput
    {
        public List<MachineStatData> MachineStat { get; set; }

        public MachineStatDataOutput(List<MachineStatData> machineStat)
        {
            MachineStat = machineStat;
        }
    }
}
