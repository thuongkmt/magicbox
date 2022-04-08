using Konbi.Common.Models;
using KonbiBrain.Messages;

namespace KonbiBrain.Common.Messages
{
    public partial class CommonCommands
    {
        public class MdbResponseResult : BaseCommand
        {
            public MdbResponseResult()
            {
                Command = CommunicationCommands.Common_MdbResponseResult;
            }
           
        }
        public class IucResponseResult:BaseCommand
        {
            public IucResponseResult()
            {
                Command = CommunicationCommands.Common_IucResponseResult;
            }

            public bool IsApproved { get; set; }
            public bool IsCallback { get; set; }
        
        }
        public class KeypadPressed : BaseCommand
        {
            public KeypadPressed()
            {
                Command = CommunicationCommands.Common_KeypadPressed;
            }

            public char Key { get; set; }
        }
       
        public class NFCCardConnectDetected : BaseCommand
        {
            public NFCCardConnectDetected()
            {
                Command = CommunicationCommands.Common_NFCCardConnectDetected;
            }
            public string NfcCardId { get; set; }
        }
        public class VMCStateChanged : BaseCommand
        {
            public VMCStateChanged()
            {
                Command = CommunicationCommands.Common_VMCStateChanged;
            }
          
            public int ErrorCode { get; set; }
        }
        public class DeviceStateChanged : BaseCommand
        {
            public DeviceStateChanged()
            {
                Command = CommunicationCommands.Common_DeviceStateChanged;
            }
            public DeviceState Status { get; set; }
            
        }

        public class ReadingNoteEventFired : BaseCommand
        {
            public ReadingNoteEventFired()
            {
                Command = CommunicationCommands.Common_ReadingNoteEventFired;
            }
        }

        public class NotifyRemoveExistingItem : BaseCommand
        {
            public NotifyRemoveExistingItem()
            {
                Command = CommunicationCommands.Common_NotifyRemoveExistingItem;
            }
        }
        public class StartReadingNoteEventFired : BaseCommand
        {
            public StartReadingNoteEventFired()
            {
                Command = CommunicationCommands.Common_StartReadingNoteEventFired;
            }
        }
        public class EndCreditNoteEventFired : BaseCommand
        {
            public EndCreditNoteEventFired()
            {
                Command = CommunicationCommands.Common_EndCreditNoteEventFired;
            }
        }
        public class NoteDispensingEventFired : BaseCommand
        {
            public NoteDispensingEventFired()
            {
                Command = CommunicationCommands.Common_NoteDispensingEventFired;
            }
        }

        public class NoteDispensingErrorEventFired : BaseCommand
        {
            public NoteDispensingErrorEventFired()
            {
                Command = CommunicationCommands.Common_NoteDispensingErrorEventFired;
            }

            public double Remainer { get; set; }
        }

        public class NoteDispensedEventFired : BaseCommand
        {
            public NoteDispensedEventFired()
            {
                Command = CommunicationCommands.Common_NoteDispensedEventFired;
            }
        }
        public class NoteRejectedEventFired : BaseCommand
        {
            public NoteRejectedEventFired()
            {
                Command = CommunicationCommands.Common_NoteRejectedEventFired;
            }
        }

        public class NoteStoredEventFired : BaseCommand
        {
            public NoteStoredEventFired()
            {
                Command = CommunicationCommands.Common_NoteStoredEventFired;
            }
        }
        public class CommandResponse : BaseCommand
        {
            public CommandResponse()
            {
                Command = CommunicationCommands.Common_CommandResponse;
            }
            public CommunicationCommands RequestedCommand { get; set; }
            public bool Result { get; set; }
        }

        public class HopperPayoutCurrentChanged : BaseCommand
        {
            public HopperPayoutCurrentChanged()
            {
                Command = CommunicationCommands.Common_HopperPayoutCurrentChanged;
            }
        }
        public class CashMdbCurrentChanged : BaseCommand
        {
            public CashMdbCurrentChanged()
            {
                Command = CommunicationCommands.Common_CashMdbCurrentChanged;
            }
        }

        public class NotReadingNoteChanged : BaseCommand
        {
            public NotReadingNoteChanged()
            {
                Command = CommunicationCommands.Common_NotReadingNote;
            }
            public bool NotReadingNote { get; set; }
        }

        public class MachineOverallStatus : BaseCommand
        {
            public MachineOverallStatus()
            {
                Command = CommunicationCommands.Common_MachineOverallStatus;
            }
        }

        public class ValidateSelection : BaseCommand
        {
            public ValidateSelection()  
            {
                Command = CommunicationCommands.Common_ValidateSelection;
            }
            public string Slot { get; set; }
            public bool IsCreditRecovery { get; set; }
        }

        public class ValidateMdbResponse:BaseCommand
        {
            public ValidateMdbResponse()
            {
                Command = CommunicationCommands.Common_ValidateMdbResponse;
            }                                              
        }

        public class DispenseChangeResult:BaseCommand
        {
            public DispenseChangeResult()
            {
                Command = CommunicationCommands.Common_DispenseChangeResult;

            }
            public bool IsSuccess { get; set; }            
        }
        public class CurrentTemprature : BaseCommand
        {
            public CurrentTemprature()
            {
                Command = CommunicationCommands.Temperature_CurrentTemperature;
            }
            public string CurrentTemperature { get; set; }
        }

        public class CurrentMachineTimeInfo : BaseCommand
        {
            public CurrentMachineTimeInfo()
            {
                Command = CommunicationCommands.Common_MachineTimeInfo;
            }
          
        }

        public class ForceToProductSelection : BaseCommand
        {
            public ForceToProductSelection()
            {
                Command = CommunicationCommands.Common_ForceToProductSelection;
            }
        }

        public class MaximizeWindow : BaseCommand
        {
            public MaximizeWindow()
            {
                Command = CommunicationCommands.Common_MaximizeWindow;
            }
        }
        public class SubmitRating : BaseCommand
        {
            public SubmitRating()
            {
                Command = CommunicationCommands.Common_SubmitRating;
            }
            public int RatingId { get; set; }
            public int Value { get; set; }
        }

        public class SubmitCardNumber : BaseCommand
        {
            public SubmitCardNumber()
            {
                Command = CommunicationCommands.Common_SubmitCardNumber;
            }
            public string CardNumber { get; set; }
        }

     
    }
}
