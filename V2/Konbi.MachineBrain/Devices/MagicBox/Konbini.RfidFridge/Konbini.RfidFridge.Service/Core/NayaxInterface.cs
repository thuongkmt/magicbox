using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Konbini.RfidFridge.Service.Core
{
    using Konbini.Messages.Enums;
    using Konbini.RfidFridge.Domain.Enums;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using PaymentType = Domain.Enums.PaymentType;
    using Timer = System.Timers.Timer;

    public class NayaxInterface
    {
        private bool HasSentConfig = false;
        private bool HasDoneStartup = false;
        private bool EnableDebug = false;
        public bool AutoCompleteSession { get; set; }
        public Action<string> LastMdbState { get; set; }
        public Action<PaymentType>  OnValidateCardSuccess;
        public Mdb.Response CurrentMdbState { get; set; }

        private LogService LogService;
        public NayaxInterface(LogService logService)
        {
            LogService = logService;
            AutoCompleteSession = true;
        }

        #region Command Struct
        private byte CMD_STRUCT_PACKET_OPTION = 0x01;
        private byte CMD_STRUCT_SOURCE = 0x01;
        private byte CMD_STRUCT_SOURCE_LSB = 0x53;
        private byte CMD_STRUCT_DES = 0x00;
        private byte CMD_STRUCT_DES_LSB = 0x39;
        #endregion


        #region Serial Port
        SerialPort Port;
        private StringBuilder _cmdBuilder = new StringBuilder();
        private Timer _aliveTimer = new Timer();

        public bool Connect(string port)
        {
            try
            {
                Port = new SerialPort(port)
                {
                    BaudRate = 115200,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    DtrEnable = true,
                };
                Port.Open();

                if (!Port.IsOpen) return false;

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
                    }
                    if (Port.IsOpen) StartReadData();
                }, null));
                if (Port.IsOpen) StartReadData();

                return Port.IsOpen;
            }
            catch (Exception ex)
            {
                LogService.LogError(ex.ToString());
                Port?.Dispose();
            }

            return false;
        }

        public void KeepDeviceAlive(int interval = 1000)
        {
            _aliveTimer = new Timer();
            _aliveTimer.Interval = interval;
            _aliveTimer.Enabled = true;
            _aliveTimer.Elapsed -= _aliveTimer_Elapsed;
            _aliveTimer.Elapsed += _aliveTimer_Elapsed;
            _aliveTimer.Start();
        }

        public void StopKeepDeviceAlive()
        {
            _aliveTimer.Enabled = false;
            _aliveTimer.Stop();
        }

        private void _aliveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.KeepAlive();
        }

        private void RaiseAppSerialDataEvent(byte[] Data)
        {
            if (EnableDebug)
                LogService.LogTerminalInfo("<<< " + Data.ToHexString());
            _cmdBuilder.Append(Data.ToHexStringNoSpace());

            if (_cmdBuilder.Length >= 2)
            {
                var lengthHex = _cmdBuilder.ToString().Substring(0, 2);

                string cmd = string.Empty;

                var length = Convert.ToInt16(lengthHex, 16);
                if (EnableDebug)
                {
                    if (length == 0)
                    {
                        throw new Exception($"Length = 0 | Command {_cmdBuilder.ToString()}");
                    }
                    else
                    {
                        LogService.LogTerminalInfo($"Length = {length} | _cmdBuilder {_cmdBuilder.Length / 2}");
                        LogService.LogTerminalInfo(_cmdBuilder.ToString().ToHexBytes().ToHexString());
                    }
                }

                if ((_cmdBuilder.Length / 2) >= (length + 2))
                {
                    cmd = _cmdBuilder.ToString().Substring(0, length * 2 + 4);
                    _cmdBuilder = _cmdBuilder.Remove(0, cmd.Length);

                    if (EnableDebug)
                    {
                        LogService.LogTerminalInfo("LEFT: " + _cmdBuilder);
                    }

                    ParseData(cmd.ToHexBytes());
                }
            }
        }

        public void SendCommand(string cmd)
        {
            WritePortData(cmd.Trim().Replace(" ", string.Empty).ToHexBytes());
        }

        private void WritePortData(byte[] bytes, bool logInfo = true)
        {
            if (logInfo)
            {
                LogService.LogTerminalInfo($"----> {bytes.ToHexString()}");
            }

            //if (bytes.Count() > 8)
            //{
            //    var cmd = (Commands)bytes[8];
            //    if (cmd != Commands.KeepAlive)
            //    {
            //        LogService.LogTerminalDebug($"----> {bytes.ToHexString()}");
            //    }
            //}

            if (Port.IsOpen)
            {
                Port?.Write(bytes, 0, bytes.Length);
            }
            else
            {
                LogService.LogTerminalDebug($"Port is close, reconnect and try again");
                Connect(Port.PortName);
                Port?.Write(bytes, 0, bytes.Length);
            }
        }

        public DateTime GetLastPolling()
        {
            return LastPolling;
        }

        public DateTime LastPolling = DateTime.Now;
        public bool GotConfig = false;
        private void ParseData(byte[] bytes)
        {
            var cmd = (Commands)bytes[8];
            var length = BitConverter.ToInt16(new byte[] { bytes[0], bytes[1] }, 0);
            var cmdData = bytes.ToList().GetRange(9, length - 9);

            //LogService.LogTerminalInfo($"FULL:  {bytes.ToAsiiString()}");
            //LogService.LogTerminalInfo($"DATA:  {cmdData.ToArray().ToAsiiString()}");

            //if (LogResponse || cmd != Commands.Response)
            //{
            LogService.LogTerminalInfo($"<---- {cmd} | Data: {cmdData.ToArray().ToHexString()}");
            LogService.LogTerminalInfo($"<---- {bytes.ToHexString()}");
            //}

            if (cmd == Commands.Response)
            {
                LastPolling = DateTime.Now;
            }

            // MDB Workflow
            if (cmd == Commands.MDBCommand)
            {
                var mdbResponse = (Mdb.Response)cmdData[0];
                var identity = bytes[3];
                Ack(identity);
                LastMdbState?.Invoke(mdbResponse.ToString());
                CurrentMdbState = mdbResponse;
                switch (mdbResponse)
                {
                    // Begin Session => Send Vend Request
                    case Mdb.Response.BeginSession:
                        LogService.LogTerminalInfo("<---- Begin Session");
                        LogService.LogTerminalDebug("<---- Begin Session");
                        LogService.LogInfo("Begin Session");
                        if (AutoCompleteSession)
                        {
                            OnValidateCardSuccess?.Invoke(PaymentType.NAYAX);
                        }
                        break;
                    case Mdb.Response.VendApproved:
                        LogService.LogTerminalInfo("<---- Vend Approved");
                        LogService.LogTerminalDebug("<---- Vend Approved");
                        LogService.LogInfo("Vend Approved");
                        if (AutoCompleteSession)
                        {

                            VendSuccess();
                            SessionComplete();
                        }
                        break;
                    case Mdb.Response.Cancelled:
                        LogService.LogTerminalInfo("<---- Vend Cancelled");
                        LogService.LogTerminalDebug("<---- Vend Cancelled");
                        LogService.LogInfo("Vend Cancelled");
                        if (AutoCompleteSession)
                        {
                            SessionComplete();
                        }
                        break;
                    case Mdb.Response.VendDenied:
                        LogService.LogTerminalInfo("<---- Vend Denied");
                        LogService.LogTerminalDebug("<---- Vend Denied");
                        LogService.LogInfo("Vend Denied");
                        if (AutoCompleteSession)
                        {
                            SessionComplete();
                        }
                        break;
                    case Mdb.Response.EndSession:
                        LogService.LogTerminalInfo("<---- End Session");
                        LogService.LogTerminalDebug("<---- End Session");
                        LogService.LogInfo("End Session");
                        if (AutoCompleteSession)
                        {
                            EnableReader();
                        }
                        break;
                }
            }

            if (cmd == Commands.Reset)
            {
                HandleReset();
            }

            if (cmd == Commands.Config)
            {
                GotConfig = true;
                CMD_STRUCT_DES = bytes[4];
                CMD_STRUCT_DES_LSB = bytes[5];
                CMD_STRUCT_SOURCE = bytes[6];
                CMD_STRUCT_SOURCE_LSB = bytes[7];
                LogService.LogInfo("Terminal got config");

                if (!HasDoneStartup)
                {
                    HasDoneStartup = true;
                    TerminalStartupCmd();
                }
            }
        }

        public void TerminalStartupCmd()
        {
            LogService.LogInfo("Start keeping alive!");
            KeepDeviceAlive();
            //Thread.Sleep(300);
            this.VendCancel();
            //Thread.Sleep(300);
            this.SessionComplete();
        }

        private void HandleReset()
        {
            LogService.LogInfo("Terminal got reset/Sending FW info!");
            GotConfig = false;
            this.SetFwInfo();
            Thread.Sleep(1000);
            this.EnableReader();

            //if (!HasDoneStartup)
            //{
            //    StopKeepDeviceAlive();
            //    Thread.Sleep(1000);
            //}

            //this.SetFwInfo();
            //WaitForConfig();
            //LogService.LogInfo("After wait for config: " + GotConfig);
            //if (GotConfig)
            //{
            //    LogService.LogInfo("Terminal got config/Start keeping alive!");
            //    KeepDeviceAlive();
            //    EnableReader();
            //    WaitingForConfig = false;
            //}

            //if (!HasDoneStartup)
            //{
            //    HasDoneStartup = true;
            //    this.VendCancel();
            //    this.SessionComplete();
            //}
        }

        private bool WaitingForConfig = false;
        private void WaitForConfig()
        {
            Task.Run(() =>
            {
                WaitingForConfig = true;
                LogService.LogTerminalInfo("Waiting for config");
                var time = 0;
                while (true)
                {
                    //LogService.LogInfo("GotConfig: " + GotConfig);
                    Thread.Sleep(100);
                    if (++time >= 50)
                    {
                        LogService.LogTerminalInfo("Failed to get config after sent FwInfo");
                        break;
                    }
                    if (GotConfig)
                    {
                        LogService.LogTerminalInfo($"Got config after {time * 100}ms");
                        LogService.LogInfo("Terminal got config/Start keeping alive!");
                        KeepDeviceAlive();
                        EnableReader();
                        WaitingForConfig = false;

                        if (!HasDoneStartup)
                        {
                            HasDoneStartup = true;
                            this.VendCancel();
                            this.SessionComplete();
                        }
                        break;
                    }
                }
                
            });
        }
        #endregion

        #region Commands
        public void SetFwInfo()
        {
            var data = new List<byte>();

            // Version 00.11
            var version = new byte[] { 0x00, 0x0B };

            byte peripheralType = 0x04;

            // Not in use
            byte peripheralSubType = 0x01;


            var peripheralCapabilitiesBitmap = new byte[] { 0x00, 0x00 };

            var peripheralModel = "Model_21202";
            var peripheralSerialNumber = "Serial_19233";
            var peripheralApplicationSwVersion = "Simulator_Ver_01_01";

            peripheralModel = peripheralModel.PadRight(20, '\0');
            peripheralSerialNumber = peripheralSerialNumber.PadRight(20, '\0');
            peripheralApplicationSwVersion = peripheralApplicationSwVersion.PadRight(20, '\0');

            data.AddRange(version);
            data.Add(peripheralType);
            data.Add(peripheralSubType);
            data.AddRange(peripheralCapabilitiesBitmap);
            data.AddRange(peripheralModel.AsiiToBytes());
            data.AddRange(peripheralSerialNumber.AsiiToBytes());
            data.AddRange(peripheralApplicationSwVersion.AsiiToBytes());


            var cmd = BuildCommand(Commands.FirmwareInfo, pckOpt: 0x00, desLsb: 0x00, data: data.ToArray());

            LogService.LogTerminalInfo($"----> Firmware Info");

            WritePortData(cmd);
        }

        public void EnableReader()
        {
            LogService.LogTerminalInfo($"----> Enable Reader");
            LogService.LogTerminalDebug($"----> Enable Reader");

            var cmd = BuildCommand(Commands.MDBCommand, pckOpt: CMD_STRUCT_PACKET_OPTION, desLsb: CMD_STRUCT_DES_LSB, data: Mdb.Reader.Enable());
            WritePortData(cmd);
        }

        public void DisableReader()
        {
            LogService.LogTerminalDebug($"----> Disable Reader");
            var cmd = BuildCommand(Commands.MDBCommand, pckOpt: CMD_STRUCT_PACKET_OPTION, desLsb: CMD_STRUCT_DES_LSB, data: Mdb.Reader.Disable());
            WritePortData(cmd);
        }

        public void KeepAlive()
        {
            LogService.LogTerminalInfo($"----> Keep Alive");
            var cmd = BuildCommand(Commands.KeepAlive, pckOpt: CMD_STRUCT_PACKET_OPTION, desLsb: CMD_STRUCT_DES_LSB);
            WritePortData(cmd);
        }

        public int Amount { get; set; }
        public int ItemNumber { get; set; }

        public void VendRequest()
        {
            ItemNumber = 1;
            LogService.LogTerminalInfo($"----> Vend Request: {Amount}");
            LogService.LogTerminalDebug($"----> Vend Request: {Amount}");

            var cmd = BuildCommand(Commands.MDBCommand, pckOpt: CMD_STRUCT_PACKET_OPTION, desLsb: CMD_STRUCT_DES_LSB, data: Mdb.Vend.Request(Amount, ItemNumber));
            WritePortData(cmd);
        }
        public void VendSuccess()
        {
            LogService.LogTerminalInfo("----> Vend Success");
            LogService.LogTerminalDebug("----> Vend Success");

            var cmd = BuildCommand(Commands.MDBCommand, pckOpt: CMD_STRUCT_PACKET_OPTION, desLsb: CMD_STRUCT_DES_LSB, data: Mdb.Vend.Success(ItemNumber));
            WritePortData(cmd);
        }

        public void SessionComplete()
        {
            LogService.LogTerminalInfo("----> Session Complete");
            LogService.LogTerminalDebug("----> Session Complete");

            var cmd = BuildCommand(Commands.MDBCommand, pckOpt: CMD_STRUCT_PACKET_OPTION, desLsb: CMD_STRUCT_DES_LSB, data: Mdb.Vend.SessionComplete());
            WritePortData(cmd);
        }
        public void CloseSession(byte status, int amount)
        {
            LogService.LogTerminalInfo("----> Close Session");
            LogService.LogTerminalDebug("----> Close Session");

            var cmd = BuildCommand(Commands.MDBCommand, pckOpt: CMD_STRUCT_PACKET_OPTION, desLsb: CMD_STRUCT_DES_LSB, data: Mdb.Vend.CloseSession(status, amount, ItemNumber));
            WritePortData(cmd);
        }
        public void VendCancel()
        {
            LogService.LogTerminalInfo("----> Vend Cancel");
            LogService.LogTerminalDebug("----> Vend Cancel");

            var cmd = BuildCommand(Commands.MDBCommand, pckOpt: CMD_STRUCT_PACKET_OPTION, desLsb: CMD_STRUCT_DES_LSB, data: Mdb.Vend.Cancel());
            WritePortData(cmd);
        }

        public void Revalue(int amount)
        {
            LogService.LogTerminalInfo("----> Revalue");
            var cmd = BuildCommand(Commands.MDBCommand, pckOpt: CMD_STRUCT_PACKET_OPTION, desLsb: CMD_STRUCT_DES_LSB, data: Mdb.Revalue.Request(amount));
            WritePortData(cmd);
        }

        public void CashSale(int amount)
        {
            LogService.LogTerminalInfo("----> Cash Sale");
            var cmd = BuildCommand(Commands.MDBCommand, pckOpt: CMD_STRUCT_PACKET_OPTION, desLsb: CMD_STRUCT_DES_LSB, data: Mdb.Vend.CashSale(amount));
            WritePortData(cmd);
        }

        public void Ack(byte identity)
        {
            LogService.LogTerminalInfo("----> ACK");
            var cmd = BuildCommand(Commands.Response, pckOpt: 0x00, desLsb: CMD_STRUCT_DES_LSB, data: new byte[] { 0x00 }, identity: identity);
            WritePortData(cmd);
        }
        public void Purchase(int amount)
        {
            Amount = amount;
            this.EnableReader();
        }
        #endregion

        #region Command Builder

        private byte[] BuildCommand(Commands cmd, byte pckOpt, byte desLsb, byte[] data = null, byte identity = 0x00)
        {
            var command = new List<byte>
            {
                pckOpt,
                identity == 0x00 ? this.Identity() : identity,
                CMD_STRUCT_SOURCE,
                CMD_STRUCT_SOURCE_LSB,
                CMD_STRUCT_DES,
                desLsb,
                (byte)cmd
            };
            if (data != null)
            {
                command.AddRange(data);
            }
            var length = command.Count + 2;
            command.InsertRange(0, length.IntTo2Bytes());
            var checksum = command.ToArray().CheckSumCrc16Ccitt();
            command.AddRange(checksum);

            return command.ToArray();
        }

        private int identity = 0;

        private byte Identity()
        {
            if (++identity == 256)
            {
                return 0;
            }
            else
            {
                return (byte)identity;
            }
        }

        #endregion

    }

    #region EXT

    public class Mdb
    {
        public static class Vend
        {
            public const byte Command = 0x13;
            public static byte[] Request(int amount, int item)
            {
                var data = new List<byte>
                {
                    Command, 0x00
                };
                data.AddRange(amount.IntTo2Bytes());
                //data.AddRange(item.IntTo2Bytes());
                return data.ToArray();
            }

            public static byte[] Cancel()
            {
                var data = new List<byte>
                {
                    Command, 0x01
                };
                return data.ToArray();
            }
            public static byte[] Success(int item)
            {
                var data = new List<byte>
                {
                    Command, 0x02
                };
                data.AddRange(item.IntTo2Bytes());

                return data.ToArray();
            }
            public static byte[] Failure()
            {
                var data = new List<byte>
                {
                    Command, 0x03
                };
                return data.ToArray();
            }
            public static byte[] SessionComplete()
            {
                var data = new List<byte>
                {
                    Command, 0x04
                };

                return data.ToArray();
            }

            public static byte[] CashSale(int price)
            {
                var data = new List<byte>
                {
                    Command, 0x05
                };
                data.AddRange(price.IntTo2Bytes());

                return data.ToArray();
            }

            public static byte[] CloseSession(byte status, int price, int item)
            {
                var data = new List<byte>
                {
                    Command, 0x80
                };
                data.Add(status);
                data.AddRange(price.IntTo2Bytes());
                data.AddRange(item.IntTo2Bytes());
                data.AddRange(1.IntTo2Bytes());

                return data.ToArray();
            }

        }

        public static class Reader
        {
            public const byte Command = 0x14;
            public static byte[] Disable()
            {
                var data = new List<byte>
                               {
                                   Command, 0x00
                               };
                return data.ToArray();
            }

            public static byte[] Enable()
            {
                var data = new List<byte>
                               {
                                   Command, 0x01
                               };
                return data.ToArray();
            }
            public static byte[] Cancel()
            {
                var data = new List<byte>
                               {
                                   Command, 0x02
                               };
                return data.ToArray();
            }
            public static byte[] DataEntryResponse()
            {
                var data = new List<byte>
                               {
                                   Command, 0x03
                               };
                return data.ToArray();
            }



        }

        public static class Revalue
        {
            public const byte Command = 0x15;
            public static byte[] Request(int amount)
            {
                var data = new List<byte>
                {
                    Command, 0x00
                };
                data.AddRange(amount.IntTo2Bytes());
                return data.ToArray();
            }
        }

        public enum Response
        {
            BeginSession = 0x03,
            SessionCancelRequest = 0x04,
            VendApproved = 0x05,
            VendDenied = 0x06,
            EndSession = 0x07,
            Cancelled = 0x08,
            RevalueApproved = 0x0D,
            RevalueDenied = 0x0E
        }

    }
    public enum Commands
    {
        Response = 0x00,
        Reset = 0x01,
        FirmwareInfo = 0x05,
        Config = 0x06,
        KeepAlive = 0x07,
        DisplayMessage = 0x08,
        DisplayMessageStatus = 0x09,
        TransferData = 0x0A,
        Status = 0x0B,
        GetTime = 0x0C,
        Time = 0x0D,
        SetParameter = 0x0E,
        SetPaymentParameters = 0x0F,
        ModemStatus = 0x20,
        OpenSocket = 0x21,
        CloseSocket = 0x22,
        SendData = 0x23,
        ReceiveData = 0x24,
        ModemRxControl = 0x25,
        Trace = 0x30,
        Alert = 0x31,
        GetFileVersion = 0x32,
        FileVersion = 0x33,
        SendFile = 0x34,
        MDBCommand = 0x80,
        RDRSetEvent = 0x90,
        RDREvent = 0x91,
        RDRTransactionRequest = 0x92,
        RDRDispenseConfirm = 0x93,
        RDRTransactionCancel = 0x94,
        RDREndTransaction = 0x95
    }



    public static class Ext
    {
        public static byte Mod256Checksum(this byte[] data)
        {
            byte check = 0x00;
            for (int i = 0; i < data.Length; i++)
            {
                check += data[i];
            }
            return check;
        }
        public static byte IntToByte(this int number)
        {
            byte[] intBytes = BitConverter.GetBytes(number);
            byte result = intBytes[0];  // second least-significant byte
            return result;
        }
        public static int ByteToInt(this byte data)
        {
            return Convert.ToInt32(data);
        }
        public static byte CheckSum(this byte[] data)
        {
            byte crc = 0;
            for (int i = 0; i < data.Length; ++i)
            {
                crc = (byte)(crc ^ data[i]);
            }
            return crc;
        }

        public static string TryGetValue(this Dictionary<string, string> dict, string key)
        {
            if (dict.TryGetValue(key, out string value))
            {
                return value.HexStringToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public static byte[] StringToByteArray(this String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2) bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static byte[] CmdToByteArray(this String cmd)
        {
            int NumberChars = cmd.Length;
            byte[] bytes = new byte[NumberChars];
            for (int i = 0; i < NumberChars; i += 1) bytes[i] = Convert.ToByte(cmd.Substring(i, 1), 16);
            return bytes;
        }

        public static string HexStringToString(this String hex)
        {
            return Encoding.ASCII.GetString(hex.StringToByteArray());
        }


        public static string ToHexString(this byte[] hex)
        {
            if (hex == null) return null;
            if (hex.Length == 0) return string.Empty;

            var s = new StringBuilder();
            foreach (byte b in hex)
            {
                s.Append(b.ToString("x2").ToUpper());
                s.Append(" ");
            }
            return s.ToString();
        }


        public static string ToHexStringNoSpace(this byte[] hex)
        {
            if (hex == null) return null;
            if (hex.Length == 0) return string.Empty;

            var s = new StringBuilder();
            foreach (byte b in hex)
            {
                s.Append(b.ToString("x2").ToUpper());
            }
            return s.ToString();
        }

        public static string ToAsiiString(this byte[] hex)
        {
            return Encoding.UTF8.GetString(hex, 0, hex.Length);
        }

        public static byte[] AsiiToBytes(this String data)
        {
            return ASCIIEncoding.ASCII.GetBytes(data);
        }

        public static byte[] ToHexBytes(this string hex)
        {
            if (hex == null) return null;
            if (hex.Length == 0) return new byte[0];

            int l = hex.Length / 2;
            var b = new byte[l];
            for (int i = 0; i < l; ++i)
            {
                b[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return b;
        }

        public static byte[] CheckSumCrc16(this byte[] data)
        {
            short num = 0;
            for (int index1 = 0; index1 < data.Length - 2; ++index1)
            {
                num ^= (short)(data[index1] << 8);
                for (int index2 = 0; index2 < 8; ++index2)
                {
                    if ((num & 32768) != 0)
                        num = (short)(num << 1 ^ 0x1021);
                    else
                        num <<= 1;
                }
            }
            var sss = BitConverter.GetBytes(num).ToHexString();
            return BitConverter.GetBytes(num).Reverse().ToArray();
        }

        public static byte[] CheckSumCrc16Ccitt(this byte[] data)
        {
            var sum = new Crc16Ccitt(InitialCrcValue.Zeros).ComputeChecksumBytes(data);
            return sum.ToArray();
        }

        public static byte[] IntTo2Bytes(this int number)
        {
            return BitConverter.GetBytes(number).Take(2).ToArray();
        }

        public static byte[] IntToBcd(this int number)
        {
            return number.ToString().PadLeft(4, '0').StringToByteArray();
        }

        public enum InitialCrcValue { Zeros, NonZero1 = 0xffff, NonZero2 = 0x1D0F }

        public class Crc16Ccitt
        {

            const ushort poly = 4129;
            ushort[] table = new ushort[256];
            ushort initialValue = 0;

            public ushort ComputeChecksum(byte[] bytes)
            {
                ushort crc = this.initialValue;
                for (int i = 0; i < bytes.Length; ++i)
                {
                    crc = (ushort)((crc << 8) ^ table[((crc >> 8) ^ (0xff & bytes[i]))]);
                }
                return crc;
            }

            public byte[] ComputeChecksumBytes(byte[] bytes)
            {
                ushort crc = ComputeChecksum(bytes);
                return BitConverter.GetBytes(crc);
            }

            public Crc16Ccitt(InitialCrcValue initialValue)
            {
                this.initialValue = (ushort)initialValue;
                ushort temp, a;
                for (int i = 0; i < table.Length; ++i)
                {
                    temp = 0;
                    a = (ushort)(i << 8);
                    for (int j = 0; j < 8; ++j)
                    {
                        if (((temp ^ a) & 0x8000) != 0)
                        {
                            temp = (ushort)((temp << 1) ^ poly);
                        }
                        else
                        {
                            temp <<= 1;
                        }
                        a <<= 1;
                    }
                    table[i] = temp;
                }
            }
        }
    }
    #endregion
}
