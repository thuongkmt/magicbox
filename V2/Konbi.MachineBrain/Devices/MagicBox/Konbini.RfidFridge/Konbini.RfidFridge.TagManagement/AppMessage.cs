
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Konbini.RfidFridge.TagManagement.Enums;

namespace Konbini.RfidFridge.TagManagement
{
    public class AppMessage
    {
    }

    public class ActiveScreenMessage : AppMessage
    {
        public Screen Screen { get; set; }
    }

    public class StateChangeMessage : AppMessage
    {
        public MachineState State { get; set; }
    }

    public class KeyPressedMessage : AppMessage
    {
        public Key Key { get; set; }
    }
    public class CustomerScreenMessage : AppMessage
    {
        public int Quantum { get; set; }
        public int RecentlyCollectedAmount { get; set; }
        public int Difference { get; set; }
        public int Change { get; set; }
        public string Message { get; set; }
        public bool HasError { get; set; }
        public bool IsLowBalance { get; set; }
        public string LowBalanceMessage { get; set; }
        public string ErrorMessage { get; set; }
    }

}
