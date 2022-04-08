using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Common
{
    public static class LoadoutHelper
    {
        //Convention for labling: 101,102,103,104,105,106,107,108,109,110 => VMC1
        //111,112,113,114,115,116,117,118,120 => VMC2
        public static bool IsVMC2(this string slot)
        {
            if (slot.EndsWith("0"))
                return slot.EndsWith("20");//110 => VMC1, 120 => VMC2

            return int.Parse(slot.Substring(1, 1)) > 0;
        }
    }
}
