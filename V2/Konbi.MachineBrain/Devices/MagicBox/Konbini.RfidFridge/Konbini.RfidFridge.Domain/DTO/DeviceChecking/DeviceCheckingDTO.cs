using Konbini.RfidFridge.Domain.Enums.DeviceChecking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain.DTO.DeviceChecking
{
    public class DeviceCheckingDTO
    {
        public  DeviceName Device { get; set; }
        public string FriendlyName { get; set; }
        public DeviceStatus Status { get; set; }
        public string Comport { get; set; }
        public string Error { get; set; }
        public string StatusString => Status.ToString();
    }
}
