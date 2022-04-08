using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using Caliburn.Micro;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
//using Microsoft.Owin.Hosting;
using Newtonsoft.Json;

namespace RawInputBrain.ViewModels
{
    using System.Collections.Generic;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;
    using System.Windows.Threading;
    using System.Configuration;
    using static RawInputBrain.RawInputInterface;
    using System.Net.Http;

    public class ShellViewModel : Conductor<object>, IShell, IHandle<object>, IDisposable
    {

        #region Member Data

        private IDisposable webApiServer;


        private ObservableCollection<string> notificationList;

        private System.Timers.Timer _statusTimer;

        #endregion

        public ShellViewModel()
        {
            NotificationList = new ObservableCollection<string>();
        }

        #region Properties

        public Window CurrentWindow { get; set; }
        public RawInputInterface RawInputInterface1 { get; set; }

        private string _rawInputSelectedDevice;

        private List<RawInputDevice> _device;

        private RawInputDevice _selectedDevice;

        public string RawInputSelectedDevice
        {
            get
            {
                return _rawInputSelectedDevice;
            }
            set
            {
                if (_rawInputSelectedDevice != value)
                {
                    _rawInputSelectedDevice = value;
                    NotifyOfPropertyChange(() => RawInputSelectedDevice);
                    //KeyValueSettingsService.SetValue(SettingKey.RawInputSelectedDevice, this.RawInputSelectedDevice);

                }
            }
        }

        public ObservableCollection<string> NotificationList
        {
            get
            {
                return notificationList;
            }
            set
            {
                notificationList = value;
            }
        }

        public List<RawInputDevice> Devices
        {
            get
            {
                return this._device;
            }
            set
            {
                if (Equals(value, this._device)) return;
                this._device = value;
                this.NotifyOfPropertyChange(() => this.Devices);
            }
        }

        public RawInputDevice SelectedDevice
        {
            get
            {
                return this._selectedDevice;
            }
            set
            {
                if (Equals(value, this._selectedDevice)) return;
                this._selectedDevice = value;
                this.NotifyOfPropertyChange(() => this.SelectedDevice);
                // SetSetting("PID", SelectedDevice.ProductId);
            }
        }

        public void AppendNotification(string msg)
        {
            Execute.OnUIThread(
            () =>
            {
                if (notificationList.Count == 100) notificationList.RemoveAt(0);
                NotificationList.Add(msg);
            });
        }


        #endregion



        protected override void OnActivate()
        {
            try
            {
                base.OnActivate();
                RawInputInterface1 = IoC.Get<RawInputInterface>();
                // KeyValueSettingsService.SetValue(SettingKey.RawInputBrainLocation, location);

                this.NotificationList = new ObservableCollection<string>() { "Raw Input v1.2" };
                Task.Factory.StartNew(() => this.Start());
                RawInputInterface1.OnKeyPress = OnKeyPress;
                // SetupWebApiServer();
            }
            catch (Exception ex)
            {
                // KonbiBrainLogService.LogException(ex);
            }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
        }

        private string _data { get; set; }
        public void OnKeyPress(Keys key, string s)
        {
            // AppendNotification($"Data: {s} | {key}");

            //Console.WriteLine(key);
            if (key == Keys.Enter)
            {
                AppendNotification($"Data: {_data} | {DateTime.Now}");
                SendResultToMain(_data);
                _data = string.Empty;
               // Thread.Sleep(3000);
            }
            else
            {
                _data += s;//.Replace("D", string.Empty);
            }
            // Thread.Sleep(100);
        }

        private void SetupWebApiServer()
        {
            var url = "http://localhost:9919/";
           // webApiServer = WebApp.Start<WebApiStartup>(url);
        }

        protected override void OnDeactivate(bool close)
        {
            webApiServer?.Dispose();
            base.OnDeactivate(close);
        }

        public void DisposeObjects()
        {
            // consumer.Stop();
        }

        public void RegisterDevice()
        {

            //var w = ModuleManagementService.GetRawInputWindow();
            RawInputInterface1.RegisterDevice(SelectedDevice, null,
                () =>
                    {
                        AppendNotification($"Register device {this._selectedDevice.FriendlyName} success");
                    },
                () =>
                    {
                        AppendNotification($"Register device {this._selectedDevice.FriendlyName} failed");
                    });
            //KeyValueSettingsService.SetValue(SettingKey.RawInputSelectedDevice, SelectedDevice.FriendlyName);
            SetSetting("PID", SelectedDevice.ProductId);
        }

        public void Start()
        {

            try
            {
                var tryTime = 0;
                RETRY:

                Devices = new List<RawInputDevice>(RawInputInterface1.GetDevicesList());
                //var selectedDevice = KeyValueSettingsService.GetValue(SettingKey.RawInputSelectedDevice);
                var pid = GetSetting("PID");
                SelectedDevice = Devices.FirstOrDefault(x => x.ProductId == pid);
                if (SelectedDevice != null)
                {
                    RegisterDevice();
                }
                else
                {
                    AppendNotification($"Device {SelectedDevice} not found.");
                    Thread.Sleep(5000);
                    if(++tryTime >= 30)
                    {
                        AppendNotification($"Failed to connect to hardware");
                        return;
                    }
                    goto RETRY;
                }
            }
            catch (Exception e)
            {
                //KonbiBrainLogService.LogException(e);
            }

        }

        public void Handle(object message)
        {
            if (message is string) AppendNotification(DateTime.Now + " " + (string)message);
        }

        private string _fakeQrText;
        public string FakeQrText
        {
            get
            {
                return _fakeQrText;
            }
            set
            {
                _fakeQrText = value;
                NotifyOfPropertyChange(() => FakeQrText);
            }
        }

        public void FakeQr()
        {
            SendResultToMain(FakeQrText);
        }


        private static readonly HttpClient client = new HttpClient();
        private void SendResultToMain(string qr)
        {
            Task.Run(() =>
            {
                try
                {
                    AppendNotification("Checking QR: " + qr);
                    var response = client.PostAsync("http://localhost:9000/api/machine/authqr/validate/" + qr, null);
                    Console.WriteLine(response.Result.ToString());
                    var responseString = response.Result.Content.ReadAsStringAsync().Result;
                    AppendNotification("Result: " + responseString);
                }
                catch (Exception ex)
                {
                    AppendNotification(ex.Message);
                }
            });
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        private static string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        private static void SetSetting(string key, string value)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("appSettings");
        }

        //public class RawInputDevice
        //{
        //    public string FriendlyName;
        //    public string Name;
        //    public string Manufacturer;
        //    public string ProductId;
        //    public string VendorId;
        //    public string Version;
        //    public object Device;

        //    public override string ToString()
        //    {
        //        return $"{Manufacturer} - {FriendlyName} PID:{ProductId}, VID:{VendorId}";
        //    }
        //}
    }
}