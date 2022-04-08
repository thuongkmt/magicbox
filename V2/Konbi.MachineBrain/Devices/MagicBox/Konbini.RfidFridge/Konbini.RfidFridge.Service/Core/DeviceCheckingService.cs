using Konbini.RfidFridge.Domain.DTO.DeviceChecking;
using Konbini.RfidFridge.Domain.Enums.DeviceChecking;
using Konbini.RfidFridge.Service.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Service.Core
{
    public class DeviceCheckingService
    {
        public List<DeviceCheckingDTO> DeviceList = new List<DeviceCheckingDTO>();

        private RabbitMqService RabbitMqService;
        public DeviceCheckingService(RabbitMqService rabbitMqService)
        {
            this.RabbitMqService = rabbitMqService;
        }
        public void AddToChecklist(DeviceName device, string comport = null)
        {
            DeviceList.Add(new DeviceCheckingDTO
            {
                Device = device,
                FriendlyName = GetFriendlyName(device),
                Status = DeviceStatus.CHECKING,
                Comport = comport ?? string.Empty,
                Error = string.Empty
            });
            RabbitMqService.PublishDeviceCheckingList(DeviceList);
        }

        public void UpdateStatus(DeviceName device, DeviceStatus status, string error = null)
        {
            var selectedDevice = DeviceList.FirstOrDefault(x => x.Device == device);
            if (selectedDevice != null)
            {
                selectedDevice.Status = status;
                selectedDevice.Error = error ?? string.Empty;
            }
            RabbitMqService.PublishDeviceCheckingList(DeviceList);
        }

        public void UpdateFriendlyName(DeviceName device, string name)
        {
            var selectedDevice = DeviceList.FirstOrDefault(x => x.Device == device);
            if (selectedDevice != null)
            {
                selectedDevice.FriendlyName = name;
            }
            RabbitMqService.PublishDeviceCheckingList(DeviceList);
        }

        public bool IsCheckingError()
        {
            return DeviceList.Any(x => x.Status == DeviceStatus.ERROR);
        }

        private string GetFriendlyName(DeviceName device)
        {
            switch (device)
            {
                case DeviceName.RFID_READER:
                    return "RFID Reader";
                case DeviceName.LOCK_CONTROLLER:
                    return "Lock Controller";
                case DeviceName.PAYMENT_TERMINAL:
                    return "Payment Terminal";
                case DeviceName.CARD_INSERT:
                    return "Card Insert";
                case DeviceName.QRCODE_READER:
                    return "Qr Code Reader";
            }
            return "Unknown Device";
        }
    }
}
