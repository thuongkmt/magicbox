using Acr.UserDialogs;
using KonbiCloud.Commands;
using KonbiCloud.ViewModels.Base;
using KonbiCloud.Views;
using MvvmCross.Commands;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace KonbiCloud.ViewModels
{
    public class DeviceListViewModel : XamarinViewModel
    {
        private IBluetoothLE ble;
        private IAdapter adapter;
        private CancellationTokenSource _cancellationTokenSource;
        public bool IsStateOn => ble.IsOn;
        public string StateText => GetStateText();
        public ICommand RefreshCommand => HttpRequestCommand.Create(TryStartScanning);
        public ICommand DisconnectCommand => HttpRequestCommand.Create(DisconnectAsync);
        public ICommand PageAppearingCommand => HttpRequestCommand.Create(PageAppearingAsync);
        public ICommand AddTagsCommand => HttpRequestCommand.Create(AddTagsAsync);
        //public MvxCommand StopScanCommand => new MvxCommand(() =>
        //{
        //    _cancellationTokenSource.Cancel();
        //    CleanupCancellationToken();
        //    //RaisePropertyChanged(() => IsRefreshing);
        //}, () => _cancellationTokenSource != null);
        private ObservableCollection<DeviceListItemViewModel> _devices { get; set; } = new ObservableCollection<DeviceListItemViewModel>();
        public ObservableCollection<DeviceListItemViewModel> Devices
        {
            get => _devices;
            set
            {
                _devices = value;
                RaisePropertyChanged(() => Devices);
            }
        }
        public DeviceListItemViewModel SelectedDevice
        {
            get => null;
            set
            {
                if (value != null)
                {
                    HandleSelectedDevice(value);
                }

                RaisePropertyChanged(() => SelectedDevice);
            }
        }
        private string GetStateText()
        {
            switch (ble.State)
            {
                case BluetoothState.Unknown:
                    return "Unknown BLE state.";
                case BluetoothState.Unavailable:
                    return "BLE is not available on this device.";
                case BluetoothState.Unauthorized:
                    return "You are not allowed to use BLE.";
                case BluetoothState.TurningOn:
                    return "BLE is warming up, please wait.";
                case BluetoothState.On:
                    return "BLE is on.";
                case BluetoothState.TurningOff:
                    return "BLE is turning off. That's sad!";
                case BluetoothState.Off:
                    return "BLE is off. Turn it on!";
                default:
                    return "Unknown BLE state.";
            }
        }
        private bool _isScanning = true;
        public bool IsScanning
        {
            get => _isScanning;
            set
            {
                _isScanning = value;
                RaisePropertyChanged(() => IsScanning);
            }
        }
        private bool _enableBtn = false;
        public bool EnableBtn
        {
            get => _enableBtn;
            set
            {
                _enableBtn = value;
                RaisePropertyChanged(() => EnableBtn);
            }
        }
        public DeviceListViewModel()
        {
            ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;
            ble.StateChanged += OnStateChanged;
            adapter.DeviceDiscovered += OnDeviceDiscovered;
            //adapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
            adapter.DeviceDisconnected += OnDeviceDisconnected;
            adapter.DeviceConnectionLost += OnDeviceConnectionLost;
            //_devices = new ObservableRangeCollection<DeviceBLE>();
        }

        private void OnStateChanged(object sender, BluetoothStateChangedArgs e)
        {
            RaisePropertyChanged(() => IsStateOn);
            RaisePropertyChanged(() => StateText);
        }

        private async Task TryStartScanning()
        {
            IsScanning = false;
            if (Device.RuntimePlatform == Device.Android)
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                if (status != PermissionStatus.Granted)
                {
                    var permissionResult = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
                    if (permissionResult.First().Value != PermissionStatus.Granted)
                    {
                        await UserDialogs.Instance.AlertAsync("Permission denied. Not scanning.");
                        CrossPermissions.Current.OpenAppSettings();
                        return;
                    }
                }
            }
            if (IsStateOn)
                ScanForDevices();
            
        }
        private async void ScanForDevices()
        {
            Devices.Clear();
            //var dv = new ObservableCollection<DeviceListItemViewModel>();
            //Devices = new ObservableCollection<DeviceListItemViewModel>();
            foreach (var connectedDevice in adapter.ConnectedDevices)
            {
                try
                {
                    await connectedDevice.UpdateRssiAsync();
                }
                catch (Exception ex)
                {
                    Trace.Message(ex.Message);
                    await UserDialogs.Instance.AlertAsync($"Failed to update RSSI for {connectedDevice.Name}");
                }
                AddOrUpdateDevice(connectedDevice);
            }
            _cancellationTokenSource = new CancellationTokenSource();
            //RaisePropertyChanged(() => StopScanCommand);

            //await RaisePropertyChanged(() => IsRefreshing);
            adapter.ScanMode = ScanMode.LowLatency;
            //await SetBusyAsync(async () =>
            //{
            //    adapter.DeviceDiscovered += (s, a) =>
            //    {
            //        if (!string.IsNullOrEmpty(a.Device.Name) && !Devices.Select(x => x.Id).Contains(a.Device.Id))
            //            //if (!Devices.Select(x => x.Id).Contains(a.Device.Id))
            //        {
            //            Devices.Add(new DeviceListItemViewModel(a.Device));

            //        }
            //    };

            //    //We have to test if the device is scanning 
            //    if (!ble.Adapter.IsScanning)
            //    {
            //        await adapter.StartScanningForDevicesAsync();
            //    }
            //});                  
            await SetBusyAsync(async () =>
            {
                await adapter.StartScanningForDevicesAsync(_cancellationTokenSource.Token);
            });

            IsScanning = true;
            BleApplication.deviceList = Devices;
        }
        private async Task DisconnectAsync()
        {
            try
            {
                if (!BleApplication.deviceConnected.IsConnected)
                    return;
                UserDialogs.Instance.ShowLoading($"Disconnecting {BleApplication.deviceConnected.Name}...");
                await adapter.DisconnectDeviceAsync(BleApplication.deviceConnected.Device);
                EnableBtn = false;
                IsScanning = true;
                //await TryStartScanning();
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message, "Disconnect error");
            }
            finally
            {
                BleApplication.deviceConnected.Update();
                UserDialogs.Instance.HideLoading();
            }
        }
        private async Task AddTagsAsync()
        {
            EnableBtn = false;
            await NavigationService.SetDetailPageAsync(typeof(TagsManagementView), BleApplication.deviceConnected, pushToStack: true);
            
        }
        private async void HandleSelectedDevice(DeviceListItemViewModel device)
        {
            if (!device.IsConnected)
            {
                EnableBtn = false;
                if (await UserDialogs.Instance.ConfirmAsync($"Connect to device '{device.Name}'?"))
                {
                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    try
                    {
                        var configConnect = new ProgressDialogConfig()
                        {
                            Title = $"Connecting to '{device.Name}'",
                            CancelText = "Cancel",
                            IsDeterministic = false,
                            OnCancel = tokenSource.Cancel
                        };

                        using (var progress = UserDialogs.Instance.Progress(configConnect))
                        {
                            progress.Show();                            
                            await adapter.ConnectToDeviceAsync(device.Device, new ConnectParameters(autoConnect: false, forceBleTransport: true), tokenSource.Token);
                            BleApplication.deviceConnected = device;
                        }

                        await UserDialogs.Instance.AlertAsync($"Connected to {device.Device.Name}.");
                        await TryStartScanning();
                        //PreviousGuid = device.Device.Id;
                        //return true;
                        EnableBtn = true;
                        await NavigationService.SetDetailPageAsync(typeof(TagsManagementView), device, pushToStack: true);
                        
                    }
                    catch (Exception ex)
                    {
                        await UserDialogs.Instance.AlertAsync(ex.Message, "Connection error");
                        Trace.Message(ex.Message);
                        //return false;
                    }
                    finally
                    {
                        UserDialogs.Instance.HideLoading();
                        device.Update();
                        //BleApplication.deviceConnected.Update();
                        BleApplication.deviceConnected = device;
                        tokenSource.Dispose();
                        tokenSource = null;
                    }
                }
            }
            else
            {
                await NavigationService.SetDetailPageAsync(typeof(TagsManagementView), device, pushToStack: true);

            }
            
        }
        private void OnDeviceDiscovered(object sender, DeviceEventArgs args)
        {
            AddOrUpdateDevice(args.Device);
        }
        private void Adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            //RaisePropertyChanged(() => IsRefreshing);

            CleanupCancellationToken();
        }
        private void CleanupCancellationToken()
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            //RaisePropertyChanged(() => StopScanCommand);
        }
        private void StopCommand()
        {

        }
        private void OnDeviceDisconnected(object sender, DeviceEventArgs e)
        {
            Devices.FirstOrDefault(d => d.Id == e.Device.Id)?.Update();
            BleApplication.deviceList = Devices;
            UserDialogs.Instance.HideLoading();
            UserDialogs.Instance.Toast($"Disconnected {e.Device.Name}");

            Console.WriteLine($"Disconnected {e.Device.Name}");
        }
        private void OnDeviceConnectionLost(object sender, DeviceErrorEventArgs e)
        {
            Devices.FirstOrDefault(d => d.Id == e.Device.Id)?.Update();

            UserDialogs.Instance.HideLoading();
            UserDialogs.Instance.Toast($"Connection LOST {e.Device.Name}", TimeSpan.FromMilliseconds(6000));
        }
        private void AddOrUpdateDevice(IDevice device)
        {
            //MvvmCross.Base.MvxMainThreadDispatchingObject.InvokeOnMainThread(() =>
            //{
                var vm = Devices.FirstOrDefault(d => d.Device.Id == device.Id);
            if (vm != null)
            {
                vm.Update();
            }
            else
            {
                if (!string.IsNullOrEmpty(device.Name) && !Devices.Select(x => x.Id).Contains(device.Id))
                //if (!Devices.Select(x => x.Id).Contains(device.Id))
                {
                    Devices.Add(new DeviceListItemViewModel(device));
                    BleApplication.deviceList = Devices;
                }
            }
            //});
        }
        public async Task PageAppearingAsync()
        {
            if (Devices.Count == 0)
                Devices = BleApplication.deviceList;

            if (BleApplication.deviceConnected != null && BleApplication.deviceConnected.IsConnected)
                EnableBtn = true;
            else EnableBtn = false;
            //stop scan
            if (BleApplication._characteristicUpdate != null)
            {
                await BleApplication._characteristicUpdate.StopUpdatesAsync();                
            }
        }
    }
}
