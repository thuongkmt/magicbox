using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using MagicCashlessPayment.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicCashlessPayment.Core.Devices
{
    public class CardReaderInterface
    {
        #region COMMANDS
        private const byte REPORT_ID = 0x00;
        private const byte ACK = 0x06;
        private const byte NACK = 0x15;
        private const byte EOT = 0x04;
        private const byte STX = 0xF2;
        private const byte ETX = 0x03;
        private const byte COMMAND = 0x43;
        private const byte ORDINARY_RETURN = 0x50;
        private const byte IRREGULAR_RETURN = 0x4E;
        // HID Device
        private const int HID_VID = 0x23D8;
        private const int HID_PID = 0x0285;
        private HidStream HidStream;
        private byte[] HidInputReportBuffer;
        private HidDeviceInputReceiver HidDeviceInputReceiver;

        private Queue<byte[]> ResponseCommandsQueue = new Queue<byte[]>();
        private Queue<SendingCommand> SendingCommandQueue = new Queue<SendingCommand>();
        private Thread _queueThread;

        // Timeout
        private const int ACK_TIMEOUT = 5000;
        private const int CMD_TIMEOUT = 2000;
        private bool USE_QUEUE = true;
        private const int QUEUE_DELAY = 300;
        #endregion

        #region MyRegion
        public Action<string> LogHardware { get; set; }
        public Action<string> LogError { get; set; }
        public Action<string> LogInfo { get; set; }
        public static DeviceInterface CommunicationInterface { get; set; }

        public CardReaderInterface()
        {
            CommunicationInterface = DeviceInterface.HID;
        }

        #endregion
        public bool Connect(bool isRecon = false)
        {
            var tryTime = 0;

            RECON:
            var result = false;
            if (CommunicationInterface == DeviceInterface.HID)
            {
                try
                {
                    var list = DeviceList.Local;
                    var hidDevices = list.GetHidDevices().ToList();//.Where(x => x.VendorID == 11734 && x.ProductID == 417).ToList();
                    LogHardware?.Invoke("Found Cardholder HID Devices: " + hidDevices.Count);
                    HidDevice device = null;

                    foreach (var d in hidDevices)
                    {
                        try
                        {

                            LogHardware?.Invoke($"{d.DevicePath} | {d.GetFriendlyName()} | {d.GetProductName()}");
                            if (d.GetProductName() == "CRT-285")
                            {
                                LogHardware?.Invoke("Found CRT-285");

                                device = d;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHardware?.Invoke("Can't get device product name, try another one...");
                        }
                    }

                    // var devices = hidDevices.Where(x => x.GetProductName() == "CRT-285").ToList();
                    //if (devices.Count() > 0)
                    //{
                    //    device = devices[0];
                    //}
                    //if (devices.Count() > 1)
                    //{
                    //    result = false;
                    //    LogHardware?.Invoke("There are 2 card holder devices!!");
                    //}
                    if (device == null)
                    {
                        LogHardware?.Invoke("Failed to connect to card reader");
                        LogError?.Invoke("Failed to connect to card reader");
                        result = false;
                    }
                    else
                    {
                        LogHardware?.Invoke($"Found device: {device.GetProductName()}, Serial number: {device.GetSerialNumber()}");
                        var reportDescriptor = device.GetReportDescriptor();
                        HidInputReportBuffer = new byte[device.GetMaxInputReportLength()];

                        // Try to open
                        device.TryOpen(out HidStream);
                        if (HidStream == null)
                        {
                            LogHardware?.Invoke($"HID Stream is null, let try to reconnect");
                            if(++tryTime <=3)
                            {
                                Thread.Sleep(5000);
                                goto RECON;
                                
                            }
                        }

                        HidDeviceInputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();
                        HidDeviceInputReceiver.Received -= InputReceiver_Received;
                        HidDeviceInputReceiver.Received += InputReceiver_Received;
                        HidDeviceInputReceiver.Start(HidStream);

                        if(!isRecon)
                        {
                            if (USE_QUEUE)
                            {
                                // Start queue
                                _queueThread = new Thread(WaitForQueueCommand);
                                _queueThread.Start();
                            }
                        }
                      
                        result = true;

                        LastPoll = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    LogHardware?.Invoke(ex.ToString());

                    return false;
                }
            }
            return result;
        }

        #region HID Event
        private void InputReceiver_Received(object sender, EventArgs e)
        {
            Report report;
            while (HidDeviceInputReceiver.TryRead(HidInputReportBuffer, 0, out report))
            {
                ProcessCommand(HidInputReportBuffer);
            }
        }
        #endregion

        #region Parse Command
        private void ProcessCommand(byte[] command)
        {
            List<byte> receivedCommand = new List<byte>();
            if (CommunicationInterface == DeviceInterface.HID)
            {
                var cmd = TrimHidCommand(command);
                receivedCommand.AddRange(cmd);
            }

            ResponseCommandsQueue.Enqueue(receivedCommand.ToArray());
        }

        public byte[] TrimHidCommand(byte[] cmd)
        {
            var cmdList = new List<byte>();
            var returnData = new List<byte>();

            cmdList.AddRange(cmd);
            if (cmd.Length == 65)
            {
                // Remove report ID
                cmdList.RemoveAt(0);
                if (cmdList[0] == ACK || cmdList[0] == NACK || cmdList[0] == EOT)
                {
                    returnData.Add(cmdList[0]);
                }
                else
                {
                    var lengthBytes = cmdList.Skip(1).Take(2).ToArray();
                    var length = lengthBytes.BcdToInt();
                    var trimmed = cmdList.Take(length + 5).ToArray();
                    returnData.AddRange(trimmed);
                }
            }

            return returnData.ToArray();
        }
        #endregion

        #region Command
        //object _lockSend = new object();
        public void Initialize(bool lockCard = true)
        {
            var cmd = CommandsBuilder.Commands.Initialize.Lock();
            SendCommand(cmd, "TX: Initialize");
        }

        public void LedSolid(PM_LED_TYPE ledType, PM_LED_STATE ledState)
        {
            var cmd = CommandsBuilder.Commands.Led.Solid(ledType, ledState);
            SendCommand(cmd, $"TX: LedSolid | {ledType} | {ledState}");
        }
        public void LedBlink(PM_LED_TYPE ledType, PM_LED_BLINK_SPEED speed = PM_LED_BLINK_SPEED.NORMAL)
        {
            var cmd = CommandsBuilder.Commands.Led.Blink(ledType, speed);
            SendCommand(cmd, $"TX: LedBlink | {ledType}");
        }
        public void TurnOffAllLeds()
        {
            LedSolid(PM_LED_TYPE.GREEN, PM_LED_STATE.OFF);
            LedSolid(PM_LED_TYPE.RED, PM_LED_STATE.OFF);
        }

        public void LatchAutoLock(bool autoLock)
        {
            var cmd = autoLock == true ? CommandsBuilder.Commands.Latch.AutoLock() : CommandsBuilder.Commands.Latch.AutoRelease();
            SendCommand(cmd, $"TX: LatchAutoLock | {autoLock}");
        }

        public void OpenLatch(bool open)
        {
            var cmd = open == true ? CommandsBuilder.Commands.Latch.Release() : CommandsBuilder.Commands.Latch.Lock();
            SendCommand(cmd, $"TX: OpenLatch | {open}");
        }

        public void CurrentStatus(Action<byte[]> callback = null)
        {
            var cmd = CommandsBuilder.Commands.Status.Current();
            SendCommand(cmd, $"TX: Get Current Status", callback);
        }

        public void Ack()
        {
            try
            {
                var cmd = CommandsBuilder.Commands.System.Ack();
                HidStream?.Write(cmd);
                LogHardware?.Invoke("TX: ACK");
            }
            catch (Exception ex)
            {
                LogHardware?.Invoke(ex.ToString());
            }
        }

        private void SendCommand(byte[] cmd, string cmdName, Action<byte[]> callback = null)
        {
            if (CommunicationInterface == DeviceInterface.HID)
            {
                //lock (_lockSend)
                //{
                if (USE_QUEUE)
                {
                    QueueCommand(cmd, cmdName, callback);
                }
                else
                {
                    WriteCommand(cmd, cmdName, callback);
                }
                //}
            }
        }

        private void WaitForQueueCommand()
        {
            while (_queueThread.IsAlive)
            {
                Thread.Sleep(QUEUE_DELAY);
                if (SendingCommandQueue.Count > 0)
                {
                    var cmd = SendingCommandQueue.Dequeue();
                    WriteCommand(cmd.Command, cmd.Name, cmd.Callback);
                }
            }
        }
        private void QueueCommand(byte[] cmd, string cmdName, Action<byte[]> callback = null)
        {
            SendingCommandQueue.Enqueue(new SendingCommand(cmd, cmdName, callback));
        }

        public void CleanCommandQueue()
        {
            SendingCommandQueue?.Clear();
            ResponseCommandsQueue?.Clear();
        }

        public Tuple<int, int> GetQueueCount()
        {
            var v1 = SendingCommandQueue.Count;
            var v2 = ResponseCommandsQueue.Count;

            return new Tuple<int, int>(v1, v2);
        }
        public void WriteCommand(byte[] cmd, string cmdName, Action<byte[]> callback = null)
        {
            //lock (_lockSend)
            //{
            try
            {
                var timeOut = 0;
                var tryingTime = 0;

            RESEND:
                // 1. Write command
                HidStream.Write(cmd);
                //if (cmdName != "TX: Get Current Status")
                //{
                LogHardware?.Invoke($"TX: =====================[{cmdName}]=====================");
                LogHardware?.Invoke($"TX: {TrimHidCommand(cmd).ToHexString()}");
                //}


                // 2. Waiting for ACK
                while (true)
                {
                    Thread.Sleep(10);
                    timeOut += 10;
                    if (timeOut >= ACK_TIMEOUT)
                    {
                        // Resend command
                        if (++tryingTime < 3)
                        {
                            LogHardware?.Invoke("Timedout to get ACK, trying to resend command");
                            goto RESEND;
                        }
                        break;
                    }
                    // Try to get ACK
                    if (ResponseCommandsQueue.Count > 0)
                    {
                        var ack = ResponseCommandsQueue.Peek();
                        if (ack[0] != ACK)
                        {
                            if (++tryingTime < 3)
                            {
                                var message = $"Falied to get ACK, GOT {new byte[] { ack[0] }.ToHexString()}, trying to resend command";
                                LogError?.Invoke(message);
                                LogHardware?.Invoke(message);
                                goto RESEND;
                            }
                        }
                        else
                        {
                            ResponseCommandsQueue.Dequeue();
                            LogHardware?.Invoke("RX: ACK");
                            break;
                        }
                    }

                }

                // 3. Waiting for response
                timeOut = 0;
                tryingTime = 0;
                while (true)
                {
                    Thread.Sleep(10);
                    timeOut += 10;
                    if (timeOut >= CMD_TIMEOUT)
                    {
                        // Resend command
                        if (++tryingTime < 3)
                        {
                            LogHardware?.Invoke("Timedout to get RESPONSE, trying to resend command");
                            goto RESEND;
                        }
                        break;
                    }
                    if (ResponseCommandsQueue.Count > 0)
                    {
                        // Try to get ACK
                        var command = ResponseCommandsQueue.Peek();
                        if (command[0] != 0xF2)
                        {
                            if (++tryingTime < 3)
                            {
                                LogHardware?.Invoke($"Falied to get COMMAND, GOT {command.ToHexString()}, trying to resend command");
                                goto RESEND;
                            }
                        }
                        else
                        {
                            ResponseCommandsQueue.Dequeue();
                            LogHardware?.Invoke($"RX: {command.ToHexString()}");
                            callback?.Invoke(command);
                            break;
                        }
                    }
                }
                // 4. Send back ACK
                Ack();
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("The device is not connected"))
                {
                    LogHardware?.Invoke("Failed to connect to hardware - retry to reconnect");
                    Thread.Sleep(5000);
                    Connect(true);
                }
                LogHardware?.Invoke(ex.ToString());
            }
            //}

        }
        #endregion

        #region Bussiness

        private System.Timers.Timer _timer = new System.Timers.Timer();
        public OrdinaryResponse Status { get; set; }
        public DateTime LastPoll { get; set; }
        public void StartPoll(int interval)
        {
            void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                CurrentStatus((response) =>
                {
                    Status = new OrdinaryResponse(response);
                    LastPoll = DateTime.Now;
                    // LogInfo?.Invoke(Status.ToString());
                });
            }

            _timer.Elapsed -= _timer_Elapsed;
            _timer.Enabled = false;
            _timer.Stop();

            _timer.Interval = interval;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = true;
            _timer.Start();
        }

        public void StopPoll()
        {
            _timer.Enabled = false;
            _timer.Stop();
        }

        #endregion

        #region Command Builder
        private static class CommandsBuilder
        {
            public static class Commands
            {
                public static class Initialize
                {
                    public const byte CM = 0x30;
                    public static byte[] Lock()
                    {
                        return BuildCommand(CM, PM: 0x30);
                    }

                    public static byte[] Release()
                    {
                        return BuildCommand(CM, PM: 0x31);
                    }
                }

                public static class Latch
                {
                    public const byte CM = 0xB0;
                    public static byte[] Lock()
                    {
                        return BuildCommand(CM, PM: 0x30);
                    }
                    public static byte[] Release()
                    {
                        return BuildCommand(CM, PM: 0x31);
                    }
                    public static byte[] AutoLock()
                    {
                        return BuildCommand(CM, PM: 0x32);
                    }
                    public static byte[] AutoRelease()
                    {
                        return BuildCommand(CM, PM: 0x33);
                    }
                }

                public static class Led
                {
                    private const byte CM = 0x80;
                    private static PM_LED_BEHAVIOR _LED_BEHAVIOR;
                    public static byte[] Solid(PM_LED_TYPE ledType, PM_LED_STATE ledState)
                    {
                        _LED_BEHAVIOR = PM_LED_BEHAVIOR.SOLID;
                        var data = new byte[] { (byte)ledType, (byte)ledState };
                        return BuildCommand(CM, PM: (byte)_LED_BEHAVIOR, DATA: data);
                    }
                    public static byte[] Blink(PM_LED_TYPE ledType, PM_LED_BLINK_SPEED speed)
                    {
                        _LED_BEHAVIOR = PM_LED_BEHAVIOR.BLINK;
                        var data = new byte[] { (byte)ledType, (byte)speed, (byte)speed };
                        return BuildCommand(CM, PM: (byte)_LED_BEHAVIOR, DATA: data);
                    }
                }

                public static class Status
                {
                    private const byte CM = 0x31;
                    public static byte[] Current()
                    {
                        return BuildCommand(CM, PM: 0x30);
                    }
                }

                public static class System
                {
                    public static byte[] Ack()
                    {
                        var cmd = new byte[65];
                        cmd[1] = ACK;
                        return cmd;
                    }
                }

                private static byte[] BuildCommand(byte CM, byte? PM, byte[] DATA = null)
                {
                    var mainCmd = new List<byte>
                    {
                        COMMAND,
                        CM
                    };

                    if (PM != null)
                    {
                        mainCmd.Add((byte)PM);
                    }

                    if (DATA != null)
                    {
                        mainCmd.AddRange(DATA);
                    }

                    var lengthBytes = mainCmd.Count.IntToBcd();

                    // General
                    var generalCmd = new List<byte>();
                    generalCmd.Add(STX);
                    generalCmd.AddRange(lengthBytes);
                    generalCmd.AddRange(mainCmd);
                    generalCmd.Add(ETX);

                    var checksum = generalCmd.ToArray().CalculateLRC();
                    generalCmd.Add(checksum);

                    // Build CMD
                    var length = generalCmd.Count;
                    var cmd = new List<byte>();

                    if (CommunicationInterface == DeviceInterface.HID)
                    {
                        if (length > 64)
                        {
                            throw new ArgumentException("Command length > 64");
                        }
                    }
                    if (CommunicationInterface == DeviceInterface.HID)
                    {
                        cmd.Add(REPORT_ID);
                    }

                    // Main command
                    cmd.AddRange(generalCmd);

                    if (CommunicationInterface == DeviceInterface.HID)
                    {
                        cmd.AddRange(new byte[64 - length]);
                    }

                    return cmd.ToArray();
                }
            }
        }

        private class SendingCommand
        {
            public byte[] Command;
            public string Name;
            public Action<byte[]> Callback;

            public SendingCommand(byte[] cmd, string name, Action<byte[]> callback = null)
            {
                Command = cmd;
                Name = name;
                Callback = callback;
            }
        }

        public class ResponseCommand
        {
            public byte CM;
            public byte PM;
            public int LENGTH;
            public byte[] DATA;
        }

        public class OrdinaryResponse : ResponseCommand
        {
            public byte ST1 { get; set; }
            public byte ST0 { get; set; }
            public OrdinaryResponse(byte[] cmd)
            {
                if (cmd.Length < 8)
                {
                    throw new ArgumentException("Wrong command");
                }
                CM = cmd[4];
                PM = cmd[5];
                ST1 = cmd[6];
                ST0 = cmd[7];
                var length = new byte[] { cmd[2], cmd[3] };
                LENGTH = length.BcdToInt();

                DATA = cmd.ToList().Skip(8).Take(LENGTH + 8).ToArray();
            }

            public bool IsCardInserted()
            {
                var a = (StatusCode.ST0)ST0 == StatusCode.ST0.CARD_INSIDE_AND_IN_PLACE;
                return a;
            }
            public bool IsLatchLock()
            {
                return (StatusCode.ST1)ST1 == StatusCode.ST1.LATCH_LOCK;
            }
            public override string ToString()
            {
                return $"ST0: {(StatusCode.ST0)((byte)ST0)} | ST1: {(StatusCode.ST1)ST1} ";
            }
        }

        public class IrregularResponse : ResponseCommand
        {
            public byte E1;
            public byte E0;

            public IrregularResponse(byte[] cmd)
            {
                if (cmd.Length < 8)
                {
                    throw new ArgumentException("Wrong command");
                }
                CM = cmd[4];
                PM = cmd[5];
                E1 = cmd[6];
                E0 = cmd[7];
                var length = new byte[] { cmd[2], cmd[3] };
                LENGTH = length.BcdToInt();

                DATA = cmd.ToList().Skip(8).Take(LENGTH + 8).ToArray();
            }

        }
        public class StatusCode
        {
            public enum ST0
            {
                NO_CARD_INSIDE = 0x30,
                CARD_INSIDE_BUT_NOT_IN_PLACE = 0x31,
                CARD_INSIDE_AND_IN_PLACE = 0x32,
            }
            public enum ST1
            {
                LATCH_LOCK = 0x30,
                LATCH_RELEASE = 0x31
            }
        }
        public enum DeviceInterface
        {
            HID = 0,
            RS232
        }

        public enum PM_LED_TYPE
        {
            RED = 0x30,
            GREEN = 0x31
        }

        public enum PM_LED_BLINK_SPEED
        {
            NORMAL = 0x03,
            FAST = 0x01
        }

        public enum PM_LED_STATE
        {
            ON = 0x31,
            OFF = 0x30
        }

        public enum PM_LED_BEHAVIOR
        {
            SOLID = 0x30,
            BLINK = 0x31
        }
        #endregion


    }
}
