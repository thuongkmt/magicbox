
using KonbiBrain.Domains.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Messages
{
    public enum CommunicationCommands
    {
        VMC_SelectedElevatorLevelCommand = 100,
        VMC_TellVMCChekIRCommand = 101,
        VMC_DevicePortChangeCommand = 102,
        VMC_DispenseItem = 103,
        VMC_ResetVendingMachine = 104,
        VMC_StartCollectionCheck = 105,
        VMC_StartIRCheck = 106,
        VMC_GetElevatorLevel = 107,
        VMC_StopCollectionCheck = 108,
        VMC_IsVmcAvailable = 109,
        VMC_RestartVmcBrain = 110,
        VMC_DisablePayment = 111,
        VMC_EnablePayment = 112,
        VMC_DispenseChanges=113,
        VMC_GetSlotStatus=114,
        VMC_SyncInventory=115,
        VMC_ForceToDispense=116,
        VMC_Diconnected=117,
        VMC_CoinStatistics=118,
        VMC_EnableCoinPayment = 119,
        VMC_RefundCredit = 120,
        VMC_DisableSlot = 121,
        VMC_GetTemperature = 122,
        VMC_OpenTheDoor = 123,
        VMC_ElevatorSelfTest = 124,
        VMC_CheckGearError = 125,

        HopperPayout_DisableHopper = 200,
        HopperPayout_DisablePayout = 201,
        HopperPayout_DispenseCoins = 202,
        HopperPayout_DispenseNotes = 203,
        HopperPayout_EmptyPayout = 204,
        HopperPayout_EnableHopper = 205,
        HopperPayout_EnablePayout = 206,
        HopperPayout_FloatNotesByDenomination = 207,
        HopperPayout_RefundCredit = 208,
        HopperPayout_ResetCredit = 209,
        HopperPayout_SendCacellation = 210,
        HopperPayout_SetPayoutInibits = 211,
        HopperPayout_EmptyHopper = 212,
        HopperPayout_SendCancellation = 213,
        HopperPayout_SetPayoutInhibits = 214,
        //HopperPayout_CalculatePayout = 215,
        HopperPayout_DispenseExcessNotes = 216,
        HopperPayout_DispenseExcessCoins = 217,
        HopperPayout_ReadingNote = 218,
        HopperPayout_GetCashCurrentTransactionInfo = 219,
        HopperPayout_Dispense = 220,
        HopperPayout_GetAllNotesLevel = 221,
        HopperPayout_PayoutEvent = 222,
        HopperPayout_EndCashSession = 223,
        HopperPayout_SaveTranInfo = 224,
        HopperPayout_ResetHopperPayout = 225,
        HopperPayout_DetectHopperPayout = 226,
        HopperPayout_IsHopperPayoutAvailable = 227,
        HopperPayout_RestartHopperPayoutBrain = 228,
        HopperPayout_CleanUpPollObjs=229,


        MDB_CancelTransaction = 301,
        MDB_EndTransaction = 302,
        MDB_Reset = 303,
        MDB_ResetAndWaitPurchase = 304,
        MDB_SendCancellation = 305,
        MDB_SendDispatchSuccess = 306,
        MDB_DisableReader = 307,
        MDB_SendPurchase = 308,
        MDB_EnableReader = 309,
        MDB_IsMdbAvailable = 310,
        MDB_CheckDevice = 310,

        Temperature_CurrentTemperature = 401,
        Temperature_SetTemperature = 402,        
        Temperature_IsColdMachineAvaiable =403,
        Temperature_IsHotMachineAvaiable=404,
        Temperature_GetTemperature = 405,

        NFC_SendCancellation = 501,
        NFC_DisableReader = 502,
        NFC_EnableReader = 503,
        NFC_SendFreeItem = 504,


        IUC_CancelTransaction = 601,
        IUC_EndTransaction = 602,
        IUC_Reset = 606,
        IUC_SendCancellation = 605,
        IUC_DisableReader = 607,
        IUC_SendPurchase = 608,
        IUC_EnableReader = 609,
        IUC_SendContactlessPurchase=610,
        IUC_SendCpasPurchase=611,
        IUC_IsIucApiAvailable = 612,


        //Common_CreditChaged = 10000,
        Common_VMCStateChanged = 10001,
        Common_HopperPayoutCurrentChanged = 10002,
        Common_CashMdbCurrentChanged = 10004,
        Common_KeypadPressed = 10003,
        Common_MdbResponseResult = 1004,
        Common_NotReadingNote = 1005,
        Common_NFCCardConnectDetected = 1006,
        Common_ReadingNoteEventFired = 1007,
        Common_NoteStoredEventFired = 1008,
        Common_StartReadingNoteEventFired = 1009,
        Common_EndCreditNoteEventFired = 1010,
        Common_NotifyRemoveExistingItem = 1011,
        Common_NoteDispensingEventFired = 1012,
        Common_NoteDispensedEventFired = 1013,
        Common_MachineOverallStatus = 1014,
        Common_NoteDispensingErrorEventFired = 1015,
        Common_ValidateSelection = 1016,
        Common_EnableCashPayment = 1017,
        Common_EnableMdbPayment = 1018,
        Common_CompleteHopperPayoutPaymentCredit = 1019,
        Common_DispenseChange = 1020,
        Common_ResetState=1021,
        Common_ProcessDispenseError=1022,
        Common_ValidateMdbResponse=1023,
        Common_DispenseChangeSuccess=1024,
        Common_MachineTimeInfo = 1025,
        Common_GeneralVendingInfo = 1026,
        Common_CancelPayment = 1027,
        Common_AddErrorTransaction = 1028,
        Common_AddCashlessTransaction=1029,
        Common_RefundCashlessError=1030,
        Common_ForceToProductSelection=1031,
        Common_MachineStatus=1032,
        Common_MaximizeWindow=1033,
        Common_NoteRejectedEventFired = 1034,
        Common_GetHeatupTime=1035,
        Common_AddNoChangeCashTransaction=1036,
        Common_EnableMdbCashPayment=1037,
        Common_EnableCpasPayment=1038,
        Common_EnableContactlessPayment=1039,
        Common_IucResponseResult = 1040,
        Common_GetProductCategory = 1041,
        Common_GetProductsByCategory = 1042,
        Common_GetProductInfo = 1043,
        Common_StopAllProcessed = 1044,
        Common_SendReceiptEmail = 1046,
        Common_GetProducts = 1048,

        Common_AddIucCashlessTransaction = 1045,
        Common_AddDispenseTimeoutTransaction = 1047,
        
        Common_EnableCykloneCashPayment = 1049,

        Common_CommandResponse = 1000000,
        Common_SubmitRating = 1000001,
    }

    public class BaseCommand
    {
        public BaseCommand()
        {
            Id = Guid.NewGuid();
            PublishedDate = DateTime.Now;
        }
        public Guid Id { get; set; }
        public Guid? ResponseForId { get; set; }
        public CommunicationCommands Command { get; set; }
        public string CommandName => Command.ToString();
        public DateTime PublishedDate { get; set; }

        public bool IsTimeout()
        {
            var time = (DateTime.Now - PublishedDate).TotalSeconds;
            if (time >= 10) return true;
            return false;
        }
    }


}
