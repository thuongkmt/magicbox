using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Konbini.RfidFridge.Service.Core
{
    using Konbini.Messages.Enums;
    using Konbini.RfidFridge.Common;
    using Konbini.RfidFridge.Domain.Enums;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using PaymentType = Domain.Enums.PaymentType;
    using Timer = System.Timers.Timer;

    public class PayterInterface
    {
        private bool EnableDebug = true;
        public bool AutoCompleteSession { get; set; }
        public Action<string> LastMdbState { get; set; }
        public Action<PaymentType> OnValidateCardSuccess;
        public Mdb.Response CurrentMdbState { get; set; }

        private LogService LogService;
        private FridgeInterface FridgeInterface;
        private SlackService SlackService;

        public PayterInterface(LogService logService, FridgeInterface fridgeInterface, SlackService slackService)
        {
            LogService = logService;
            FridgeInterface = fridgeInterface;
            SlackService = slackService;

            AutoCompleteSession = true;
        }

        #region Command Struct
        private byte CMD_STRUCT_PREAMBLE = 0xAA;
        private byte CMD_STRUCT_MAGIC = 0x3C;
        #endregion


        #region Serial Port
        SerialPort Port;
        private List<byte> _cmdBuilder = new List<byte>();
        private Timer _aliveTimer = new Timer();

        public bool Connect(string port)
        {
            try
            {
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
                        LogService.LogTerminalInfo(ex.ToString());
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

        private void RaiseAppSerialDataEvent(byte[] Data)
        {
            try
            {
                if (EnableDebug)
                    LogService.LogTerminalInfo("<<< " + Data.ToHexString());
                _cmdBuilder.AddRange(Data);

                if (_cmdBuilder.Count >= 2)
                {

                    var totalCommandInBuffer = _cmdBuilder.Count(x => x == CMD_STRUCT_PREAMBLE);

                    for (int i = 0; i < totalCommandInBuffer; i++)
                    {
                        if (_cmdBuilder[0] == CMD_STRUCT_PREAMBLE)
                        {
                            var cmdLength = _cmdBuilder[1].ByteToInt();
                            if (EnableDebug)
                            {
                                LogService.LogTerminalInfo("Length: " + cmdLength);
                            }

                            var totalCommandLength = cmdLength + 3;
                            if (_cmdBuilder.Count >= totalCommandLength)
                            {
                                var cmd = _cmdBuilder.Take(totalCommandLength).ToList();
                                _cmdBuilder.RemoveRange(0, totalCommandLength);

                                if (EnableDebug)
                                {
                                    LogService.LogTerminalInfo("CMD: " + cmd.ToArray().ToHexString());
                                    LogService.LogTerminalInfo("LEFT: " + _cmdBuilder.ToArray().ToHexString());
                                }

                                ParseData(cmd.ToArray(), cmdLength);
                            }
                        }
                        else
                        {
                            LogService.LogTerminalInfo("ERROR! Unexpected Response: " + _cmdBuilder.ToArray().ToHexString());
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                LogService.LogTerminalInfo(ex.ToString());
            }
        }

        public void SendCommand(string cmd)
        {
            WritePortData(cmd.Trim().Replace(" ", string.Empty).ToHexBytes());
        }

        private void WritePortData(byte[] bytes, bool logInfo = true)
        {
            try
            {
                if (logInfo)
                {
                    var c = $"----> {bytes.ToHexString()}";
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

                if (Port != null)
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
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
        }

        public DateTime GetLastPolling()
        {
            return LastPolling;
        }

        public DateTime LastPolling = DateTime.Now;
        public bool GotConfig = false;
        private void ParseData(byte[] bytes, int dataLength)
        {
            var cmd = (CommandHeader)bytes[3];
            var cmdData = bytes.ToList().GetRange(4, dataLength - 2);
            var cmdDataS = ParseCommandData(cmd, cmdData.ToArray());

            LogService.LogTerminalInfo($"<---- {cmd} | Data: {cmdDataS}");
            LogService.LogTerminalInfo($"<---- {bytes.ToHexString()}");
            LogService.LogTerminalInfo($"================================================");

            switch (cmd)
            {
                case CommandHeader.Status:
                    LastPolling = DateTime.Now;
                    if (FridgeInterface.CurrentMachineStatus == MachineStatus.IDLE)
                    {
                        // Transaciton is running when machine is IDLE => Problem
                        if (cmdData[1] == 0x00)
                        {
                            var message = "Payter terminal got problem in Transaction Flow, send Reset command";
                            SlackService.SendAlert(RfidFridgeSetting.Machine.Name, message);
                            LogService.LogError(message);
                            int.TryParse(RfidFridgeSetting.System.Payment.Payter.PreAuthAmount, out int payterreserveAmount);
                            // Do init again to reset terminal
                            InitTerminal(payterreserveAmount);
                        }
                    }
                    break;

                // Begin Session => Send Vend Request
                case CommandHeader.SessionStart:
                    // LogService.LogTerminalInfo("<---- Begin Session");
                    //  LogService.LogTerminalDebug("<---- Begin Session");
                    //  LogService.LogInfo("Begin Session");
                    if (AutoCompleteSession)
                    {
                        //OnValidateCardSuccess?.Invoke(PaymentType.PAYTER);
                    }
                    break;
                case CommandHeader.VendApproved:
                    // LogService.LogTerminalInfo("<---- Vend Approved");
                    // LogService.LogTerminalDebug("<---- Vend Approved");
                    // LogService.LogInfo("Vend Approved");
                    //GetStatus();
                    if (AutoCompleteSession)
                    {

                        //VendSuccess();
                        //SessionComplete();
                        OnValidateCardSuccess?.Invoke(PaymentType.PAYTER);
                    }
                    break;
                case CommandHeader.VendFailed:
                    // LogService.LogTerminalInfo("<---- Vend Failed");
                    // LogService.LogTerminalDebug("<---- Vend Failed");
                    //  LogService.LogInfo("Vend Failed");
                    if (AutoCompleteSession)
                    {
                        SessionComplete();
                    }
                    break;
                case CommandHeader.VendDenied:
                    // LogService.LogTerminalInfo("<---- Vend Denied");
                    // LogService.LogTerminalDebug("<---- Vend Denied");
                    // LogService.LogInfo("Vend Denied");
                    if (AutoCompleteSession)
                    {
                        SessionComplete();
                    }
                    break;
                case CommandHeader.SessionClose:
                    // LogService.LogTerminalInfo("<---- Sesion Close");
                    //  LogService.LogTerminalDebug("<---- Sesion Close");
                    //  LogService.LogInfo("Sesion Close");
                    if (AutoCompleteSession)
                    {
                        EnableReader();
                        VendRequestV3(ReserveAmount);
                    }
                    break;
                case CommandHeader.SessionComplete:
                    // LogService.LogTerminalInfo("<---- Sesion Close");
                    //  LogService.LogTerminalDebug("<---- Sesion Close");
                    //  LogService.LogInfo("Sesion Close");
                    if (AutoCompleteSession)
                    {
                        //EnableReader();
                        //Thread.Sleep(1000);
                        //VendRequestV3(ReserveAmount);

                    }
                    break;

            }


        }

        private string ParseCommandData(CommandHeader cmd, byte[] data)
        {
            string result = data.ToHexString();
            switch (cmd)
            {
                case CommandHeader.Result:
                    result = ((CommandData_Result)data[0]).ToString();
                    break;
                case CommandHeader.TransactionEvent:
                    result = ((CommandData_TransactionEvent)data[0]).ToString();
                    break;
                case CommandHeader.Status:
                    var isTxnRunning = (data[1] == 0x01) ? "YES" : "NO";
                    result = $"Status: {(CommandData_Status)data[0]} | Is Transaction Running {isTxnRunning}";
                    break;
            }
            return result;
        }
        #endregion

        #region Commands

        public void Reset()
        {
            LogService.LogTerminalInfo($"----> Reset Reader");
            LogService.LogTerminalDebug($"----> Reset Reader");

            var cmd = BuildCommand(CommandHeader.Reset);
            WritePortData(cmd);
        }

        public void Sync()
        {
            LogService.LogTerminalInfo($"----> Reader Sync Data");
            LogService.LogTerminalDebug($"----> Reader Sync Data");

            var cmd = BuildCommand(CommandHeader.Synchronize);
            WritePortData(cmd);
        }

        public void Setup(int reserveAmount)
        {
            LogService.LogTerminalInfo($"----> Setup Reader | Reserve amount: " + reserveAmount);
            LogService.LogTerminalDebug($"----> Setup Reader | Reserve amount: " + reserveAmount);

            var amount = reserveAmount.IntTo2Bytes().Reverse().ToArray();
            var cmd = BuildCommand(CommandHeader.Setup, amount);
            WritePortData(cmd);
        }


        public void SessionStart(int reserveAmount)
        {
            LogService.LogTerminalInfo($"----> Session Start | Reserve amount: " + reserveAmount);
            LogService.LogTerminalDebug($"----> Session Start | Reserve amount: " + reserveAmount);

            var amount = reserveAmount.IntTo2Bytes().Reverse().ToArray();
            var cmd = BuildCommand(CommandHeader.SessionStart, amount);
            WritePortData(cmd);
        }

        public void SessionClose()
        {
            LogService.LogTerminalInfo($"----> Session Close");
            LogService.LogTerminalDebug($"----> Session Close");

            var cmd = BuildCommand(CommandHeader.SessionClose);
            WritePortData(cmd);
        }

        public void EnableReader()
        {
            LogService.LogTerminalInfo($"----> Enable Reader");
            LogService.LogTerminalDebug($"----> Enable Reader");

            var cmd = BuildCommand(CommandHeader.Enable);
            WritePortData(cmd);
        }

        public void DisableReader()
        {
            LogService.LogTerminalDebug($"----> Disable Reader");
            // var cmd = BuildCommand(Commands.MDBCommand, pckOpt: CMD_STRUCT_PACKET_OPTION, desLsb: CMD_STRUCT_DES_LSB, data: Mdb.Reader.Disable());
            // WritePortData(cmd);
        }

        public void Protocol(CommandData_Protocol_Version version, CommandData_Protocol_Option opt)
        {
            LogService.LogTerminalInfo($"----> Setup Protocol | " + version);
            LogService.LogTerminalDebug($"----> Setup Protocol | " + version);

            var data = new byte[] { (byte)version, 0x00, 0x00, 0x00, (byte)opt };
            var cmd = BuildCommand(CommandHeader.Protocol, data);
            WritePortData(cmd);
        }



        public int Amount { get; set; }
        public int ItemNumber { get; set; }

        public void VendRequest()
        {
            ItemNumber = 1;
            LogService.LogTerminalInfo($"----> Vend Request: {Amount}");
            LogService.LogTerminalDebug($"----> Vend Request: {Amount}");

            var amountInBytes = Amount.IntTo2Bytes().Reverse().ToArray();
            var cmd = BuildCommand(CommandHeader.VendRequest, amountInBytes);
            WritePortData(cmd);
        }

        public void VendRequestV3(int amount = 0)
        {
            ItemNumber = 1;


            if (amount > 0)
                Amount = amount;

            LogService.LogTerminalInfo($"----> Vend Request: {Amount}");
            LogService.LogTerminalDebug($"----> Vend Request: {Amount}");
            var amountInBytes = Amount.IntTo2Bytes();
            var data = new List<byte>();
            data.AddRange(amountInBytes.Reverse());
            data.AddRange(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x00, 0x00 });

            var cmd = BuildCommand(CommandHeader.VendRequest, data.ToArray());
            WritePortData(cmd);
        }

        public void VendRequest(int amount)
        {
            Amount = amount;
            VendRequest();
        }
        public void VendSuccess(int amount = 0)
        {
            LogService.LogTerminalInfo("----> Vend Success | Amount: " + amount);
            LogService.LogTerminalDebug("----> Vend Success | Amount: " + amount);

            var data = new List<byte>();

            if (amount > 0)
            {
                var amountInBytes = amount.IntTo2Bytes();
                data = new List<byte>();
                data.AddRange(amountInBytes.Reverse());
            }
            var cmd = BuildCommand(CommandHeader.VendSuccess, data.ToArray());
            WritePortData(cmd);
        }

        public void SessionComplete()
        {
            LogService.LogTerminalInfo("----> Session Complete");
            LogService.LogTerminalDebug("----> Session Complete");

            var cmd = BuildCommand(CommandHeader.SessionComplete);
            WritePortData(cmd);
        }

        public void VendCancel()
        {
            LogService.LogTerminalInfo("----> Vend Cancel");
            LogService.LogTerminalDebug("----> Vend Cancel");

            var cmd = BuildCommand(CommandHeader.VendFailed);
            WritePortData(cmd);
        }
        public int ReserveAmount;

        public void GetStatus()
        {
            LogService.LogTerminalInfo("----> Get Status");
            LogService.LogTerminalDebug("----> Get Status");

            var cmd = BuildCommand(CommandHeader.Status);
            WritePortData(cmd);
        }
        public void InitTerminal(int reserveAmount)
        {
            ReserveAmount = reserveAmount;
            Reset();
            Thread.Sleep(200);

            Sync();
            Thread.Sleep(200);

            Protocol(CommandData_Protocol_Version.V3, CommandData_Protocol_Option.EnableEvChargeMode);
            Thread.Sleep(200);

            //reserveAmount
            Setup(reserveAmount);
            Thread.Sleep(200);

            SessionComplete();
            Thread.Sleep(200);

            EnableReader();
            Thread.Sleep(200);

            VendRequestV3(reserveAmount);

            Thread.Sleep(200);
            //GetStatus();

            _aliveTimer.Stop();
            _aliveTimer.Enabled = false;
            _aliveTimer.Interval = 60 * 1000 * 10;
            _aliveTimer.Elapsed -= _aliveTimer_Elapsed;
            _aliveTimer.Elapsed += _aliveTimer_Elapsed;

            _aliveTimer.Enabled = true;
            _aliveTimer.Start();
        }

        private void _aliveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            GetStatus();
        }

        public void Purchase(int amount)
        {
            Amount = amount;
            this.EnableReader();
        }
        #endregion

        #region Command Builder

        private byte[] BuildCommand(CommandHeader cmd, byte[] data = null)
        {
            var command = new List<byte>
            {
                (byte)cmd
            };
            if (data != null)
            {
                command.AddRange(data);
            }

            command.Insert(0, CMD_STRUCT_MAGIC);
            var length = command.Count;

            command.Insert(0, length.IntToByte());
            command.Insert(0, 0xCC);

            var checksum = command.ToArray().Mod256Checksum();
            command.Add(checksum);

            return command.ToArray();
        }


        #endregion

    }

    #region EXT


    public enum CommandHeader
    {
        UnknownCommand = -1,
        Enable = 0x12,
        Reset = 0x55,
        Result = 0x00,
        SessionStart = 0x21,
        SessionComplete = 0x22,
        SessionClose = 0x25,
        Setup = 0x23,
        Synchronize = 0x24,
        TransactionEvent = 0x38,
        VendApproved = 0x31,
        VendDenied = 0x32,
        VendFailed = 0x33,
        VendRequest = 0x34,
        VendSuccess = 0x35,
        Protocol = 0x13,
        Status = 0x26
    }



    // Transaction Event 0x38
    public enum CommandData_TransactionEvent
    {
        Started = 0x00,
        ReadingCard = 0x01,
        CardReadComplete = 0x02,
        TransactionComplete = 0x03,
        UnknowState = 0xFF,
    }

    public enum CommandData_Result
    {
        OK = 0x00,
        NOK = 0x01
    }

    public enum CommandData_Protocol_Version
    {
        V1 = 0x01,
        V2 = 0x02,
        V3 = 0x03

    }

    public enum CommandData_Protocol_Option
    {
        EnableTransactionEvent = 0x01,
        EnableEvChargeMode = 0x02
    }

    public enum CommandData_Status
    {
        OFFLINE = 0x01,
        ONLINE = 0x02,
        DISABLED = 0x04,
        ENABLED = 0x08,
        SESSION = 0x10,
        VEND_A = 0x020,
        VEND_B = 0x40
    }


    //public static class Ext1
    //{
    //    public static int ByteToInt(this byte data)
    //    {
    //        return Convert.ToInt32(data);
    //    }
    //    public static byte CheckSum(this byte[] data)
    //    {
    //        byte crc = 0;
    //        for (int i = 0; i < data.Length; ++i)
    //        {
    //            crc = (byte)(crc ^ data[i]);
    //        }
    //        return crc;
    //    }

    //    public static string TryGetValue(this Dictionary<string, string> dict, string key)
    //    {
    //        if (dict.TryGetValue(key, out string value))
    //        {
    //            return value.HexStringToString();
    //        }
    //        else
    //        {
    //            return string.Empty;
    //        }
    //    }

    //    public static byte[] StringToByteArray(this String hex)
    //    {
    //        int NumberChars = hex.Length;
    //        byte[] bytes = new byte[NumberChars / 2];
    //        for (int i = 0; i < NumberChars; i += 2) bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
    //        return bytes;
    //    }

    //    public static byte[] CmdToByteArray(this String cmd)
    //    {
    //        int NumberChars = cmd.Length;
    //        byte[] bytes = new byte[NumberChars];
    //        for (int i = 0; i < NumberChars; i += 1) bytes[i] = Convert.ToByte(cmd.Substring(i, 1), 16);
    //        return bytes;
    //    }

    //    public static string HexStringToString(this String hex)
    //    {
    //        return Encoding.ASCII.GetString(hex.StringToByteArray());
    //    }


    //    public static string ToHexString(this byte[] hex)
    //    {
    //        if (hex == null) return null;
    //        if (hex.Length == 0) return string.Empty;

    //        var s = new StringBuilder();
    //        foreach (byte b in hex)
    //        {
    //            s.Append(b.ToString("x2").ToUpper());
    //            s.Append(" ");
    //        }
    //        return s.ToString();
    //    }


    //    public static string ToHexStringNoSpace(this byte[] hex)
    //    {
    //        if (hex == null) return null;
    //        if (hex.Length == 0) return string.Empty;

    //        var s = new StringBuilder();
    //        foreach (byte b in hex)
    //        {
    //            s.Append(b.ToString("x2").ToUpper());
    //        }
    //        return s.ToString();
    //    }

    //    public static string ToAsiiString(this byte[] hex)
    //    {
    //        return Encoding.UTF8.GetString(hex, 0, hex.Length);
    //    }

    //    public static byte[] AsiiToBytes(this String data)
    //    {
    //        return ASCIIEncoding.ASCII.GetBytes(data);
    //    }

    //    public static byte[] ToHexBytes(this string hex)
    //    {
    //        if (hex == null) return null;
    //        if (hex.Length == 0) return new byte[0];

    //        int l = hex.Length / 2;
    //        var b = new byte[l];
    //        for (int i = 0; i < l; ++i)
    //        {
    //            b[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
    //        }
    //        return b;
    //    }

    //    public static byte[] CheckSumCrc16(this byte[] data)
    //    {
    //        short num = 0;
    //        for (int index1 = 0; index1 < data.Length - 2; ++index1)
    //        {
    //            num ^= (short)(data[index1] << 8);
    //            for (int index2 = 0; index2 < 8; ++index2)
    //            {
    //                if ((num & 32768) != 0)
    //                    num = (short)(num << 1 ^ 0x1021);
    //                else
    //                    num <<= 1;
    //            }
    //        }
    //        var sss = BitConverter.GetBytes(num).ToHexString();
    //        return BitConverter.GetBytes(num).Reverse().ToArray();
    //    }

    //    public static byte[] CheckSumCrc16Ccitt(this byte[] data)
    //    {
    //        var sum = new Crc16Ccitt(InitialCrcValue.Zeros).ComputeChecksumBytes(data);
    //        return sum.ToArray();
    //    }

    //    public static byte Mod256Checksum(this byte[] data)
    //    {
    //        byte check = 0x00;
    //        for (int i = 0; i < data.Length; i++)
    //        {
    //            check += data[i];
    //        }
    //        return check;
    //    }

    //    public static byte[] IntTo2Bytes(this int number)
    //    {
    //        return BitConverter.GetBytes(number).Take(2).ToArray();
    //    }

    //    public static byte[] IntToBcd(this int number)
    //    {
    //        return number.ToString().PadLeft(4, '0').StringToByteArray();
    //    }

    //    public static byte IntToByte(this int number)
    //    {
    //        byte[] intBytes = BitConverter.GetBytes(number);
    //        byte result = intBytes[1];  // second least-significant byte
    //        return result;
    //    }

    //    public enum InitialCrcValue { Zeros, NonZero1 = 0xffff, NonZero2 = 0x1D0F }

    //    public class Crc16Ccitt
    //    {

    //        const ushort poly = 4129;
    //        ushort[] table = new ushort[256];
    //        ushort initialValue = 0;

    //        public ushort ComputeChecksum(byte[] bytes)
    //        {
    //            ushort crc = this.initialValue;
    //            for (int i = 0; i < bytes.Length; ++i)
    //            {
    //                crc = (ushort)((crc << 8) ^ table[((crc >> 8) ^ (0xff & bytes[i]))]);
    //            }
    //            return crc;
    //        }

    //        public byte[] ComputeChecksumBytes(byte[] bytes)
    //        {
    //            ushort crc = ComputeChecksum(bytes);
    //            return BitConverter.GetBytes(crc);
    //        }

    //        public Crc16Ccitt(InitialCrcValue initialValue)
    //        {
    //            this.initialValue = (ushort)initialValue;
    //            ushort temp, a;
    //            for (int i = 0; i < table.Length; ++i)
    //            {
    //                temp = 0;
    //                a = (ushort)(i << 8);
    //                for (int j = 0; j < 8; ++j)
    //                {
    //                    if (((temp ^ a) & 0x8000) != 0)
    //                    {
    //                        temp = (ushort)((temp << 1) ^ poly);
    //                    }
    //                    else
    //                    {
    //                        temp <<= 1;
    //                    }
    //                    a <<= 1;
    //                }
    //                table[i] = temp;
    //            }
    //        }
    //    }
    //}
    #endregion
}
