using Abp.Dependency;
using KonbiBrain.Common.Messages;
using KonbiBrain.Common.Messages.Payment;
using KonbiBrain.Messages;
using KonbiCloud.Common;
using KonbiCloud.Enums;
using KonbiCloud.SignalR;
using Konbini.Messages.Commands.RFIDTable;
using Konbini.Messages.Enums;
using Newtonsoft.Json;
using NsqSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;

namespace KonbiCloud.RFIDTable
{
    public class TableSettingsManager : ITableSettingsManager, IHandler
    {
        private readonly IMessageProducerService messageProducerService;
        private IRfidTableSignalRMessageCommunicator signalRCommunicator;
        private readonly IDetailLogService detailLogService;
        private readonly Consumer consumer;
        private CancellationTokenSource ctx;
        private bool isServiceRunning;
        private string[] PortAvailable => SerialPort.GetPortNames().OrderBy(el=>el).ToArray();

        private IUniversalCommands Command { get; set; }

        public string TableProcessName => "KonbiBrain.WindowServices.RFIDTable";

        public bool IsServiceRunning
        {
            get { return isServiceRunning; }
            set
            {
                if (isServiceRunning != value)
                {
                    isServiceRunning = value;

                    GetSettingsAsync();
                }

            }
        }

        private object commandLock = new object();
        private Guid instanceId = Guid.NewGuid();
        public event EventHandler<CommandEventArgs> DeviceFeedBack;

        public TableSettingsManager(IMessageProducerService messageProducerService, IRfidTableSignalRMessageCommunicator signalRCommunicator, IDetailLogService detailLogService)
        {
            this.messageProducerService = messageProducerService;
            this.signalRCommunicator = signalRCommunicator;
            this.detailLogService = detailLogService;

            //consumer = new Consumer(NsqTopics.RFID_TABLE_RESPONSE_TOPIC, NsqConstants.NsqDefaultChannel, new Config() { ClientID = Guid.NewGuid().ToString() });

            //consumer.AddHandler(this);
            //consumer.ConnectToNsqLookupd(NsqConstants.NsqUrlConsumer);

        }
        public async Task<CommandState> GetServiceInfosAsync()
        {
            var cmd = new ConfigCommand();
            cmd.CommandId = Guid.NewGuid();
            cmd.CommandObject.Action = "GetSettings";
            Command = cmd;
            return await SendCommandAsync();
            //ctx?.Cancel();
            //lock (commandLock)
            //{
            //    var cmd = new ConfigCommand();
            //    cmd.CommandId = Guid.NewGuid();
            //    cmd.CommandObject.Action = "GetSettings";
            //    Command = cmd;
            //    // Command.CommandId = Guid.NewGuid();
            //    ///var sendingSuccess = await Task.Run(() => { return nsqProducerService.SendPaymentCommand(cmd); });
            //    var result = messageProducerService.SendRfidTableCommand(Command);
            //    if (result)
            //    {
            //        Command.CommandState = CommandState.SendSuccess;
            //    }
            //    else
            //    {
            //        Command.CommandState = CommandState.Failed;

            //    }

            //}
            //ctx = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            //try
            //{

            //    while (Command.CommandState == CommandState.SendSuccess && Command.Command == UniversalCommandConstants.RfidTableConfiguration && !ctx.Token.IsCancellationRequested)
            //    {
            //        await Task.Delay(50, ctx.Token);
            //    }
            //    if (ctx.Token.IsCancellationRequested)
            //        Command.CommandState = CommandState.Cancelled;

            //}
            //catch (OperationCanceledException)
            //{
            //    Command.CommandState = CommandState.TimeOut;

            //}

            //return Command.CommandState;
        }
        private async Task<CommandState> SendCommandAsync()
        {
            ctx?.Cancel();
            lock (commandLock)
            {
             
                var result = messageProducerService.SendRfidTableCommand(Command);
                if (result)
                {
                    Command.CommandState = CommandState.SendSuccess;
                }
                else
                {
                    Command.CommandState = CommandState.Failed;

                }

            }
            ctx = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {

                while (Command.CommandState == CommandState.SendSuccess  && !ctx.Token.IsCancellationRequested)
                {
                    await Task.Delay(50, ctx.Token);
                }
                if (ctx.Token.IsCancellationRequested)
                    Command.CommandState = CommandState.Cancelled;

            }
            catch (OperationCanceledException)
            {
                Command.CommandState = CommandState.TimeOut;

            }

            return Command.CommandState;
        }
        public void HandleMessage(IMessage message)
        {
            // process for ACK message where  telling that the request  has been sent to device successfully.
            try
            {
                var msg = Encoding.UTF8.GetString(message.Body);
                var obj = JsonConvert.DeserializeObject<UniversalCommands>(msg);
                Debug.WriteLine(obj.Command);
                Debug.WriteLine(msg);
                if (obj.Command == UniversalCommandConstants.ACKResponse)
                {
                    if (Command != null && Command.CommandId == obj.CommandId)
                    {
                        Command.CommandState = CommandState.Received;
                    }
                }
                else if (obj.Command == UniversalCommandConstants.RfidTableConfiguration)
                {
                    var Command = JsonConvert.DeserializeObject<ConfigCommand>(msg);
                    if (Command.CommandObject.Action == "settingsResult")
                        signalRCommunicator.UpdateTableSettings(Command.CommandObject.selectedComPort, PortAvailable.ToList(), isServiceRunning);
                }
                else
                {
                    OnDeviceFeedBack(new CommandEventArgs() { CommandStr =msg, Command = JsonConvert.DeserializeObject<UniversalCommands<object>>(msg) });
                }
            }
            catch
            {
                // dont catch unexpected message
               
            }
           

        }

        public void LogFailedMessage(IMessage message)
        {
            // throw new NotImplementedException();
        }
        protected void OnDeviceFeedBack(CommandEventArgs e)
        {
            DeviceFeedBack?.Invoke(this, e);
        }

        public async Task GetSettingsAsync()
        {
            var result = await GetServiceInfosAsync();
            if (result == CommandState.TimeOut)
            {
                await signalRCommunicator.UpdateTableSettings(string.Empty, PortAvailable.ToList(), IsServiceRunning); // service is not running
            }
           
        }

        public bool StopService()
        {
            //var processes = Process.GetProcessesByName(TableProcessName);
            //if(processes!=null && processes.Length > 0)
            //{
            //    try
            //    {
            //        processes.First().Kill();
            //    }
            //    catch (Exception ex)
            //    {
            //        detailLogService.Log("Stop Konbi.RFIDTable  exception: " + ex.Message + " " + ex.StackTrace);

            //        return false;
            //    }

            //}

            try
            {
                var process = new Process();
                var startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.UseShellExecute = true;
                startInfo.Arguments = "/C sc stop Konbi.RFIDTable ";
                process.StartInfo = startInfo;
                process.Start();

            }
            catch (Exception ex)
            {
                detailLogService.Log("Stop Konbi.RFIDTable  exception: " + ex.Message + " " + ex.StackTrace);
                return false;
            }
            return true;
        }
        
        public bool StartService(string port)
        {
            
            try
            {
                var process = new Process();
                var startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.UseShellExecute = true;
                startInfo.Arguments = "/C sc start Konbi.RFIDTable " + port;
                process.StartInfo = startInfo;
                process.Start();
                
            }
            catch(Exception ex)
            {
                detailLogService.Log("Start Konbi.RFIDTable  exception: " + ex.Message + " " + ex.StackTrace);
                return false;
            }
            return true;
        }

        public async Task<bool> ForceToReadPlates()
        {
            var cmd = new ConfigCommand();
            cmd.CommandId = Guid.NewGuid();
            cmd.CommandObject.Action = "forceToReadPlates";
            Command = cmd;
            var result = await SendCommandAsync();
            if (result == CommandState.Received)
                return true;
            return false;
        }
    }
}
