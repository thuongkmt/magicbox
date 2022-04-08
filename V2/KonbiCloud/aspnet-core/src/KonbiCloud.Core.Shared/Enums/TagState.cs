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
        Syncing,
        /// <summary>
        /// Synced to machine, haven't stocked
        /// </summary>
        Active,
        /// <summary>
        /// Topuped to machine
        /// </summary>
        Stocked,
        /// <summary>
        /// When sync, mark as sold
        /// </summary>
        Sold,
        Removed,
        Unloaded
    }
}
