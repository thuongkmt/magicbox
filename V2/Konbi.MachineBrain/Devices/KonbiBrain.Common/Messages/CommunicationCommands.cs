using System;

namespace KonbiBrain.Common.Messages
{
    public enum CommunicationCommands
    {
      
        Temperature_CurrentTemperature = 401,
        Temperature_SetTemperature = 402,        
    
        Temperature_GetTemperature = 405,

      

        //Common_CreditChaged = 10000,
        Common_VMCStateChanged = 10001,
        Common_DeviceStateChanged = 10000,
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
       
        Common_ValidateMdbResponse=1023,
        Common_DispenseChangeResult=1024,
        Common_MachineTimeInfo = 1025,
        
        Common_ForceToProductSelection=1031,
        Common_MachineStatus=1032,
        Common_MaximizeWindow=1033,
        Common_NoteRejectedEventFired = 1034,
        
        Common_IucResponseResult = 1040,
       
        Common_SubmitCardNumber = 10525,                     
        Common_ValidateEmployee = 1052,           

        Common_CommandResponse = 1000000,
        Common_SubmitRating = 1000001,
     
    }           

    public enum IoTHubCommands
    {
        CheckMachineOnline=12001,
        D2CUpdateMachineStatus=12002,
        D2CVMCStatus=12003,
        HardwareDiagnostic = 12004,
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

    public class RemoteBaseCommand 
    {
        public string RemoteMsg { get; set; }
        public bool IsValidCommand { get; set; }
        public string CommandArgument { get; set; }

    }

    public class IoTRemoteCommand
    {
        public IoTHubCommands IoTHubCommand { get; set; }
        public DateTime DateTime { get; set; }
        public dynamic CommandObj { get; set; }
    }

}
