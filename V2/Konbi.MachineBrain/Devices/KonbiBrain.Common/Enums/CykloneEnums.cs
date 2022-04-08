namespace KonbiBrain.Common.Enums
{


    public enum CykloneState
    {
        None = -1,
        StandBy = 0,
        Collecting = 1,
        CancellingCollect = 2,
        Dispensing = 3,
        ProcessingOrder = 4,
        PrintingReceipt = 5,
        Error = 6,
        Initializing = 7,
    }

    public enum BankType
    {
        None = 0xFF,
        CoinRecycler = 0x00,
        CoinRecycler_Alt = 0x01,
        CoinBox = 0x0F,
        BillDispenser = 0x10,
        BillCollector = 0x1F,
        Cashless = 0xF1,
    }

    public enum EventType
    {
        CancelledTransaction = 0,
        SuccessfulTransaction = 1,
        System = 10,
        BalanceCheckPoint = 100, // To be made every X transactions or Y minutes (whichever is sooner)
        Other = 1000,

        SettingBalancesToSimulatedZero = 200,

        // ERRORS AND ONLY ERRORS TO BE STRICTLY NEGATIVE
        Error = -1,
        Error_Dispense = -100,
        Error_FailedToDispense = -105,
        Error_OverDispenseSingleBank = -110,
        Error_OverDispenseOverall = -115,
        Error_InsufficientCash = -120,
        Error_DispensedLessThanRequested = -125,
        Error_IncorrectFloat = -130,
        Error_StartUp = -1000,
    }

    public enum TransactionStatus
    {
        Completed = 0,
        Completed_TopUp = 10,
        InProcess = 1,
        Error = 255,
        Cancelled = -1,
    }

    public enum IucInterfaceType
    {
        Usb,
        Mdb
    }

// Enum.GetValues(typeof(SomeEnum)) -> SomeEnum []
}
