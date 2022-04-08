using Konbini.RfidFridge.Domain.DTO;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Konbini.RfidFridge.Domain.Enums;
using Konbini.RfidFridge.Service.Data;
using Konbini.RfidFridge.Service.Lib;
using Konbini.RfidFridge.Service.Util;
using System.Diagnostics;
using System.Timers;

namespace Konbini.RfidFridge.Service.Core
{
    public class FridgeLockInterface
    {
        private static UIntPtr lockHandler = UIntPtr.Zero;

        private LogService LogService;

        public Action<string> OnDoorAlarm { get; set; }
        public FridgeLockInterface(LogService logService)
        {
            LogService = logService;
        }

        public int Connect(string comport)
        {
            int iret = FridgeLock.Fridgelock_Connect(comport, 38400, "8E1", ref lockHandler);
            LogService.LogLockInfo($"Connect: {iret} | Comport {comport} | Handler {lockHandler}");
            return iret;
        }

        public int OpenDoor()
        {
            int doorIndex = 1;
            int iret = FridgeLock.Fridgelock_DoorCtrol(lockHandler, (byte)doorIndex);
            LogService.LogLockInfo($"OpenDoor: {iret} | Handler {lockHandler}");

            return iret;
        }
        private string CurrentDoorStatus { get; set; }
        public int GetDoorStatus(ref bool isDoorOpen, ref bool alarm, ref bool passing, ref float temperature)
        {
            int doorIndex = 1;
            byte doorFlg = 0;
            byte alarmFlg = 0;
            byte passingFlg = 0;
            int iret = FridgeLock.Fridgelock_GetDoorStatus(lockHandler, (byte)doorIndex, ref doorFlg, ref alarmFlg, ref passingFlg, ref temperature);
            isDoorOpen = (doorFlg == 1);
            alarm = (alarmFlg == 1);
            passing = (passingFlg == 1);
            var status = $"GetDoorStatus: {iret} | TMP = {temperature} | Handler {lockHandler} | Is Door Open {isDoorOpen} | Alarm {alarm} | Passing {passing}";
            if (CurrentDoorStatus != status)
            {
                LogService.LogLockInfo(status);
            }
            CurrentDoorStatus = status;
            if (alarm)
            {
                var alarmMessage = $"DOOR ALARM!!!! | Is Door Open: {isDoorOpen}";
                OnDoorAlarm?.Invoke(alarmMessage);
            }
            return iret;
        }

        public float GetTemperature()
        {
            int doorIndex = 1;
            byte doorFlg = 0;
            byte alarmFlg = 0;
            byte passingFlg = 0;
            float temperature = 0.0f;
            int iret = FridgeLock.Fridgelock_GetDoorStatus(lockHandler, (byte)doorIndex, ref doorFlg, ref alarmFlg, ref passingFlg, ref temperature);
            LogService.LogLockInfo($"GetTemperature: {iret}  TMP = {temperature} | Handler {lockHandler} | {doorFlg} | {alarmFlg} | {passingFlg}" );
            return temperature;
        }

        public bool IsDeviceOk()
        {
            int doorIndex = 1;
            byte doorFlg = 0;
            byte alarmFlg = 0;
            byte passingFlg = 0;
            float temperature = 0.0f;
            int iret = FridgeLock.Fridgelock_GetDoorStatus(lockHandler, (byte)doorIndex, ref doorFlg, ref alarmFlg, ref passingFlg, ref temperature);
            LogService.LogLockInfo($"IsDeviceOk: {iret} | TMP = {temperature} | Handler {lockHandler} | {doorFlg} | {alarmFlg} | {passingFlg}");

            return iret == 0;
        }
    }
}
