using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.TagManagement.Enums
{
    public enum MachineState
    {
        None = -1,
        NotInit = 0,
        Stanby = 1,
        ProcessingOrder = 2
    }
}
