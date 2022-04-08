using Konbini.RfidFridge.Common;
using Konbini.RfidFridge.Domain;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Konbini.RfidFridge.Service.Core
{
    public class QrReaderTtlInterface
    {
        public string ComportName;

        public SerialPort Port;

        public QrPaymentService QrPaymentService;
        public LogService LogService;
        public DeviceCheckingService DeviceCheckingService;
        public SlackService SlackService;


        public bool DebugMode = true;
        public bool DeviceIsWorking = false;

        public int PollingInterval = 60 * 1000 * 5;
        private DateTime DeviceLastPolling = DateTime.Now;

        private System.Timers.Timer _hearBeatTimer = new System.Timers.Timer();

        public QrReaderTtlInterface(QrPaymentService qrPaymentService, LogService logService, DeviceCheckingService deviceCheckingService, SlackService slackService)
        {
            QrPaymentService = qrPaymentService;
            LogService = logService;
            DeviceCheckingService = deviceCheckingService;
            SlackService = slackService;
        }

        public void Disconnect()
        {
            Port?.Close();
        }
        public bool Connect(string port, System.Action<string> action = null, bool isRecon = false)
        {
            var result = false;

            if (string.IsNullOrEmpty(port))
            {
                port = "USB";
            }

            DeviceCheckingService.AddToChecklist(Domain.Enums.DeviceChecking.DeviceName.QRCODE_READER, port);
            DeviceCheckingService.UpdateStatus(Domain.Enums.DeviceChecking.DeviceName.QRCODE_READER, Domain.Enums.DeviceChecking.DeviceStatus.CHECKING);

            if (port == "USB")
            {
                DeviceCheckingService.UpdateStatus(Domain.Enums.DeviceChecking.DeviceName.QRCODE_READER, Domain.Enums.DeviceChecking.DeviceStatus.OK);
                return true;
            }

            try
            {
                if (!isRecon)
                {
                    StartHearbeatChecking();
                }


                Port = new SerialPort(port)
                {
                    BaudRate = 9600,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    DtrEnable = true,
                };
                Port.Open();

                System.Action StartReadData = null;
                byte[] buffer = new byte[2000];
                StartReadData = (() => Port.BaseStream.BeginRead(buffer, 0, buffer.Length,
                delegate (IAsyncResult ar)
                {
                    try
                    {
                        int count = Port.BaseStream.EndRead(ar);
                        byte[] dst = new byte[count];
                        Buffer.BlockCopy(buffer, 0, dst, 0, count);
                        RaiseAppSerialDataEvent(dst);
                    }
                    catch (Exception ex)
                    {
                        LogService.LogQrReader(ex.ToString());
                    }
                    if (Port.IsOpen) StartReadData();
                }, null));

                if (Port.IsOpen)
                {
                    StartReadData();
                }

                ComportName = port;

                var msg = string.Empty;

                if (CheckStatus())//Port.IsOpen
                {
                    DeviceLastPolling = DateTime.Now;
                    DeviceCheckingService.UpdateStatus(Domain.Enums.DeviceChecking.DeviceName.QRCODE_READER, Domain.Enums.DeviceChecking.DeviceStatus.OK);
                    msg = $"QR Reader found at [{ComportName}]";
                    LogService.LogQrReader(msg);
                    result = true;
                }
                else
                {
                    DeviceCheckingService.UpdateStatus(Domain.Enums.DeviceChecking.DeviceName.QRCODE_READER, Domain.Enums.DeviceChecking.DeviceStatus.ERROR);
                    msg = $"Failed to connect to QR Reader at [{ComportName}]";
                    LogService.LogQrReader(msg);
                    result = false;
                }



                LogService.LogQrReader(msg);
                action?.Invoke(msg);

            }
            catch (Exception ex)
            {
                DeviceCheckingService.UpdateStatus(Domain.Enums.DeviceChecking.DeviceName.QRCODE_READER, Domain.Enums.DeviceChecking.DeviceStatus.ERROR);

                LogService.LogQrReader(ex.ToString());
                Port.Dispose();
            }

            return result;
        }

        public void StartHearbeatChecking()
        {
            _hearBeatTimer.Interval = PollingInterval;
            _hearBeatTimer.Stop();
            _hearBeatTimer.Enabled = false;
            _hearBeatTimer.Elapsed += _hearBeatTimer_Elapsed;

            LogService.LogQrReader("Start Timer to check heartbeat");

            _hearBeatTimer.Enabled = true;
            _hearBeatTimer.Start();
        }

        private void _hearBeatTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CheckStatus();
        }

        private StringBuilder _cmdBuilder = new StringBuilder();

        private void RaiseAppSerialDataEvent(byte[] bytes)
        {
            if (DebugMode)
            {
                LogService.LogQrReader("<<< " + bytes.ToArray().ToHexString());
            }

            var cmdHex = bytes.ToHexStringNoSpace();

            if (!string.IsNullOrEmpty(cmdHex))
            {
                var currentCmd = _cmdBuilder.Append(cmdHex);

                LogService.LogQrReader("currentCmd " + currentCmd);
                var datas = new List<string>();

                var currentCmdInString = currentCmd.ToString().StringToByteArray().ToAsiiString();
                LogService.LogQrReader("currentCmdInString " + currentCmdInString);

                if (currentCmdInString.Contains("$010370-4C10"))
                {
                    DeviceLastPolling = DateTime.Now;

                    LogService.LogQrReader("Got Hearbeat response, device is running.");
                    _cmdBuilder.Clear();
                    DeviceIsWorking = true;
                    return;
                }
                if (currentCmd.ToString().Contains("0D0A"))
                {
                    datas = currentCmd.ToString().Split(new[] { "0D0A" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (datas.Count() > 0)
                    {
                        _cmdBuilder.Clear();
                        var data = datas[0].StringToByteArray();
                        OnQrScanned(data);
                        return;
                    }
                }
            }
        }

        private void OnQrScanned(byte[] data)
        {
            LogService.LogQrReader($"<---- {data.ToHexString()}");
            var qrCode = data.ToAsiiString();
            if (GlobalAppData.IsTransacting)
            {
                LogService.LogInfo($"TXN ALREADY STARTED");
            }
            else
            {
                QrPaymentService.Validate(qrCode);
            }

        }

        public bool CheckStatus()
        {
            if (GlobalAppData.CurrentMachineStatus != Domain.Enums.MachineStatus.IDLE)
            {
                LogService.LogQrReader("Machine is not in IDLE, skip to check.");
                return true;
            }
            DeviceIsWorking = false;
            var cmd = Encoding.ASCII.GetBytes("$01036F-D9DF");
            WritePortData(cmd);

            var t = 0;
            while (!DeviceIsWorking)
            {
                if (++t >= 10)
                {
                    LogService.LogQrReader("Device is not running!!!.");
                    SlackService.SendAlert(RfidFridgeSetting.Machine.Name, "QR Reader is not working!");
                    return false;
                }
                Thread.Sleep(200);
            }
            return true;
        }

        private void WritePortData(byte[] bytes)
        {

            try
            {
                LogService.LogQrReader($"----> {bytes.ToHexString()}");
                var rawData = bytes.ToList();

                if (Port.IsOpen)
                {
                    Port?.Write(bytes, 0, bytes.Length);
                }
                else
                {
                    LogService.LogQrReader("Port is closed, retry to send command");
                    Connect(Port.PortName, isRecon: true);
                    Port?.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                LogService.LogQrReader(ex.ToString());
            }
        }

    }


}
