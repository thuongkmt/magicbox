using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.BlackVMCDiagnostic.DTO
{
    public class MachineState
    {
        public MachineStatus Status { get; set; }
        public int MotorQty { get; set; }
        public bool PickCheck { get; set; }
        public bool HasElevator { get; set; }
        public MachineStates CoinsState { get; set; }
        public MachineStates NotesState { get; set; }

        public Dictionary<string, MachineStates> SlotStatues = new Dictionary<string, MachineStates>();

        public bool BottomSwitchState { get; set; }
        public string ElevatorLocation { get; set; }
        public override string ToString()
        {
            string slotStatusStr = "";
            foreach (var pair in this.SlotStatues)
            {
                slotStatusStr += pair.Key + " | " + pair.Value.ToString() + "   ";
            }

            return string.Format("Machine: Status - {0}, MotorQty - {1}, HasElevator - {2}, CoinsState - {4}, NotesState - {5}, BottomSwitch - {6}, Location - {7},PickTest - {8}, \n Slot Status: {3}", Status, MotorQty, HasElevator, slotStatusStr, CoinsState, NotesState, BottomSwitchState, ElevatorLocation, PickCheck);
        }


    }
    public enum MachineStatus
    {
        Normal,
        Abnormal,
        ControlBoardError
    }
    //public enum PickCheckStatus
    //{
    //    Normal,
    //    Noequipment
    //}
    public enum MachineStates
    {
        Noequipment = 0,
        Normal = 1,
        Abnormal = 2
    }
}
