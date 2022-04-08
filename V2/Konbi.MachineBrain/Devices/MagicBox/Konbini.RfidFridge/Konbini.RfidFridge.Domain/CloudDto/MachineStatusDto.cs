using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain.CloudDto
{
    public class MachineStatusDto
    {
        public Guid? MachineId { get; set; }
        public string MachineName { get; set; }
        public bool? DoorState { get; set; }
        public string DoorLastClosed { get; set; }
        public string Temperature { get; set; }
        public string MachineIp { get; set; }
        public string Uptime { get; set; }
        public string Cpu { get; set; }
        public string Memory { get; set; }
        public string Hdd { get; set; }
        public string PaymentState { get; set; }
        public string TopupTime { get; set; }
        public string LastTagTakenOut { get; set; }
        public string LastTagPutIn { get; set; }
        public bool? IsOnline { get; set; }
    }
}
