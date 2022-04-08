namespace KonbiCloud.Enums
{
    public enum ProductTagStateEnum
    {
        /// <summary>
        /// Synced to machine, haven't stocked
        /// </summary>
        Mapped,
        /// <summary>
        /// Topuped to machine
        /// </summary>
        Stocked,
        /// <summary>
        /// When sync, mark as sold
        /// </summary>
        Sold
    }
}
