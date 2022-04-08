using Konbini.RfidFridge.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain
{
    public static class GlobalAppData
    {
        public static MachineStatus CurrentMachineStatus { get; set; }
        public static int CurrentTransactionAmount { get; set; }

        public static bool IsTransacting { get; set; }

    }
}
