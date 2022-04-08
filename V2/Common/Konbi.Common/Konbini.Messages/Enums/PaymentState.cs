namespace Konbini.Messages.Enums
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
        ActivatingPayment = 105,
        ActivatedPaymentSuccess=110,
        ActivatedPaymentError =115,

        /// <summary>
        /// 
        /// </summary>
        Cancelled = 120,

        /// <summary>
        /// When card scanned, cash/coin start to inserting => InProgress
        /// </summary>
        InProgress = 200,

        /// <summary>
        /// Payment success
        /// </summary>
        Success = 300,

        /// <summary>
        /// Payment failure
        /// </summary>
        Failure = 400,
        /// <summary>
        /// in case  transaction was rejected e.g:  Plate alread sold for the session. cannot sell again
        /// </summary>
        Rejected = 500
    }
}
