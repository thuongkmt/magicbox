using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Konbini.RfidFridge.Service.Core;
using MagicCashlessPayment.Core.Util;
using static MagicCashlessPayment.Core.Devices.R2Interface.Response;

namespace MagicCashlessPayment.Core.Devices

{
    public class R2Interface
    {
        private const byte STX = 0x02;
        private const byte ETX = 0x03;
        private byte SEQ = 0x00;

        private List<byte> _cmdBuffer = new List<byte>();
        public SerialPort Port;
        public bool EnableDebug { get; set; }
        public Action<string> Log { get; set; }
        public Action<string> LogInfo { get; set; }

        public Action<Response> OnSaleApprove { get; set; }
        public Action<Response> OnValidateApprove { get; set; }

        public Action<Response> OnSaleError { get; set; }
        public Action<string> OnSaleCancel { get; set; }
        #region Serial port
        private SlackService SlackService;

        public R2Interface(SlackService slackService)
        {
            SlackService = slackService;
        }
        public bool Connect(string port)
        {
            try
            {
                SEQ = 0x00;

                Port = new SerialPort(port)
                {
                    BaudRate = 115200,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
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
                        Log?.Invoke(ex.ToString());
                    }
                    if (Port.IsOpen) StartReadData();
                }, null));
                if (Port.IsOpen)
                {
                    LogInfo?.Invoke("Terminal connected to: " + Port.PortName);
                    StartReadData();
                }
                else
                {
                    LogInfo?.Invoke("Terminal fail to connects to: " + Port.PortName);
                }

                return Port.IsOpen;
            }
            catch (Exception ex)
            {

                Port.Dispose();
                Log?.Invoke(ex.ToString());
                LogInfo?.Invoke("Failed to connect to terminal: " + ex.Message);
            }

            return false;
        }

        public bool IsConnected => Port.IsOpen;

        public void Test(string command)
        {
            var bytes = command.Replace(" ", string.Empty).StringToByteArray();
            RaiseAppSerialDataEvent(bytes);
        }

        private void RaiseAppSerialDataEvent(byte[] Data)
        {
            if (EnableDebug)
                Console.WriteLine("<<< " + Data.ToHexString());

            _cmdBuffer.AddRange(Data);

            try
            {
                if (_cmdBuffer.Count > 1)
                {
                    if (_cmdBuffer[0] == STX)
                    {
                        if (_cmdBuffer.Count > 3)
                        {
                            var length = 256 * _cmdBuffer[1] + _cmdBuffer[2];
                            var cmd = _cmdBuffer.GetRange(0, length + 5);

                            if (_cmdBuffer.Count == cmd.Count)
                            {
                                _cmdBuffer.Clear();

                            }
                            else
                            {
                                var another = _cmdBuffer.Skip(cmd.Count).Take(_cmdBuffer.Count - cmd.Count).ToList();
                                _cmdBuffer.Clear();
                                _cmdBuffer.AddRange(another);
                            }

                            CommandReceived(cmd.ToArray());
                        }
                    }
                    else
                    {
                        Console.WriteLine("Command is not start with STX");
                    }
                }
            }
            catch (Exception ex)
            {
                if (EnableDebug)
                {
                    LogInfo?.Invoke(ex.ToString());
                }
                    
            }




            //if (_cmdBuffer.Contains(ETX))
            //{
            //    try
            //    {
            //        // Find where is ETX
            //        var endIndex = _cmdBuffer.IndexOf(ETX);

            //        // Split data util LRC
            //        var cmd = _cmdBuffer.GetRange(0, endIndex + 1);

            //        //if(cmd.Count > 3)
            //        //{

            //        //}

            //        // Clean buffer after finish
            //        _cmdBuffer.Clear();

            //        CommandReceived(cmd.ToArray());
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.Message);
            //    }
            //}
        }

        #endregion


        #region Command and Reponse
        private void CommandReceived(byte[] bytes)
        {
            Log?.Invoke($"<---- {bytes.ToArray().ToHexString()}");

            if (bytes.Count() > 7)
            {
                ProcessResponse(bytes);

            }

            if (bytes.Count() == 7)
            {
                var data = bytes[4];

                // 0x14 Card not found
                // 0x21 Ezlink balance not enough

                if (data != 0x00)
                {
                    OnSaleCancel?.Invoke(new byte[] { data }.ToHexStringNoSpace().Trim());
                }
            }
        }

        public void ProcessResponse(byte[] cmd)
        {
            var response = new Response(cmd);
            var reponseCode = response.ResponseCode;

            Log?.Invoke($"Reponse Code: {Response.GetResponseCode(reponseCode)}");
            Log?.Invoke($"{response}");

            if (reponseCode == "00")
            {
                // Transaction success
                if (string.IsNullOrEmpty(response.Stan))
                {
                    OnValidateApprove?.Invoke(response);
                }
                else
                {
                    OnSaleApprove?.Invoke(response);
                }
            }
            else
            {
                OnSaleError?.Invoke(response);
            }
        }

        private byte[] BuildCommand(byte[] cmd)
        {
            var listCmd = new List<byte>(cmd);

            listCmd.Insert(0, CalulateSeq());

            var cmdLength = listCmd.Count;
            var bLength = cmdLength.IntTo2Bytes1().Reverse();

            listCmd.InsertRange(0, bLength);
            listCmd.Insert(0, STX);
            var checksum = listCmd.ToArray().XorCheckSum();
            listCmd.Add(checksum);
            listCmd.Add(ETX);

            return listCmd.ToArray();
        }

        private byte CalulateSeq()
        {
            if (++SEQ > 0xFF)
            {
                SEQ = 0x00;
            }
            return SEQ;
        }

        public void InitSale(int amount, Action<Response> onValidateApprove = null, Action<Response> onSaleApproved = null, Action<Response> onSaleError = null, Action<string> onSaleCancel = null)
        {
            LogInfo?.Invoke("Init Sale");
            Log?.Invoke("Init Sale");

            OnValidateApprove = onValidateApprove;

            OnSaleApprove = onSaleApproved;
            OnSaleError = onSaleError;
            OnSaleCancel = onSaleCancel;

            var cmd = BuildCommand(Command.InitSale(amount));
            SendCommand(cmd);
        }

        public void ProceedSale(Command.PROCEED_SALE_ACT_CODE code)
        {
            LogInfo?.Invoke("Proceed Sale");
            Log?.Invoke("Proceed Sale");

            var cmd = BuildCommand(Command.ProceedSale(code));
            SendCommand(cmd);
        }

        public bool Validate(int amount, ref CARD_LABEL cardLabel, ref string cancelMessage)
        {
            Response reponse = null;
            LogInfo?.Invoke("Validating card");
            var done = false;
            var isSuccess = false;
            var isCancel = false;
            var cancelMsg = string.Empty;
            // Init sale
            InitSale(amount,
            (ValidateApprove) =>
            {
                reponse = ValidateApprove;
                isSuccess = true;
                done = true;
            },
            (SaleApprove) =>
            {
            },
            (SaleError) =>
            {
                reponse = SaleError;
                isSuccess = false;
                done = true;
            },
            (SaleCancel) =>
            {
                cancelMsg = GetResponseCode(SaleCancel);

                isSuccess = false;
                isCancel = true;
                done = true;
            });

            var timeForWait = 0;
            // wait for done
            while (!done)
            {
                Thread.Sleep(1000);
                LogInfo?.Invoke("Validating...");
                if (++timeForWait >= 60)
                {
                    LogInfo?.Invoke("Timeout!");
                    cancelMessage = "Timed out. Can't read card info";
                    return false;
                }
            }
            LogInfo?.Invoke("Validating card done | Result: " + isSuccess);

            if (isCancel)
            {
                LogInfo?.Invoke("Validating card done | Cancelled");
                cancelMessage = cancelMsg;
                return false;
            }
            else
            {
                if (reponse != null)
                {
                    cardLabel = reponse.CardLabel;

                    // Skip this validate transaction
                    LogInfo?.Invoke("Skip the validate transaction");
                    ProceedSale(Command.PROCEED_SALE_ACT_CODE.SKIP);
                    return isSuccess;

                }
                else
                {
                    // Skip this validate transaction
                    LogInfo?.Invoke("Skip the validate transaction");
                    ProceedSale(Command.PROCEED_SALE_ACT_CODE.SKIP);
                    return false;
                }
            }

        }

        public bool Charge(int amount, ref Response outResponse)
        {
            Response reponse = new Response();
            LogInfo?.Invoke("Charing | Amount: " + amount);
            var done = false;
            var initSaleDone = false;

            var isSuccess = false;
            var isCancel = false;
            // Init sale
            InitSale(amount,
            (ValidateApprove) =>
            {
                reponse = ValidateApprove;
                initSaleDone = true;
            },
            (SaleApprove) =>
            {
                reponse = SaleApprove;
                isSuccess = true;
                done = true;
            },
            (SaleError) =>
            {
                reponse = SaleError;
                isSuccess = false;
                done = true;
            },
            (SaleCancel) =>
            {
                reponse = new Response();
                reponse.ResponseCode = SaleCancel;
                isSuccess = false;
                isCancel = true;
                done = true;
            });

            var timeout = 0;
            // wait for done
            while (!initSaleDone)
            {
                Thread.Sleep(1000);
                LogInfo?.Invoke("Charing...");
                if (++timeout > 30)
                {
                    LogInfo?.Invoke("Charing... | timeout");
                    reponse = new Response();
                    reponse.ResponseCode = "FE";
                    return false;
                }
            }

            LogInfo?.Invoke("Init Sale Done | Result: " + isSuccess);

            if (reponse != null)
            {
                if (reponse.ResponseCode == "00")
                {
                    LogInfo?.Invoke("Approve transaction..");
                    ProceedSale(Command.PROCEED_SALE_ACT_CODE.OK);
                }
            }
            else
            {
                LogInfo?.Invoke("Init Sale Reponse is null!!");
            }

            timeout = 0;
            // Process sale
            while (!done)
            {
                Thread.Sleep(1000);
                LogInfo?.Invoke("Waiting for approve txn...");
                if (++timeout > 30)
                {
                    LogInfo?.Invoke("Waiting for approve txn | timeout");
                    return false;
                }
            }

            LogInfo?.Invoke("Init Sale Done | Result: " + isSuccess);

            if (reponse != null)
            {
                if (reponse.ResponseCode == "00")
                {
                    LogInfo?.Invoke("Transaciton approved..");
                    isSuccess = true;
                }
            }
            else
            {
                LogInfo?.Invoke("Process Sale Response is null!!");
            }
            outResponse = reponse;
            return isSuccess;
        }

        private void SendCommand(byte[] cmd)
        {
            Log?.Invoke($"---> {cmd.ToHexString()}");
            Port?.Write(cmd, 0, cmd.Length);
        }
        #endregion

        public static class Command
        {
            public static byte[] InitSale(int amount)
            {
                var cmd = new List<byte>
                {
                    0x21
                };

                var sAmount = amount.ToString().PadLeft(12, '0');
                var bAmount = sAmount.StringToByteArray();
                cmd.AddRange(bAmount);

                return cmd.ToArray();
            }

            public static byte[] ProceedSale(PROCEED_SALE_ACT_CODE code)
            {
                var cmd = new List<byte>
                {
                    0x22
                };

                cmd.Add((byte)code);

                return cmd.ToArray();
            }

            public enum PROCEED_SALE_ACT_CODE
            {
                OK = 0x00,
                SKIP = 0x01,
                REJECT = 0xFF
            }
        }

        public class Response
        {
            public Response()
            {

            }
            public Response(byte[] response)
            {
                this.ResponseCode = new byte[] { response[4] }.ToHexStringNoSpace();

                // Terminal ID
                var tid = response.Skip(5).Take(8).ToArray();
                TID = tid.ToAsiiString();

                // Datetime
                var datetime = response.Skip(13).Take(6).ToArray();
                var sDateTime = "20" + datetime.ToHexStringNoSpace();
                DateTime = DateTime.ParseExact(sDateTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);

                var maskpan = response.Skip(19).Take(10).ToArray();
                MaskPan = maskpan.ToHexString();
                var cardLabel = new byte[] { maskpan[0] }.ToHexStringNoSpace();
                if (cardLabel == "10")
                {
                    CardLabel = CARD_LABEL.EZLINK;
                }
                else if (cardLabel == "80")
                {
                    CardLabel = CARD_LABEL.CONCESSION;
                }
                else if (cardLabel == "11")
                {
                    CardLabel = CARD_LABEL.NFP;
                }
                else if (cardLabel.StartsWith("4"))
                {
                    CardLabel = CARD_LABEL.VISA;
                }
                else if (cardLabel.StartsWith("5"))
                {
                    CardLabel = CARD_LABEL.MASTER;
                }
                CardNumer = maskpan.ToHexStringNoSpace();


                var hashpan = response.Skip(29).Take(40).ToArray();
                HashPan = hashpan.ToAsiiString();


                var rrn = response.Skip(69).Take(12).ToArray();
                Rrn = rrn.ToAsiiString();

                var stan = response.Skip(81).Take(3).ToArray();
                Stan = stan.ToHexStringNoSpace();

                var approveCode = response.Skip(84).Take(6).ToArray();
                ApproveCode = approveCode.ToAsiiString();

                var addDataLength = response.Length - 92;
                if (addDataLength > 0)
                {
                    var addData = response.Skip(90).Take(addDataLength).ToArray();
                    AddData = addData.ToHexString();
                }
            }

            public string ResponseCode { get; set; }
            public string TID { get; set; }
            public DateTime DateTime { get; set; }
            public string MaskPan { get; set; }
            public string HashPan { get; set; }
            public string Rrn { get; set; }
            public string Stan { get; set; }
            public string ApproveCode { get; set; }
            public string AddData { get; set; }
            public CARD_LABEL CardLabel { get; set; }
            public string CardNumer { get; set; }

            public override string ToString()
            {
                var stringBuilder = new StringBuilder();
                var properties = this.GetType().GetProperties().ToList();

                foreach (var pro in properties)
                {
                    stringBuilder.AppendLine($"[{pro.Name}] = {pro.GetValue(this, null)}");
                }
                return stringBuilder.ToString();
            }

            public static string GetResponseCode(string responseCode)
            {
                ResponseCodes.TryGetValue(responseCode, out string reponseCodeDetail);
                if (string.IsNullOrEmpty(reponseCodeDetail))
                {
                    reponseCodeDetail = "UNKNOW RESPONSE CODE";
                }
                return $"[{responseCode}] {reponseCodeDetail}";
            }

            private static readonly Dictionary<string, string> ResponseCodes = new Dictionary<string, string>
            {
                ["00"] = "Successful - Command executed successfully/Transaction Completed",
                ["10"] = "Failed – Card already registered. Entry not cleared, (no exit)",
                ["11"] = "Failed – Card is not registered. Exit not allowed",
                ["12"] = "Failed – Transaction Not Allowed. Underflow (Insufficient Fund)",
                ["13"] = "Failed – Transaction Not Allowed. Overflow (Exceeded Limit)",
                ["14"] = "Failed – Card Not Found",
                ["15"] = "Failed – Card Not Accepted/Invalid Card",
                ["16"] = "Failed – Card Blacklisted",
                ["21"] = "Failed – Transaction Not Completed. Declined by Bank ",
                ["22"] = "Failed – Transaction Not Completed. Connection Timeout (Bank)",
                ["23"] = "Failed – Transaction Not Completed. Connection Timeout (Internal) ",
                ["31"] = "Failed – Condition Not Satisfied ",
                ["32"] = "Failed – Invalid Input Data/Invalid Input Length ",
                ["EC"] = "Failed –Recovery SEQ number not matched ",
                ["ED"] = "Failed – Recovery SEQ number not matched ",
                ["D2"] = "Transaction Declined",
                ["FE"] = "Charge timeout",
                ["FB"] = "EZLink/NFP Not Enough Balance",
                ["FF"] = "Not Applicable",
            };

            public enum CARD_LABEL
            {
                NONE,
                VISA,
                MASTER,
                EZLINK,
                CONCESSION,
                NFP
            }



        }

        public void Test()
        {

        }
    }



    //public static class DataParserExtenstions
    //{
    //    public static string ParseResponseData(this string response, int start, int length, ref int nextStart)
    //    {
    //        nextStart = start + length;
    //        return response.Substring(start, length);
    //    }

    //    public static int ByteToInt(this byte data)
    //    {
    //        return Convert.ToInt32(data);
    //    }
    //    public static byte XorCheckSum(this byte[] data)
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

    //    public static byte[] IntTo2Bytes(this int number)
    //    {
    //        return BitConverter.GetBytes(number).Take(2).ToArray();
    //    }

    //    public static byte[] IntToBcd(this int number)
    //    {
    //        return number.ToString().PadLeft(4, '0').StringToByteArray();
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
}
