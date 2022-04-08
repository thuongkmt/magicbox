namespace KonbiCloud.Enums
{
    public enum PaymentState
    {
        /// <summary>
        /// Init transaction
        /// </summary>
        Init = 100,

        /// <summary>
        /// When receive payment commad, return Enable success/Enable Error
        /// </summary>
        EnableSuccess=110,
        EnableError=115,

        /// <summary>
        /// When card scaned, cash/coin start to inserting => InProgress
        /// </summary>
        InProgress = 200,

        /// <summary>
        /// Payment success
        /// </summary>
        Success = 300,

        /// <summary>
        /// Payment failure
        /// </summary>
        Failure = 400
    }
}
