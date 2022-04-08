using Konbini.Messages;
using Konbini.Messages.Enums;
using Konbini.Messages.Services;
using Konbini.RfidFridge.Common;
using Konbini.RfidFridge.Domain.CloudDto;
using Konbini.RfidFridge.Domain.DTO;
using Konbini.RfidFridge.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Konbini.RfidFridge.Service.Core
{
    public class MachineStatusService
    {
        #region Services
        private LogService LogService;
        private PcHeartBeatService PcHeartBeatService;
        private ISendMessageToCloudService SendMessageToCloudService;
        private TemperatureInterface TemperatureInterface;
        private FridgeInterface FridgeInterface;
        #endregion

        #region Private properties
        private bool UseCloud;
        private Timer _timerPcHeartBeat = new Timer();
        #endregion

        #region Status properties
        private PcHeartBeartStatus pcHeartBeartStatus;
        public PcHeartBeartStatus PcHeartBeartStatus
        {
            get => pcHeartBeartStatus;
            set
            {
                pcHeartBeartStatus = value;
            }
        }

        public MachineStatusDto CurrentMachineStatus { get; set; }
        #endregion

        public MachineStatusService(LogService logService,
            PcHeartBeatService pcHeartBeatService,
            ISendMessageToCloudService sendMessageToCloudService,
            TemperatureInterface temperatureInterface,
            FridgeInterface fridgeInterface
            )
        {
            LogService = logService;
            PcHeartBeatService = pcHeartBeatService;
            SendMessageToCloudService = sendMessageToCloudService;
            TemperatureInterface = temperatureInterface;
            FridgeInterface = fridgeInterface;
        }
        public void Init(bool useCloud)
        {
            try
            {
                CurrentMachineStatus = new MachineStatusDto();
                PcHeartBeartStatus = new PcHeartBeartStatus();

                UseCloud = useCloud;
                LogService.LogInfo("Machine status service init");

                // CollectData and send to cloud first time
                CollectData();
                if (UseCloud)
                {
                    ReportToCloud();
                }

                // PC heart beat
                StartRecordPcHeartBeat();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex.ToString());
            }
        }

        /// <summary>
        /// Real time properties
        /// </summary>
        /// <param name="property"></param>
        /// <param name="name"></param>
        public void NotifyChangeToMachineStatus(object property, string name)
        {
            if(CurrentMachineStatus == null)
            {
                return;
            }
            LogService.LogMachineStatus($"Notify realtime : {name} | {property}");
            switch (name)
            {
                case nameof(FridgeInterface.CurrentMachineStatus):
                    CurrentMachineStatus.PaymentState = ((MachineStatus)property).ToString();
                    LogService.LogMachineStatus($"NotifyChangeToMachineStatus: {name} | {CurrentMachineStatus.PaymentState}");
                    break;
                case nameof(FridgeInterface.CurrentDoorState):
                    CurrentMachineStatus.DoorState = (Domain.Enums.DoorState)property == Domain.Enums.DoorState.OPEN ? false : true;
                    LogService.LogMachineStatus($"NotifyChangeToMachineStatus: {name} | {CurrentMachineStatus.DoorState}");
                    break;
            }

            if (UseCloud)
            {
                ReportToCloud();
            }

        }

        private void StartRecordPcHeartBeat()
        {
            int.TryParse(RfidFridgeSetting.System.MachineStatus.PcHeartBeatInterval, out int interval);

            _timerPcHeartBeat.Interval = interval;
            _timerPcHeartBeat.Elapsed += _timerPcHeartBeat_Elapsed;
            _timerPcHeartBeat.Start();

            void _timerPcHeartBeat_Elapsed(object sender, ElapsedEventArgs e)
            {
                // Set properties 
                CollectPcData();

                // Report to cloud
                if (UseCloud)
                {
                    ReportToCloud();
                }
            }
        }

        private void CollectData()
        {
            // General propeties
            CurrentMachineStatus.MachineId = Guid.Parse(RfidFridgeSetting.Machine.Id);
            CurrentMachineStatus.MachineName = RfidFridgeSetting.Machine.Name;
            CurrentMachineStatus.PaymentState = FridgeInterface.CurrentMachineStatus.ToString();
            CurrentMachineStatus.DoorState = FridgeInterface.CurrentDoorState == Domain.Enums.DoorState.OPEN ? false : true;

            // Pc
            CollectPcData();
        }

        private void CollectPcData()
        {
            LogService.LogMachineStatus(PcHeartBeartStatus.ToString());
            PcHeartBeartStatus = PcHeartBeatService.GetCurrentHeartBeartStatus();
            CurrentMachineStatus.Cpu = $"{PcHeartBeartStatus.CpuUsage}%";
            CurrentMachineStatus.Memory = $"{PcHeartBeartStatus.MemoryUsage}%";
            CurrentMachineStatus.Hdd = $"{PcHeartBeartStatus.DiskUsage}%";
            CurrentMachineStatus.MachineIp = PcHeartBeartStatus.LocalIpString;

            CurrentMachineStatus.Temperature = $"{TemperatureInterface.Temperatures[0]}°C";
        }

        private void ReportToCloud()
        {

            Task.Run(() =>
            {
                var kv = new KeyValueMessage()
                {
                    Key = MessageKeys.MachineStatus,
                    MachineId = Guid.Parse(RfidFridgeSetting.Machine.Id),
                    Value = CurrentMachineStatus
                    //TenantId = 4
                };

                SendMessageToCloudService.SendMsgToCloud(kv);
            });
        }
        #region Functions

        #endregion
    }
}
