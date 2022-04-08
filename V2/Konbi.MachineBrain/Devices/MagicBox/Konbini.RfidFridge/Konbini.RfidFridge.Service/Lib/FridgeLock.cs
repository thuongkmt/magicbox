using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Konbini.RfidFridge.Service.Lib
{
    public class FridgeLock
    {
        [DllImport("FridgeLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Fridgelock_Connect(string port,int baud,string frame,ref UIntPtr hd);

        [DllImport("FridgeLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Fridgelock_Disconnect(UIntPtr hd);

        [DllImport("FridgeLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Fridgelock_GetInformation(UIntPtr hd, StringBuilder pInfo, ref int bufferLen);

        [DllImport("FridgeLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Fridgelock_DoorCtrol(UIntPtr hd, byte idx);

        [DllImport("FridgeLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Fridgelock_GetDoorStatus(UIntPtr hd, byte idx, ref byte doorFlg,ref byte alarmFlg, ref byte passingFlg,ref float temperature);


        [DllImport("FridgeLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Fridgelock_SetOutput(UIntPtr hd, Byte idx, byte type, byte frequency, UInt16 timeon, UInt16 timeoff);
   
    
    }
}
