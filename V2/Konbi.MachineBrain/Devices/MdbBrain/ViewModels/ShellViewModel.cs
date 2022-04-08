using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO.Ports;
using System.Threading;
using Caliburn.Micro;
using Konbi.Common.Interfaces;
using KonbiBrain.Messages;
using NsqSharp;
using System.Threading.Tasks;

namespace MdbCashlessBrain.ViewModels
{
    public class ShellViewModel : Conductor<object>, IShell, IHandle<object>
    {

        #region Member Data
        
        private Consumer consumer;
        
        private ObservableCollection<string> notificationList;
        private System.Timers.Timer CheckMDBStateTimer;
        private bool _MDBInCheckingState;
        private System.Timers.Timer _checkDeviceTimer { get; set; }

        #endregion

        public ShellViewModel()
        {
            NotificationList = new ObservableCollection<string>();
        }

        #region Properties

        public IKonbiBrainLogService KonbiBrainLogService { get; set; }
        public MdbProcessingService MdbDevice { get; set; }
        

        private string selectedPort;
        public string SelectedPort
        {
            get
            {
                if (string.IsNullOrEmpty(selectedPort))
                {
                    selectedPort = ConfigurationManager.AppSettings["Port"];
                }
                return selectedPort;
            }
            set { selectedPort = value; }
        }

        public string[] PortList
        {
            get
            {
                var tempports = SerialPort.GetPortNames();
                return tempports;
            }
        }

        public void ResetAndWaitPurchase()
        {

            MdbDevice.SendPurchase(0.01);

        }
        public void EndTransaction()
        {

        }

        public double ResetAndWaitPurchaseValue { get; set; }
        public ObservableCollection<string> NotificationList
        {
            get { return notificationList; }
            set { notificationList = value; }
        }

        public bool DisableReader()
        {
            var result = MdbDevice.DisableAndCheckResponse();
            AppendNotification("DisableAndCheckResponse | Result : " + result);
            return result;
        }

        public void AppendNotification(string msg)
        {
            //KonbiBrainLogService.LogInfo($"MDB: {msg}");
            Execute.OnUIThreadAsync(() =>
            {
                if (notificationList.Count == 100) notificationList.RemoveAt(0);
                NotificationList.Add(msg);
            });
        }

        #endregion

        #region Methods

        protected override void OnActivate()
        {
            try
            {
                base.OnActivate();
              
                consumer = new Consumer(NsqTopics.PAYMENT_REQUEST_TOPIC, NsqConstants.NsqDefaultChannel);
                consumer.AddHandler(new PaymentRequestMessageHandler(this));
                Console.WriteLine(NsqConstants.NsqUrlConsumer);
                consumer.ConnectToNsqLookupd(NsqConstants.NsqUrlConsumer);
                this.NotificationList = new ObservableCollection<string>() { "Hello!" };


                NotifyOfPropertyChange(nameof(PortList));
                NotifyOfPropertyChange(nameof(SelectedPort));
                Task.Factory.StartNew(() => Start());

            }
            catch (Exception ex)
            {
                KonbiBrainLogService.LogException(ex);
            }

        }

      
        protected override void OnDeactivate(bool close)
        {
            consumer?.Stop();
            base.OnDeactivate(close);
        }


        public void DisposeObjects()
        {
            consumer.Stop();
        }

        public void Start()
        {
            try
            {
                MdbDevice.SetPort(SelectedPort);
                MdbDevice.StartBackgroundWorker();
                Thread.Sleep(3000);
                Task.Factory.StartNew(() => MdbDevice.InitMdb());
            }
            catch (Exception e)
            {
               this.KonbiBrainLogService.LogException(e);
            }
          
        }

        public void Stop()
        {
            MdbDevice.Stop();
        }


        public void Handle(object message)
        {
            if (message is string)
                AppendNotification(DateTime.Now + " " + (string)message);
        }
        #endregion


    }
}