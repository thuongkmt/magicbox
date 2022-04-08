using Konbini.RfidFridge.Domain.DTO;
using Konbini.RfidFridge.Service.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Data.Test
{
    public class Application
    {
        public LogService logService;
        public FridgeLockInterface FridgeLockInterface;
        public TemperatureInterface TemperatureInterface;
        public TeraWalletInterface TeraWalletInterface;

        public Application(LogService logService, FridgeLockInterface fridgeLockInterface, TemperatureInterface temperatureInterface, TeraWalletInterface walletInterface)
        {
            this.logService = logService;
            this.FridgeLockInterface = fridgeLockInterface;
            this.TemperatureInterface = temperatureInterface;
            this.TeraWalletInterface = walletInterface;
        }

        private static UIntPtr mHandle = UIntPtr.Zero;

        public void Run()
        {
            TeraWalletInterface.TOKEN = "m6buplgi9n7ppq3i6nalyd2gei14b8f4bgb9kotr";
            var balance = TeraWalletInterface.GetBalance(3);
            Console.Read();

            //var b = 0.01;

            //var c = Math.Round(b, 2);

            //Console.WriteLine((c * 100).ToString());
            //logService.Init();
            //logService.LogTemp("OK BABI");

            //Console.WriteLine("COMPORT:");
            //var comport = Console.ReadLine();
            //FridgeLockInterface.Connect(comport);
            //FridgeLockInterface.OpenDoor();


            //TemperatureInterface.ThermalProbeType = TemperatureInterface.ProbeType.FridgeLock;
            //// Init device
            //TemperatureInterface.Init(1000, 5);

            //// Report to local
            //var tmpReportTime = 0;
            //TemperatureInterface.OnTemperaturesReport = (temp) =>
            //{
            //    var tmpDto = new TemperatureDto(temp, 0);
            //    logService.Debug($"Temperatures: {tmpDto}");
            //    tmpReportTime = 0;
            //};

            //Console.ReadLine();


            //logService.Init();
            //logService.LogTemp("OK BABI");

            //Console.WriteLine("COMPORT:");
            //var comport = Console.ReadLine();
            //int iret = FridgeLock.Fridgelock_Connect(comport, 38400, "8E1", ref mHandle);
            //Console.WriteLine($"Connect to comport: {iret}");
            //byte doorFlg = 0;
            //byte alarmFlg = 0;
            //byte passingFlg = 0;
            //float temperature = 0.0f;
            //int doorIndex = 1;

            //while (true)
            //{
            //    iret = FridgeLock.Fridgelock_GetDoorStatus(mHandle, (byte)doorIndex, ref doorFlg, ref alarmFlg, ref passingFlg, ref temperature);
            //    logService.Debug($"###{DateTime.Now};{temperature}");
            //    Thread.Sleep(1000);
            //}

            Console.ReadLine();
        }

        public class FridgeLock
        {
            [DllImport("FridgeLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern int Fridgelock_Connect(string port, int baud, string frame, ref UIntPtr hd);

            [DllImport("FridgeLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern int Fridgelock_Disconnect(UIntPtr hd);

            [DllImport("FridgeLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern int Fridgelock_GetInformation(UIntPtr hd, StringBuilder pInfo, ref int bufferLen);

            [DllImport("FridgeLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern int Fridgelock_DoorCtrol(UIntPtr hd, byte idx);

            [DllImport("FridgeLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern int Fridgelock_GetDoorStatus(UIntPtr hd, byte idx, ref byte doorFlg, ref byte alarmFlg, ref byte passingFlg, ref float temperature);


            [DllImport("FridgeLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern int Fridgelock_SetOutput(UIntPtr hd, Byte idx, byte type, byte frequency, UInt16 timeon, UInt16 timeoff);


        }
    }
}
