using Konbini.RfidFridge.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain.DTO
{
    public class DialogMessageDTO
    {
        public string Message { get; set; }
        public int Timeout { get; set; }
    }
}
