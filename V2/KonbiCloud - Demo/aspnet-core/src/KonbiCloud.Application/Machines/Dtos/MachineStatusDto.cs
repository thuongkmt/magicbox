
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Machines.Dtos
{
    public class MachineStatusDto
    {
        public Guid MachineID { get; set; }
        public string MachineName { get; set; }
        public string LastUpdateTime { get; set; }
        public string PreviousUpdateTime { get; set; }

        public string VMCCurrent { get; set; }
        public string VMCPrevious { get; set; }
        public string VMCCurrentStatus { get; set; }
        public string VMCPreviousStatus { get; set; }

        public string WhiteVMCCurrent { get; set; }
        public string WhiteVMCPrevious { get; set; }

        public string IUCCurrent { get; set; }
        public string IUCPrevious { get; set; }

        public string CykloneCurrent { get; set; }
        public string CyklonePrevious { get; set; }

        public string MDBCurrent { get; set; }
        public string MDBPrevious { get; set; }

        public string TemperatureCurrent { get; set; }
        public string TemperaturePrevious { get; set; }

        public string ExtentCurrent { get; set; }
        public string ExtentPrevious { get; set; }

        public string Status { get; set; }
    }
}
