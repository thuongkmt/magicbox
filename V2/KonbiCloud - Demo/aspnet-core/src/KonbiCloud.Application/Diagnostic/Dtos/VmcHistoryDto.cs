using System;

namespace KonbiCloud.Diagnostic.Dtos
{
    public class VmcHistoryDto
    {
        public Guid MachineId { get; set; }
        public string MachineName { get; set; }
        public int MinutesOfDay { get; set; }
        public string Level { get; set; }
        public decimal Temperature { get; set; }
        public int VmcLevel 
        {
            get
            {
                switch (Level)
                {
                    case "A": return 8;
                    case "B": return 7;
                    case "C": return 6;
                    case "D": return 5;
                    case "E": return 4;
                    case "F": return 3;
                    case "Z": return 2;
                    case "W": return 1;
                    case "Y": return -1;
                    case "X": return -2;
                }
                return 0;
            }
          
        }
    }
}