using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Konbi.Common.Interfaces;
using NsqSharp;
using System.Threading.Tasks;
using System.Windows;
using KonbiBrain.Interfaces;
using KonbiBrain.Messages;
using StompSharp;
using StompSharp.Messages;
using System.Configuration;
using System.Text;
using System.Windows.Forms;

namespace RawInputBrain.ViewModels
{


    public class ShellViewModel : Conductor<object>, IShell, IHandle<object>, IDisposable
    {
        private StompClient client;
        #region Member Data

        private IDisposable webApiServer;

        private Consumer consumer;

        private ObservableCollection<string> notificationList;

        private string qrCodeInput = "";

        #endregion

        public ShellViewModel()
        {
            NotificationList = new ObservableCollection<string>();
            client= new StompClient(ConfigurationManager.AppSettings["RabbitMqServerIp"], 61613);
        }

        #region Properties

    
        public IMessageProducerService NsqMessageProducerService { get; set; }

        public IKonbiBrainLogService KonbiBrainLogService { get; set; }

        public Window CurrentWindow { get; set; }
        public RawInputInterface RawInputDevice { get; set; }

        private string _rawInputSelectedDevice;

        private ObservableCollection<RawInputDevice> _device;

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

                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["DeviceFriendlyName"].Value = SelectedDevice.FriendlyName;
                    config.Save(ConfigurationSaveMode.Modified);

                    ConfigurationManager.RefreshSection("appSettings");
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

        public ObservableCollection<RawInputDevice> Devices
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


        public void SendRabbitMq()
        {
            Task.Factory.StartNew(() => { SendMessage("HADOAN_CONFIRM_OPEN").Wait(); });
        }
        #endregion



        protected override void OnActivate()
        {
            try
            {
                base.OnActivate();

                var location = System.Reflection.Assembly.GetExecutingAssembly().Location;

                consumer = new Consumer(NsqTopics.MDB_MESSAGE_TOPIC, NsqConstants.NsqDefaultChannel);
                consumer.AddHandler(new MessageHandler(this));
                Console.WriteLine(NsqConstants.NsqUrlConsumer);
                consumer.ConnectToNsqLookupd(NsqConstants.NsqUrlConsumer);
                this.NotificationList = new ObservableCollection<string>() { "Raw Input v1.10" };
                Task.Factory.StartNew(() => this.Start());
                RawInputDevice.OnKeyPress = OnKeyPress;
            }
            catch (Exception ex)
            {
                KonbiBrainLogService.LogException(ex);
            }
        }

        private string _number { get; set; }
        public void OnKeyPress(System.Windows.Forms.Keys key)
        {
            
            if (key == Keys.Enter)
            {
                AppendNotification($"Number: {_number} | {DateTime.Now}");
                //SendResultToMain(qrCodeInput);

                //hardcode for demo, if qrcode is "HADOAN", send messge to pwd to open fridge
                if(!string.IsNullOrEmpty(qrCodeInput) && qrCodeInput.ToUpper().Equals("HADOAN"))
                    Task.Factory.StartNew(() => { SendMessage("HADOAN_CONFIRM_OPEN").Wait(); });

                qrCodeInput = string.Empty;
            }
            else
            {
                _number += key.ToString().Replace("D", string.Empty);
                qrCodeInput += key.ToString();
            }
        }

    

        protected override void OnDeactivate(bool close)
        {
            consumer?.Stop();
            webApiServer?.Dispose();
            base.OnDeactivate(close);
        }

        public void DisposeObjects()
        {
            consumer.Stop();
        }

        public void RegisterDevice()
        {

            RawInputDevice.RegisterDevice(SelectedDevice, this.CurrentWindow,
                () =>
                    {
                        AppendNotification($"Register device {this._selectedDevice.FriendlyName} success");
                    },
                () =>
                    {
                        AppendNotification($"Register device {this._selectedDevice.FriendlyName} failed");
                    });

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["DeviceFriendlyName"].Value = SelectedDevice.FriendlyName;
            config.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");
        }

        public void Start()
        {

            try
            {
                Devices = new ObservableCollection<RawInputDevice>(RawInputDevice.GetDevicesList());
                var selectedDevice = ConfigurationManager.AppSettings["DeviceFriendlyName"];
                SelectedDevice = Devices.FirstOrDefault(x => x.FriendlyName == selectedDevice);
                if (SelectedDevice != null)
                {
                    RegisterDevice();
                }
                else
                {
                    AppendNotification($"Device {selectedDevice} not found.");
                }
            }
            catch (Exception e)
            {
                KonbiBrainLogService.LogException(e);
            }

        }

        public void Handle(object message)
        {
            if (message is string) AppendNotification(DateTime.Now + " " + (string)message);
        }



        private void SendResultToMain(string number)
        {
            throw new NotImplementedException();
            //var cmd = new CommonCommands.SubmitCardNumber { CardNumber = number };
            //NsqMessageProducerService.SendToCustomerUICommand(cmd);
        }

        public void Dispose()
        {
            client?.Dispose();
        }


        private async  Task SendMessage(string message)
        {
          

            var destination = client.GetDestination("/topic/demo", client.SubscriptionBehaviors.AutoAcknowledge);

            IReceiptBehavior receiptBehavior =
                new ReceiptBehavior(destination.Destination, client.Transport.IncommingMessages);
            receiptBehavior = NoReceiptBehavior.Default;

            var bodyOutgoingMessage =
                (new BodyOutgoingMessage(Encoding.ASCII.GetBytes(message))).WithPersistence();
           

            await destination.SendAsync(bodyOutgoingMessage, receiptBehavior);
           
        }

    }
}