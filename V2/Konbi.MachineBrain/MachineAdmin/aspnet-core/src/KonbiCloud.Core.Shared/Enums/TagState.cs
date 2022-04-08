using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Enums
{
    public enum TagState
    {
        /// <summary>
        /// Haven't sync to machine
        /// </summary>
        Syncing = 0,
        /// <summary>
        /// Synced to machine, haven't stocked
        /// </summary>
        Active = 1,
        /// <summary>
        /// Topuped to machine
        /// </summary>
        Stocked = 2,
        /// <summary>
        /// When sync, mark as sold
        /// </summary>
        Sold = 3,
        Removed = 4,
        Unloaded = 5
    }
}
