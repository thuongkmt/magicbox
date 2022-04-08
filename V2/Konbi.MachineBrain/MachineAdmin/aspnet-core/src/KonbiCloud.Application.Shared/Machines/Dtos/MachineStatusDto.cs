using System;
using System.Collections.Generic;
using System.Text;
using KonbiCloud.Enums;
using Konbini.Messages.Enums;

namespace KonbiCloud.Machines.Dtos
{
    public class MachineStatusDto
    {
        public DoorState State { get; set; }
        public DateTime? DoorLastClosed { get; set; }
        public decimal? Temperature { get; set; }
        public string MachineId { get; set; }
        public long Uptime { get; set; }
        public float Cpu { get; set; }
        public float Memory { get; set; }
        public float Hdd { get; set; }
        public PaymentState PaymentState { get; set; }
        public DateTime TopupTime { get; set; }
        public string LastTagTakenOut { get; set; }
        public string LastTagPutIn{ get; set; }

    }
}
