using Konbini.RfidFridge.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain.DTO
{
    public class MachineStatusMessageDto
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
