using System;
using System.IO.Ports;
using System.Threading;
using Caliburn.Micro;
using KonbiBrain.Messages;
using KonbiBrain.Interfaces;
using NsqSharp;
using Konbi.Common.Interfaces;
using KonbiBrain.Common.Helpers;

namespace TemplateBrain.ViewModels
{
    public class ShellViewModel : Conductor<object>, IShell
    {
        private Consumer consumer;
        private string selectedPort;
        private double currentTemperature;
        private System.Timers.Timer timer = new System.Timers.Timer();

        public ChillerMachine ChillerMachine { get; set; }    
        public IMessageProducerService MessageProducerService { get; set; }
        public IKonbiBrainLogService KonbiBrainLogService { get; set; }
       

        protected override void OnActivate()
        {
            try
            {
              
                timer.Interval = 1 * 60 * 1000;//5 minutes;
                //timer.Interval = 1000;
                timer.Elapsed += Timer_Elapsed;

                consumer = new Consumer(NsqTopics.TEMPERATURE_MESSAGE_TOPIC, NsqConstants.NsqDefaultChannel);
                consumer.AddHandler(new MessageHandler(this));
                Console.WriteLine(NsqConstants.NsqUrlConsumer);
                consumer.ConnectToNsqLookupd(NsqConstants.NsqUrlConsumer);


                //TODO: Setting from local server
                //var setting = TemperatureSettingDataService.Single();
                //SelectedPort = setting.Port;
                //CurrentTemperature = setting.CurrentTemperature;
                //ChangingTemperature = setting.NormalTemperature;

                Connect();
                Start();
            }

            catch(Exception ex)
            {
                KonbiBrainLogService.LogTemperatureDeviceError(ex);
            }

        }
        protected override void OnDeactivate(bool close)
        {
            consumer.Stop();
            ChillerMachine.StopRefrigerator();
            ChillerMachine.Close();
            base.OnDeactivate(close);
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var temp = ChillerMachine.GetCurrentTemperature();
            if (temp == 0)
            {
                var cmd1 = new TemperatureCommands.CurrentTemprature();
                MessageProducerService.SendToCustomerUICommand(cmd1);
                return;
            }
            //var setting = TemperatureSettingDataService.Single();
            //setting.CurrentTemperature = temp;
            //TemperatureSettingDataService.Update(setting);
            //CurrentTemperature = temp;

            //var machine = MachineStatusEntityService.Single();
            //var cloudEntity = new KonbiBrain.DocumentDb.LiteDb.TemperatureLog()
            //{
            //    LocalDateTime = DateTime.Now,
            //    Temperature = temp,
            //    MachineId = Guid.Parse(machine.MachineId),
            //    MachineLogicalId = machine.LogicalID,
            //    UtcDateTime = DateTime.UtcNow
            //};

           //KonbiBrainLogService.LogTemperatureDeviceError("Temperature "+cloudEntity.Temperature);
            //Task.Factory.StartNew(() => KonbiCloudWebApiClientService.InsertTemperatureLog(cloudEntity)).LogTaskExceptions();

        
        }

        public string SelectedPort
        {
            get { return selectedPort; }
            set
            {
                if (selectedPort != value)
                {
                    selectedPort = value;
                    NotifyOfPropertyChange("SelectedPort");
                    //var setting = TemperatureSettingDataService.Single();
                    //setting.Port = SelectedPort;
                    //TemperatureSettingDataService.Update(setting);
                }
            }
        }

        public string[] PortList
        {
            get
            {
                var tempports = SerialPort.GetPortNames();
                return tempports;
            }
        }

        public double CurrentTemperature
        {
            get { return currentTemperature; }
            set
            {
                currentTemperature = value;
                NotifyOfPropertyChange();
            }
        }
        public int ChangingTemperature { get; set; }
        public void SetTemperature()
        {
            ChillerMachine.SetTemperature(ChangingTemperature);
        }

        public void SetTemperature(int temp)
        {
            ChangingTemperature = temp;
            ChillerMachine.SetTemperature(temp);
        }

        public void GetCurrentTemperature()
        {
            var temp = ChillerMachine.GetCurrentTemperature();            
            if (temp == 0)
            {
                var cmd1 = new TemperatureCommands.CurrentTemprature();
                MessageProducerService.SendToCustomerUICommand(cmd1);
                return;
            }
            //var setting = TemperatureSettingDataService.Single();
            //setting.CurrentTemperature = temp;
            //TemperatureSettingDataService.Update(setting);
            CurrentTemperature = temp;
           

        
            var cmd = new TemperatureCommands.CurrentTemprature()
            {
                CurrentTemperature = temp
            };
            MessageProducerService.SendToKonbiBrainCommand(cmd);
            MessageProducerService.SendToCustomerUICommand(cmd);
        }
        public void Connect()
        {
            ChillerMachine.StartRefrigerator(SelectedPort);
            
        }

        public void Start()
        {
            var temp = ChillerMachine.GetCurrentTemperature();
            //var setting = TemperatureSettingDataService.Single();
            //setting.CurrentTemperature = temp;
            //TemperatureSettingDataService.Update(setting);
            CurrentTemperature = temp;
            timer.Start();
            ChillerMachine.Open();
            Thread.Sleep(1000);
        }

        public void Stop()
        {
            ChillerMachine.StopRefrigerator();
            ChillerMachine.Close();
        }
        
        public void DisposeObjects()
        {
            ChillerMachine.StopRefrigerator();
            ChillerMachine.Close();
        }

        public System.Windows.Forms.UserControl GetControl()
        {
            return ChillerMachine.GetControlEn();
        }
    }
}