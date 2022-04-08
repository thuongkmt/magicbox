using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Enums
{
    public enum TopupHistoryType
    {
        /// <summary>
        /// Current tags are in fridge when topping up
        /// </summary>
        Current =0,
        /// <summary>
        /// Tags that are just topup
        /// </summary>
        Stocked = 1,
        /// <summary>
        /// Tags are removed from fridge in the topup session
        /// </summary>
        Unloaded = 2
    }
}
