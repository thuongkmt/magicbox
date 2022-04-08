using InTheHand.Net.Sockets;
using KonbiCloud.ViewModels;
using Plugin.BLE.Abstractions.Contracts;
using System.Collections.ObjectModel;

namespace KonbiCloud
{
    public class BleApplication
    {
        public static BluetoothDeviceInfo _device = null;
        public static NetworkStream _stream = null;
        public static BluetoothClient _client = new BluetoothClient();

        public static ObservableCollection<DeviceListItemViewModel> deviceList = new ObservableCollection<DeviceListItemViewModel>();
        public static DeviceListItemViewModel deviceConnected;
        public static IService _service;
        public static ICharacteristic _characteristicUpdate;
    }
}
