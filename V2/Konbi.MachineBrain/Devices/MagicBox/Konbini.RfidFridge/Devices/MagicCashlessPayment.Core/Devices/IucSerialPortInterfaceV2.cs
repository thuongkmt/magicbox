
using Konbini.RfidFridge.Common;
using Konbini.RfidFridge.Domain;
using Konbini.RfidFridge.Service.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static MagicCashlessPayment.Core.Devices.IucSerialPortInterfaceV2.Commands;

namespace MagicCashlessPayment.Core.Devices
{

    public class IucSerialPortInterfaceV2 : IDisposable
    {
        #region Serial Port
        public Action<string> Log { get; set; }
        public Action<string> LogInfo { get; set; }
        public Action<string> LogError { get; set; }

        private System.Timers.Timer _timerCheckPort = new System.Timers.Timer();

        public SerialPort Port;
        private static int INumber { get; set; }
        private StringBuilder _cmdBuilder = new StringBuilder();

        //public Action<string> Log { get; set; }
        public Action<string> Debug { get; set; }

        public Action<IucApprovedResponse> OnSaleApproved { get; set; }
        public Action<SaleResponse> OnSaleError { get; set; }
        public Action<SaleResponse> OnSaleCancelled { get; set; }
        public Action<string> OnTerminalCallback { get; set; }

        public Action<ResponseResult> OnCommandError { get; set; }


        private ConcurrentQueue<string> _responseQueue = new ConcurrentQueue<string>();
        private ConcurrentQueue<SendingCommand> _commandQueue = new ConcurrentQueue<SendingCommand>();
        private DateTime? lastSaleDateTime = null;
        public string ComportName { get; set; }
        public bool DebugMode
        {
            get
            {
                return true;
            }
        }

        private bool isTerminalRunning;
        public bool IsTerminalRunning
        {
            get
            {
                return isTerminalRunning;
            }
            set
            {
                if (isTerminalRunning != value)
                {
                    isTerminalRunning = value;
                }
            }
        }

        public bool USE_CMDQUEUE = true;

        private SlackService SlackService;
        public IucSerialPortInterfaceV2(SlackService slackService)
        {
            SlackService = slackService;
        }
        public void Disconnect()
        {
            Port?.Close();
        }
        public bool ConnectPort(string port, System.Action<string> action = null)
        {
            var result = false;

            try
            {
                INumber = -1;

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
                        Log?.Invoke(ex.ToString());
                    }
                    if (Port.IsOpen) StartReadData();
                }, null));

                if (Port.IsOpen)
                {
                    StartReadData();

                    if (USE_CMDQUEUE)
                    {
                        SendCommandJob();
                    }

                    //PerformCheckBlacklist();
                }

                ComportName = port;

                var msg = string.Empty;
                if (Port.IsOpen)//Port.IsOpen
                {

                    var checkResult = CheckDevice();
                    if (checkResult)
                    {
                        msg = $"Terminal found at [{ComportName}]";
                        LogInfo?.Invoke(msg);
                        result = true;
                    }
                    else
                    {
                        msg = $"Failed to connect to Terminal at [{ComportName}]";
                        LogError?.Invoke(msg);
                    }
                }
                else
                {
                    msg = $"Failed to connect to IUC at [{ComportName}]";
                    LogError?.Invoke(msg);
                    result = false;
                }

                Log?.Invoke(msg);
                action?.Invoke(msg);

            }
            catch (Exception ex)
            {
                Log?.Invoke(ex.ToString());
                Port.Dispose();
            }

            return result;
        }

        public void CheckPortClose()
        {
            _timerCheckPort = new System.Timers.Timer();
            _timerCheckPort.Enabled = true;
            _timerCheckPort.Interval = 60 * 1000 * 10;
            _timerCheckPort.Elapsed += _timerCheckPort_Elapsed;
            _timerCheckPort.Start();
        }

        private void _timerCheckPort_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!Port.IsOpen)
            {
                SlackService?.SendAlert(RfidFridgeSetting.Machine.Name, "IUC Comport is closed!!!");
            }
        }

        int EnqIndex = 0;

        private void RaiseAppSerialDataEvent(byte[] bytes)
        {
            try
            {
                var dataOnThisSession = new List<byte>();
                if (DebugMode)
                {
                    Log?.Invoke("<<< " + bytes.ToArray().ToHexString());
                }

                foreach (var Data in bytes)
                {

                    //if (Data == 0x06)
                    //{
                    //    Log?.Invoke("<---- ACK");
                    //    QueueResponse("06");

                    //    return;
                    //}
                    //if (Data == 0x15)
                    //{
                    //    QueueResponse("15");
                    //    Log?.Invoke("<---- NACK");
                    //    return;
                    //}
                    //if (Data == 0x05)
                    //{
                    //    QueueResponse("05");
                    //    Log?.Invoke("<---- EQN");
                    //    return;
                    //}
                    //if (Data == 0x04)
                    //{
                    //    QueueResponse("04");
                    //    Log?.Invoke("<---- EOT");
                    //    return;
                    //}

                    dataOnThisSession.Add(Data);
                }


                _cmdBuilder.Append(dataOnThisSession.ToArray().ToHexString());

                if (DebugMode)
                {
                    Log?.Invoke("_cmdBuilder: " + _cmdBuilder.ToString());
                }

                var trimmedCmd = _cmdBuilder.ToString().Trim();
                if (trimmedCmd.StartsWith("02"))
                {
                    var endIndex = _cmdBuilder.ToString().Trim().IndexOf("03");
                    if (endIndex >= 0)
                    {
                        if (DebugMode)
                        {
                            Log?.Invoke("END INDEX:" + endIndex);
                        }
                        var cmd = _cmdBuilder.ToString().Substring(0, endIndex + 5);

                        if (DebugMode)
                        {
                            Log?.Invoke("CMD:" + cmd + "|");
                        }
                        _cmdBuilder = _cmdBuilder.Remove(0, cmd.Length);
                        //_cmdBuilder = _cmdBuilder.ToString().Trim();
                        _cmdBuilder = new StringBuilder(_cmdBuilder.ToString().Trim());
                        if (DebugMode)
                        {
                            Log?.Invoke("LEFT:" + _cmdBuilder + "|");
                        }

                        QueueResponse(cmd);
                    }
                }
                else
                {
                    if (trimmedCmd.Equals("06"))
                    {
                        _cmdBuilder.Clear();

                        QueueResponse("06");

                        Log?.Invoke("<---- ACK");

                        return;
                    }
                    if (trimmedCmd.Equals("15"))
                    {
                        _cmdBuilder.Clear();

                        QueueResponse("15");
                        Log?.Invoke("<---- NACK");
                        return;
                    }
                    if (trimmedCmd.Equals("05"))
                    {
                        _cmdBuilder.Clear();

                        QueueResponse("05");
                        Log?.Invoke("<---- EQN");
                        return;
                    }
                    if (trimmedCmd.Equals("04"))
                    {
                        _cmdBuilder.Clear();

                        QueueResponse("04");
                        Log?.Invoke("<---- EOT");
                        return;
                    }

                    Log?.Invoke("Edge Cases response:" + trimmedCmd);

                }
            }
            catch (Exception ex)
            {
                Log?.Invoke(ex.ToString());
            }
        }



        private void WritePortData(byte[] bytes)
        {

            //Log?.Invoke("===================================SENDING COMMAND===================================");
            Log?.Invoke($"----> {bytes.ToHexString()}");
            var rawData = bytes.ToList();

            if (bytes[0] == 0x02)
            {
                // Remove STX
                rawData.RemoveAt(0);
                // Remove LRC
                rawData.RemoveAt(rawData.Count - 1);
                // Remove ETX
                rawData.RemoveAt(rawData.Count - 1);
                var stringData = rawData.ToArray().ToAsiiString();
                Log?.Invoke("----> " + stringData);
            }


            if (Port.IsOpen)
            {
                Port?.Write(bytes, 0, bytes.Length);
            }
            else
            {
                SlackService?.SendAlert(RfidFridgeSetting.Machine.Name, "Port is closed, retry to send command");
                Log?.Invoke("Port is closed, retry to send command");
                ConnectPort(Port.PortName);
                Port?.Write(bytes, 0, bytes.Length);
            }
        }


        #endregion

        #region Parse Data
        private void ParseData(byte[] data)
        {

            //Log?.Invoke("===================================RECEIVING COMMAND===================================");
            Log?.Invoke("<---- " + data.ToHexString());
            var rawData = data.ToList();

            // Remove STX
            rawData.RemoveAt(0);
            // Remove LRC
            rawData.RemoveAt(rawData.Count - 1);
            // Remove ETX
            rawData.RemoveAt(rawData.Count - 1);

            var response = rawData.ToArray().ToAsiiString();
            Log?.Invoke("<---- " + response);
            this.Ack();

            ProcessResponseData(response);
        }


        private void ProcessResponseData(string response)
        {


            if (string.Compare(response, 0, "R923", 0, 4) == 0 || string.Compare(response, 0, "R924", 0, 4) == 0)
            {
                Task.Run(() =>
                {
                    // Callback
                    GetTagValue(response, 34, out string outValue);
                    var code = outValue.Substring(0, 2);
                    var responseWithoutCode = outValue.Replace(code, string.Empty);
                    OnTerminalCallback?.Invoke(responseWithoutCode);
                });
            }

            if (string.Compare(response, 0, "R20", 0, 3) == 0 ||
             string.Compare(response, 0, "R630", 0, 4) == 0 ||
             string.Compare(response, 0, "R300", 0, 4) == 0 ||
             string.Compare(response, 0, "R301", 0, 4) == 0 ||
             string.Compare(response, 0, "R600", 0, 4) == 0 ||
             string.Compare(response, 0, "R610", 0, 4) == 0)

            {
                if (response.StartsWith("R209"))
                {
                    return;
                }
                if (GetTagValue(response, 39, out string responseCode) == 0)
                {
                    GetTagValue(response, 4, out string amout);
                    int.TryParse(amout, out int transactionAmount);
                    var saleReponse = new SaleResponse
                    {
                        Amout = transactionAmount,
                        ResponseCode = responseCode,
                        Message = IucErrorCode.CResponseCode(responseCode)
                    };

                    var approvedResponse = new IucApprovedResponse();

                    if (string.CompareOrdinal(responseCode, "00") == 0)
                    {
                        Log?.Invoke("COMMAND OK");

                        #region Transaction Infomations

                        var transInfo = "";
                        transInfo += "-----------------------------------\r\n";
                        if (string.Compare(response, 0, "R200", 0, 4) == 0)
                            transInfo += "              SALE\r\n";
                        else if (string.Compare(response, 0, "R600", 0, 4) == 0)
                            transInfo += "              SALE\r\n";
                        else if (string.Compare(response, 0, "R630", 0, 4) == 0)
                            transInfo += "              SALE\r\n";
                        else if (string.Compare(response, 0, "R201", 0, 4) == 0)
                            transInfo += "            PREAUTH\r\n";
                        else if (string.Compare(response, 0, "R202", 0, 4) == 0)
                            transInfo += "            OFFLINE\r\n";
                        else if (string.Compare(response, 0, "R203", 0, 4) == 0)
                            transInfo += "             REFUND\r\n";
                        else if (string.Compare(response, 0, "R300", 0, 4) == 0)
                            transInfo += "             VOID\r\n";
                        else if (string.Compare(response, 0, "R301", 0, 4) == 0)
                            transInfo += "             CANCEL PREAUTH\r\n";
                        else
                            transInfo += "           TRANSACTION\r\n";
                        transInfo += "-----------------------------------\r\n";

                        GetTagValue(response, 41, out var stValue);
                        transInfo += "TID: ";
                        transInfo += stValue;
                        approvedResponse.Tid = stValue;

                        transInfo += "  ";
                        GetTagValue(response, 42, out stValue);
                        transInfo += "MID: ";
                        transInfo += stValue;
                        transInfo += "\r\n";
                        approvedResponse.Mid = stValue;

                        if (GetTagValue(response, 7, out stValue) == 0)
                        {
                            var dateTime = string.Empty;
                            transInfo += "DATE TIME: ";
                            dateTime += stValue.Substring(2, 2); // DD   
                            dateTime += "/";
                            dateTime += stValue.Substring(0, 2); // MM
                            dateTime += " ";
                            dateTime += stValue.Substring(4, 2);
                            dateTime += ":";
                            dateTime += stValue.Substring(6, 2);
                            dateTime += ":";
                            dateTime += stValue.Substring(8, 2);
                            transInfo += dateTime;
                            transInfo += "\r\n";

                            // approvedResponse.DateTime = dateTime;
                        }

                        if (GetTagValue(response, 62, out stValue) == 0)
                        {
                            transInfo += "INVOICE: ";
                            transInfo += stValue;
                            approvedResponse.Invoice = stValue;
                            transInfo += "  ";
                        }
                        if (GetTagValue(response, 64, out stValue) == 0)
                        {
                            transInfo += "BATCH: ";
                            approvedResponse.Batch = stValue;
                            transInfo += stValue;
                        }
                        transInfo += "\r\n";

                        if (GetTagValue(response, 61, out stValue) == 0)
                        {
                            transInfo += "UTRN: ";
                            transInfo += stValue;
                            transInfo += "\r\n";
                        }

                        if (GetTagValue(response, 54, out stValue) == 0)
                        {
                            // card label
                            transInfo += stValue;
                            approvedResponse.CardLabel = stValue;
                            transInfo += ": ";
                        }
                        if (GetTagValue(response, 2, out stValue) == 0)
                        {
                            // card number
                            transInfo += "CARD NUMBER: ";
                            transInfo += stValue;
                            approvedResponse.CardNumber = stValue;
                            transInfo += "\r\n";
                        }

                        if (GetTagValue(response, 37, out stValue) == 0)
                        {
                            transInfo += "RRN: ";
                            transInfo += stValue;
                            approvedResponse.Rrn = stValue;
                            transInfo += "\r\n";
                        }

                        if (GetTagValue(response, 38, out stValue) == 0)
                        {
                            transInfo += "APPROVAL CODE: ";
                            transInfo += stValue;
                            approvedResponse.ApproveCode = stValue;
                            transInfo += "\r\n";
                        }

                        if (GetTagValue(response, 22, out stValue) == 0)
                        {
                            transInfo += "ENTRY MODE: ";
                            if (stValue[0] == 'E')
                                transInfo += "MANUAL ENTRY";
                            else if (stValue[0] == 'M')
                                transInfo += "MAGSTRIPE";
                            else if (stValue[0] == 'F')
                                transInfo += "FALLBACK";
                            else if (stValue[0] == 'C')
                                transInfo += "CHIP";
                            else if (stValue[0] == 'P')
                                transInfo += "CONTACTLESS";
                            else
                                transInfo += stValue;
                            approvedResponse.EntryMode = stValue;
                            transInfo += "\r\n";
                        }

                        if (GetTagValue(response, 53, out stValue) == 0)
                        {

                            var appLabel = stValue.Substring(30, 16);
                            transInfo += "APP LABEL: ";
                            transInfo += appLabel;
                            transInfo += "\r\n";
                            approvedResponse.AppLabel = appLabel;

                            var aid = stValue.Substring(14, 16);
                            transInfo += "AID: ";
                            transInfo += aid;
                            transInfo += "\r\n";
                            approvedResponse.Aid = aid;

                            //var tvr = stValue.Substring(0, 10);;
                            transInfo += "TVR TSI: ";
                            transInfo += stValue.Substring(0, 10);
                            transInfo += " ";
                            transInfo += stValue.Substring(10, 4);
                            transInfo += "\r\n";

                            var tc = stValue.Substring(46, 16);
                            transInfo += "TC: ";
                            transInfo += stValue.Substring(46, 16);
                            transInfo += "\r\n";
                            approvedResponse.Tc = tc;

                        }

                        if (GetTagValue(response, 4, out stValue) == 0)
                        {
                            transInfo += "AMOUNT:   ";
                            var stAmount = int.Parse(stValue.Substring(0, 10)).ToString();
                            transInfo += "$";
                            transInfo += stAmount;
                            transInfo += ".";
                            transInfo += stValue.Substring(10, 2);
                            decimal.TryParse(stAmount + "." + stValue.Substring(10, 2), out decimal amt);
                            approvedResponse.Amount = amt;
                        }

                        if (GetTagValue(response, 14, out stValue) == 0)
                        {
                            transInfo += "EXP DATE: ";
                            transInfo += stValue;
                            approvedResponse.ExpDate = stValue;
                            transInfo += "\r\n";
                        }

                        transInfo += "\r\n";
                        //Debug?.Invoke(transInfo);
                        Log?.Invoke(transInfo);
                        SlackService.SendInfo(RfidFridgeSetting.Machine.Name, $"Terminal Response: " + transInfo);
                        if (!response.StartsWith("R209"))
                        {
                            OnSaleApproved?.Invoke(approvedResponse);
                        }
                        #endregion
                    }
                    else if (string.CompareOrdinal(responseCode, "CT") == 0 || string.CompareOrdinal(responseCode, "TA") == 0 || string.CompareOrdinal(responseCode, "TN") == 0)
                    {
                        Task.Run(() =>
                        {
                            if (!response.StartsWith("R209"))
                            {
                                OnSaleCancelled?.Invoke(saleReponse);
                                Log?.Invoke($"SALE/PREAUTH CANCELLED: {IucErrorCode.CResponseCode(responseCode)}");
                            }
                        });

                    }
                    else
                    {
                        Task.Run(() =>
                        {
                            if (!response.StartsWith("R209"))
                            {
                                OnSaleError?.Invoke(saleReponse);
                                Log?.Invoke($"SALE/PREAUTH ERROR: {IucErrorCode.CResponseCode(responseCode)}");
                            }
                        });
                    }



                }

            }
            else if (String.Compare(response, 0, "R900", 0, 3) == 0)
            {
                var stTerminalInfoValue = response.Substring(4);
                stTerminalInfoValue += "\r\n";
                Log?.Invoke($"Terminal Response: {stTerminalInfoValue}");
            }
        }

        #endregion

        #region Utils

        public void ClearResponseQueue()
        {
            this._responseQueue = new ConcurrentQueue<string>();
        }
        private void QueueResponse(string response)
        {
            // Never expect to get large queue
            if (this._responseQueue.Count > 100)
                this.ClearResponseQueue();
            this._responseQueue.Enqueue(response);
        }

        private string GetResponse()
        {
            return this._responseQueue.TryDequeue(out string result) ? result : "";
        }

        public void Test()
        {
            //var cmd = Commands.CancelPreauth("#wss3sWvthC6T6crJkMQ9E6RVglEePo0X1FWOCY9KGKw=", "#x2aCd1rBAnaeJOPSfv8t6Q==", "035608700017", "700017");
            //Console.WriteLine(cmd);
            //cmd = Commands.CancelPreauth("#5/kSTO4OeSIahNxrID8A/Er+AkrNV5cg54/zsqCkgr4=", "#HHD2zDe1DtybNoRUk4dA7A==", "035608700018", "700018");
            //Console.WriteLine(cmd);
            //cmd = Commands.CancelPreauth("#2Rk8sPfMLa3KRBfJ1smYGzySz9TPr1uhy3RZhNBl8iY=", "#n28dBLgGkPxgPJE8B6w7lw==", "035608700019", "700019");
            //Console.WriteLine(cmd);
        }

        private string GetExpectedResponse(string startWith, int timeout = 15000)
        {
            var tryTime = 0;
            while (true)
            {
                Thread.Sleep(1);
                if (++tryTime > timeout)
                {
                    return string.Empty;
                }
                var currentReponse = GetResponse();
                if (currentReponse.StartsWith(startWith))
                {
                    return currentReponse;
                }
            }
        }

        public static int GetTagValue(string responseString, int inTag, out string outValue)
        {
            var i = 0;
            if (responseString == null)
                goto FnEnd;
            for (i = 4; i < responseString.Length;)
            {
                var tag = responseString.Substring(i, 2);
                i += 2;
                var length = responseString.Substring(i, 2);
                i += 2;
                if (int.Parse(length) > 0)
                {
                    var value = responseString.Substring(i, int.Parse(length));

                    if (int.Parse(tag) == inTag)
                    {
                        outValue = value;
                        return 0;
                    }
                    i += int.Parse(length);
                }
            }

        FnEnd:
            outValue = null;
            return 1; // Error tag not found
        }


        #endregion

        #region Commands

        public void SendSaleCommand(string command, Action<IucApprovedResponse> onSaleApproved = null, Action<SaleResponse> onSaleError = null, Action<SaleResponse> onSaleCancelled = null, Action<string> onTerminalCallback = null, Action<ResponseResult> onCommandError = null)
        {
            lastSaleDateTime = DateTime.Now;
            OnSaleApproved = onSaleApproved;
            OnSaleError = onSaleError;
            OnSaleCancelled = onSaleCancelled;
            OnTerminalCallback = onTerminalCallback;
            OnCommandError = onCommandError;
            SendCommand(command, timeOut: 90000);
        }

        public void CleanCallBack()
        {
            Log?.Invoke("Clean call back");


            OnSaleApproved = null; ;
            OnSaleError = null;
            OnSaleCancelled = null;
            OnTerminalCallback = null;
            OnCommandError = null;
        }

        public int CheckCpasBalance(ref string cardNumber)
        {
            var balance = 0;
            Debug?.Invoke("Check CPAS balance");
            SlackService?.SendInfo(RfidFridgeSetting.Machine.Name, "Checking CPAS balance");
            var response = SendAndRecv("C6180103010", "R618", 10000);
            GetTagValue(response, 39, out string responseCode);
            if (responseCode == "00")
            {
                if (GetTagValue(response, 63, out string stringBalance) == 0)
                {
                    var s = stringBalance.Substring(0, 12);
                    var b = int.Parse(s.ToString());
                    balance = b;
                }
                else
                {
                    balance = -1;
                }

                if (GetTagValue(response, 02, out string cNumber) == 0)
                {
                    cardNumber = cNumber;
                }
            }
            else
            {
                balance = -1;
            }


            SlackService?.SendInfo(RfidFridgeSetting.Machine.Name, $"CPAS Balance:{balance} | Code: {response}");

            Debug?.Invoke("Balance: " + balance);
            return balance;
        }

        public bool CheckCreditCard()
        {
            var tryTime = 1;
        RETRY:
            Debug?.Invoke("Check Credit Card | try time: " + tryTime);
            SlackService?.SendInfo(RfidFridgeSetting.Machine.Name, "Checking Credit card");

            //// Check card data
            //OnTerminalCallback = (s) =>
            //{
            //    Debug?.Invoke("Check card data callback: " + s);
            //};
            //var cardData = SendAndRecv("C2090103010", "R209", 10000);
            //Debug?.Invoke($"Card Data: {cardData}");



            //if (string.IsNullOrEmpty(cardData))
            //{
            //    if (++tryTime >= 4)
            //    {
            //        Debug?.Invoke("Failed to check CC");

            //    }
            //    else
            //    {
            //        Debug?.Invoke("Retry to get card info");
            //        goto RETRY;
            //    }
            //}

            //if (string.IsNullOrEmpty(cardData))
            //{
            //    return false;
            //}

            //Thread.Sleep(1000);


            var invoiceNumber = "";
            var checkDone = false;
            var checkVoid = false;
            var timeout = 0;
            var result = false;

            SendSaleCommand(Commands.Sale(1, 3),
                   (CheckApprove) =>
                   {
                       Debug?.Invoke("Check Sale Approve");
                       #region Check done

                       var response = (IucApprovedResponse)CheckApprove;
                       invoiceNumber = response.Invoice;
                       checkDone = true;
                       result = true;

                       SlackService?.SendInfo(RfidFridgeSetting.Machine.Name, "Checking Credit Card | Approved");
                       #endregion
                   },
                   (CheckError) =>
                   {
                       var response = (SaleResponse)CheckError;
                       Debug?.Invoke($"Check Sale Error: [{response.ResponseCode}] {response.Message}");
                       result = false;
                       checkDone = true;
                       SlackService?.SendInfo(RfidFridgeSetting.Machine.Name, $"Checking Credit Error | [{response.ResponseCode}] {response.Message}");
                   },
                   (CheckCancel) =>
                   {
                       Debug?.Invoke("Check Sale Cancel");
                       result = false;
                       checkDone = true;
                   },
                   (Checkback) =>
                   {
                       Debug?.Invoke("Check Card Callback: " + Checkback);
                       timeout = 0;
                   });

            timeout = 0;
            do
            {
                // Default IUC timeout is 90, to be safe set our timeout to 95
                if (++timeout >= 35)
                {
                    Debug?.Invoke("Check Credit Card Time Out");
                    return false;
                }
                Thread.Sleep(1000);
                Debug?.Invoke("Timeout waiting: " + timeout);
            }
            while (!checkDone);

            Debug?.Invoke("Done Check Credit Card");

            if (result == false)
            {
                // Fail to check card, no need to void
                Debug?.Invoke("Fail to check credit card");
                if (++tryTime >= 1)
                {
                    return result;
                }
                else
                {
                    goto RETRY;
                }
            }

            Thread.Sleep(1000);
            Debug?.Invoke("Start void");
            SlackService?.SendInfo(RfidFridgeSetting.Machine.Name, "Start VOID");

            SendSaleCommand(Commands.Void(invoiceNumber),
                  (VoidApprove) =>
                  {
                      #region Void Done
                      Debug?.Invoke("VOID Approve");
                      SlackService?.SendInfo(RfidFridgeSetting.Machine.Name, "VOID Approved");

                      checkVoid = true;
                      result = true;
                      #endregion
                  },
                  (VoidError) =>
                  {
                      Debug?.Invoke("VOID Error");
                      SlackService?.SendInfo(RfidFridgeSetting.Machine.Name, "VOID Error");

                      checkVoid = true;
                  },
                  (VoidCancel) =>
                  {
                      Debug?.Invoke("VOID Cancel");
                      SlackService?.SendInfo(RfidFridgeSetting.Machine.Name, "VOID Cancel");

                      checkVoid = true;
                  },
                  (VoidCallback) =>
                  {
                      Debug?.Invoke("VOID Callback: " + VoidCallback);
                  });
            timeout = 0;
            do
            {
                if (++timeout >= 60)
                {
                    return false;
                }
                Thread.Sleep(1000);
            }
            while (!checkVoid);
            Debug?.Invoke("End void");
            Debug?.Invoke("Check card result = " + result);
            SlackService?.SendInfo(RfidFridgeSetting.Machine.Name, "Check card result = " + result);

            return true;
        }

        public bool CheckDevice()
        {
            var result = false;
            var response = PollingDevice();
            if (!string.IsNullOrEmpty(response) && response.StartsWith("R902"))
            {
                result = true;
            }
            return result;
        }

        public Dictionary<string, string> TerminalInfo()
        {
            var response = SendAndRecv("C900", "R900");
            Log?.Invoke("Terminal Info reponse: " + response);

            var infos = response.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var title = infos.FirstOrDefault(x => x.Contains("R900"))?.Replace("R900", String.Empty).Trim();
            var version = infos.FirstOrDefault(x => x.Contains("VERSION"))?.Replace("VERSION", String.Empty).Trim();
            var tid = infos.FirstOrDefault(x => x.Contains("MMS APP: DBS   MMS TID:"))?.Replace("MMS APP: DBS   MMS TID:", String.Empty).Trim();
            var autoSett = infos.FirstOrDefault(x => x.Contains("AUTO SETTLE (HHMM):"))?.Replace("AUTO SETTLE (HHMM):", String.Empty).Trim();
            var localIp = infos.FirstOrDefault(x => x.Contains("LOCAL ADDR:"))?.Replace("LOCAL ADDR:", String.Empty).Trim();

            var info = infos.FirstOrDefault(x => x.Contains("DBS-"))?.Replace("DBS", String.Empty).Trim();

            if (info != null)
            {
                var d = info.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                var mid = string.Empty;
                if (d.Length > 0)
                {
                    tid = d[2];
                    mid = d[3];
                }
                LogInfo("Terminal info: " + info);
                var dict = new Dictionary<string, string>
            {
                { "TITLE", title },
                { "VERSION", version },
                { "TID", tid },
                { "AUTOSETT", autoSett },
                { "IP", localIp },
                { "MID", mid },
            };
                return dict;
            }
            else
            {
                return null;
            }

        }

        public bool CheckBlacklist()
        {
            //var response = SendAndRecv("C616", "R616");
            //Log?.Invoke("Check Blacklist reponse: " + response);
            //if (GetTagValue(response, 39, out string responseCode) == 0)
            //{
            //    if (responseCode == "BL")
            //    {
            //        return true;
            //    }
            //}
            return true;
        }

        private System.Timers.Timer _timerCheckBlacklist = new System.Timers.Timer();

        private void PerformCheckBlacklist()
        {
            _timerCheckBlacklist.Enabled = false;
            _timerCheckBlacklist.Stop();
            _timerCheckBlacklist.Interval = 3.6e+6;

            _timerCheckBlacklist.Elapsed += PerformCheckBlacklist_Job;

            _timerCheckBlacklist.Enabled = true;
            _timerCheckBlacklist.Start();

        }

        private void PerformCheckBlacklist_Job(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now.Hour == 7)
            {
                // Start check blacklist
                CheckBlacklistJob();
            }
        }

        public void CheckBlacklistJob()
        {
            try
            {

                //Task.Run(() =>
                //{
                var IsBlacklist = CheckBlacklist();
                SlackService.SendInfo(RfidFridgeSetting.Machine.Name, "Checking Blacklist status for IUC");
                if (IsBlacklist)
                {
                    // Terminal got EZLink blacklist
                    Log?.Invoke("GOT BLACKLIST!!!!");

                    // Send notify to slack
                    SlackService.SendInfo(RfidFridgeSetting.Machine.Name, $"Blacklist was not downloaded!!!");
                }
                else
                {
                    SlackService.SendInfo(RfidFridgeSetting.Machine.Name, $"Blacklist has been downloaded");
                }
                //});

            }
            catch (Exception ex)
            {
                Log?.Invoke(ex.ToString());
            }

        }

        public string Tid;
        public string Mid;
        public void GetTidAndMid()
        {
            var tryTime = 0;
            RETRY:
            var response = TerminalInfo();

            if (response != null)
            {
                Tid = response["TID"];
                Mid = response["MID"];

                LogInfo($"Terminal ID: {Tid}, Merchant ID {Mid}");
            }
            else
            {
                if(tryTime++ >=5)
                {
                    Thread.Sleep(3000);
                    LogInfo("Failed to get TID/MID, retrying...");
                    goto RETRY;
                }
            }
        }

        public string PollingDevice()
        {
            Log?.Invoke($"PollingDevice {DateTime.Now} - Last sale {lastSaleDateTime?.ToString()}");
            string response = string.Empty;
            //default timeout is 90 seconds
            //if last payment time within 120 seconds, no need to polling
            if (lastSaleDateTime.HasValue && ((DateTime.Now - lastSaleDateTime.Value).TotalSeconds <= 600))
            {
                Log?.Invoke("Sale just started, no need polling");
                response = "R902";
            }
            else
            {
                response = SendAndRecv("C902", "R902", 3000);
            }
            // notify to Main about device state
            if (!string.IsNullOrEmpty(response) && response.StartsWith("R902"))
            {

                IsTerminalRunning = true;

            }
            else
            {
                IsTerminalRunning = false;
                //LogService.SendToSlackAlert("Iuc is disconnected");
            }

            Log?.Invoke($"PollingDevice result {response}");
            return response;

        }

        public void CancelCommand()
        {
            WritePortData(new byte[] { 0x18 });
        }
        public void Enq()
        {
            WritePortData(new byte[] { 0x05 });
        }
        public void Eot()
        {
            WritePortData(new byte[] { 0x04 });
        }
        public void Ack()
        {
            WritePortData(new byte[] { 0x06 });
        }
        public void NonAck()
        {
            WritePortData(new byte[] { 0x15 });
        }

        public void SendCommand(string command, Action<string> onCmdFinished = null, int timeOut = 35000)
        {
            if (USE_CMDQUEUE)
            {
                this._commandQueue.Enqueue(new SendingCommand(command, onCmdFinished, timeOut));
            }
            else
            {
                this._cmdBuilder = new StringBuilder();
                var cmdByte = command.AsiiToBytes().ToList();
                cmdByte.Add(0x03);
                var lrc = cmdByte.ToArray().CheckSum();
                cmdByte.Add(lrc);
                cmdByte.Insert(0, 0x02);

                this.Enq();
                WritePortData(cmdByte.ToArray());
                this.Eot();
            }
        }

        public void Inquiry()
        {
            var cmdByte = "C400".AsiiToBytes().ToList();
            cmdByte.Add(0x03);
            var lrc = cmdByte.ToArray().CheckSum();
            cmdByte.Add(lrc);
            cmdByte.Insert(0, 0x02);

            this.Enq();
            WritePortData(cmdByte.ToArray());
            this.Eot();
        }
        public void SendCommandJob()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        var hasData = this._commandQueue.TryDequeue(out SendingCommand command);
                        if (hasData)
                        {
                            Log?.Invoke("=====================PROCESSING COMMAND=====================");
                            Log?.Invoke(command.Command);
                            this._cmdBuilder = new StringBuilder();
                            var cmdByte = command.Command.AsiiToBytes().ToList();
                            cmdByte.Add(0x03);
                            var lrc = cmdByte.ToArray().CheckSum();
                            cmdByte.Add(lrc);
                            cmdByte.Insert(0, 0x02);

                            //this.Enq();
                            //WritePortData(cmdByte.ToArray());
                            //this.Eot();


                            var tryTime = 0;
                        CMD_EQN:
                            // Enquire and waiting for ACK
                            //Log?.Invoke("Sending EQN and waiting for ACK");

                            var eqn = Enquire();
                            if (!eqn)
                            {
                                if (++tryTime > 3)
                                {
                                    // Fail to eqn

                                    var msg = "IUC FAILED TO GOT ACK";
                                    Log?.Invoke(msg);
                                    LogError?.Invoke(msg);
                                    SlackService?.SendAlert(RfidFridgeSetting.Machine.Name, msg);

                                    OnCommandError?.Invoke(ResponseResult.FAIL_TO_GOT_ACK);
                                    Log?.Invoke($"=========================PROCESSING COMMAND DONE | Result:{ResponseResult.FAIL_TO_GOT_ACK} =========================");


                                    continue;
                                }
                                // Try to resend eqn
                                Thread.Sleep(200);

                                // Send end transmit
                                this.Eot();
                                // Session delay
                                Thread.Sleep(1000);

                                goto CMD_EQN;
                            }
                            else
                            {
                                //Log?.Invoke("Got ACK");
                            }
                            tryTime = 0;

                        CMD_SEND:
                            WritePortData(cmdByte.ToArray());
                            if (WaitingForAck())
                            {
                                Eot();
                                var responseResult = ProcessResponseJob(command);

                                if (responseResult == ResponseResult.TIMEOUT)
                                {
                                    OnCommandError?.Invoke(responseResult);
                                    //Log?.Invoke($"Timeout to get Response, try to Inquiry");
                                    // Thread.Sleep(1000);
                                    //Inquiry();
                                    //responseResult = ProcessResponseJob(command);
                                    //if (responseResult == ResponseResult.TIMEOUT)
                                    // {
                                    //    OnCommandError?.Invoke(responseResult);
                                    //}
                                }

                                Log?.Invoke($"=========================PROCESSING COMMAND DONE | Result:{responseResult} =========================");

                                if (responseResult == Commands.ResponseResult.NACK)
                                {
                                    OnCommandError?.Invoke(ResponseResult.NACK);

                                    Log?.Invoke("Resend command again!");
                                    Eot();
                                    goto CMD_EQN;
                                }
                                else if (responseResult == Commands.ResponseResult.UNEXPECTED_ERROR)
                                {
                                    Log?.Invoke("Terminal did not return correct response!");
                                    OnCommandError?.Invoke(ResponseResult.UNEXPECTED_ERROR);
                                    Eot();
                                }
                            }
                            else
                            {
                                if (++tryTime > 3)
                                {
                                    // Fail to send
                                    OnCommandError?.Invoke(ResponseResult.FAIL_TO_GOT_ACK);
                                    Log?.Invoke($"=========================PROCESSING COMMAND DONE | Result:{ResponseResult.FAIL_TO_GOT_ACK} =========================");

                                    var msg = "IUC FAILED TO GOT ACK";
                                    Log?.Invoke(msg);
                                    LogError?.Invoke(msg);
                                    SlackService?.SendAlert(RfidFridgeSetting.Machine.Name, msg);
                                    continue;
                                }
                                else
                                {
                                    Enq();
                                    var r = WaitingForAck();
                                    Log?.Invoke("IUC FAILED TO GOT ACK WHEN RETRY TO SEND COMMAND");
                                }
                                Thread.Sleep(1000);

                                // Try to resend
                                goto CMD_SEND;
                            }
                        }
                        Thread.Sleep(200);
                    }
                    catch (Exception ex)
                    {
                        Log?.Invoke("Send command job got problem: " + ex.ToString());
                    }
                }
            });

        }


        bool cmdFinished = false;

        public Commands.ResponseResult ProcessResponseJob(SendingCommand command)
        {
            Commands.ResponseResult result;
            var expectedResponse = command.Command.Substring(0, 4).ToString().Replace("C", "R").ToString();
            cmdFinished = false;
            var cmdFinalResponse = string.Empty;
            var timeOut = 0;
            var delay = 100;
            var timeOutSkip = 0;
            var ackForResponse = false;
            var response = string.Empty;

            while (true)
            {
                var hasData = this._responseQueue.TryDequeue(out string cmd);
                if (hasData)
                {

                    Log?.Invoke("<---- " + cmd);
                    var data = cmd.Trim().Replace(" ", string.Empty).ToHexBytes();
                    if (data.Length > 1)
                    {
                        var rawData = data.ToList();

                        // Remove STX
                        rawData.RemoveAt(0);
                        // Remove LRC
                        rawData.RemoveAt(rawData.Count - 1);
                        // Remove ETX
                        rawData.RemoveAt(rawData.Count - 1);

                        response = rawData.ToArray().ToAsiiString();

                        Log?.Invoke("<---- " + cmd);
                        Log?.Invoke("      " + response);
                        //this.Ack();
                        //try
                        //{
                        //    ProcessResponseData(response);
                        //}
                        //catch (Exception ex)
                        //{
                        //    Log?.Invoke("UNEXPECTED ERROR");
                        //    result = Commands.ResponseResult.UNEXPECTED_ERROR;
                        //    break;
                        //}
                        if (response.StartsWith(expectedResponse))
                        {
                            cmdFinished = true;
                            ackForResponse = false;
                            cmdFinalResponse = response;
                            timeOutSkip = 0;
                            //this.Ack();
                        }


                    }



                    if (cmd == "04")
                    {
                        if (cmdFinished)
                        {
                            Log?.Invoke("FINISHED");

                            try
                            {
                                ProcessResponseData(response);
                                command.ResponseCallBack?.Invoke(cmdFinalResponse);
                                result = Commands.ResponseResult.FINISHED;
                                break;

                            }
                            catch (Exception ex)
                            {
                                Log?.Invoke("UNEXPECTED ERROR");
                                result = Commands.ResponseResult.UNEXPECTED_ERROR;
                                break;
                            }

                        }
                    }
                    else if (cmd == "15")
                    {
                        result = Commands.ResponseResult.NACK;
                        break;
                    }
                    else
                    {
                        this.Ack();
                    }

                    if (timeOutSkip >= 30)
                    {
                        if (cmdFinished)
                        {
                            // Skip 04
                            Log?.Invoke("Skip wait for 04, force finish command");

                            command.ResponseCallBack?.Invoke(cmdFinalResponse);
                            result = Commands.ResponseResult.FINISHED;
                            break;
                        }
                    }
                }
                if ((timeOut += delay) >= command.Timeout)
                {
                    Log?.Invoke("TIMEOUT");
                    SlackService?.SendAlert(RfidFridgeSetting.Machine.Name, "TERMINAL TIMEOUT TO GET RESPONSE");
                    result = Commands.ResponseResult.TIMEOUT;
                    break;
                }
                timeOutSkip += delay;
                Thread.Sleep(delay);
            }

            return result;
        }

        public bool Enquire()
        {
            Enq();
            return WaitingForAck();
        }

        public bool WaitingForAck()
        {
            return GetExpectedResponse("06", 1500) == "06";
        }

        public string SendAndRecv(string command, string expectedResponse = null, int timeout = 15000, bool cleanCallBack = true)
        {
            Debug?.Invoke($"SendAndRecv ----> {command} | Expect reponse {Convert.ToString(expectedResponse)}");
            if (cleanCallBack)
            {
                CleanCallBack();
            }
            var response = string.Empty;
            SendCommand(command, cmdResponse =>
            {
                if (expectedResponse == null)
                {
                    expectedResponse = command.Take(4).ToString().Replace("C", "R");
                }
                if (cmdResponse.StartsWith(expectedResponse))
                {
                    response = cmdResponse;
                }
            });

            var timeoutC = 0;
            while (true)
            {
                if (response != string.Empty)
                {
                    break;
                }
                if (++timeoutC >= timeout)
                {
                    break;
                }
                Thread.Sleep(1);
            }

            Debug?.Invoke("SendAndRecv <---- " + response);
            return response;
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public enum WalletLabel
        {
            GrabPay,
            Dash,
            Alipay,
            Wechat,
            DBSMax,
            DBSMAXDEMO
        }

        public class SendingCommand
        {
            public SendingCommand(string cmd, Action<string> responseCallBack, int timeout = 35000)
            {
                this.Command = cmd;
                this.ResponseCallBack = responseCallBack;
                this.Timeout = timeout;
            }
            public string Command;
            public Action<string> ResponseCallBack;
            public int Timeout;
        }

        public class Commands
        {
            private static int saleTimeout = 0;
            static Commands()
            {
                //saleTimeout = new keyValueSettingsService().GetIntValue(SettingKey.IucSaleTimeOut);
                // saleTimeout = 60;//60 seconds, Ha hardcode for default value
            }

            public static int CmdIdentity = 0;

            public static string Sale(int amount, int timeout = 60)
            {

                var tags = new List<CommandTag>()
                {
                    new CommandTag(1,3,timeout),
                    new CommandTag(4,12,amount),
                    new CommandTag(57,06,InreaseIdentity())
                };
                var cmd = new CommandInfomation("C200", "R200", tags);
                return cmd.ToString();
            }

            public static string Preauth(int amount, int timeout = 60)
            {

                var tags = new List<CommandTag>()
                {
                    new CommandTag(1,3,timeout),
                    new CommandTag(4,12,amount),
                    new CommandTag(57,06,InreaseIdentity())
                };
                var cmd = new CommandInfomation("C201", "201", tags);
                return cmd.ToString();
            }

            public static string CancelPreauth(string cardNumber, string expDate, string rrn, string invoice, string tid, string mid, int amount = 1)
            {

                var tags = new List<CommandTag>()
                {
                    new CommandTag(1,3,60),
                    new CommandTag(2,45,cardNumber),
                    new CommandTag(4,12,amount),
                    new CommandTag(14,25,expDate),
                    new CommandTag(37,12,rrn),
                    new CommandTag(41,8, tid),
                    new CommandTag(42,12, mid),
                    new CommandTag(62,06,invoice),

                    new CommandTag(57,06,InreaseIdentity())
                };
                var cmd = new CommandInfomation("C301", "R301", tags);
                return cmd.ToString();
            }
            public static string Contactless(int amount, int timeout = 60)
            {
                var tags = new List<CommandTag>()
                {
                    new CommandTag(1,3,timeout),
                    new CommandTag(4,12,amount),
                    new CommandTag(57,06,InreaseIdentity())
                };
                var cmd = new CommandInfomation("C630", "R630", tags);
                return cmd.ToString();
            }

            public static string Mpqr(int amount, WalletLabel walletLabel, int timeout = 60)
            {
                var tags = new List<CommandTag>()
                {
                    new CommandTag(1,3,timeout),
                    new CommandTag(4,12,amount),
                    new CommandTag(54,walletLabel.ToString().Length,walletLabel.ToString()),
                    new CommandTag(57,06,InreaseIdentity())
                };
                var cmd = new CommandInfomation("C640", "R430", tags);
                return cmd.ToString();
            }

            public static string Cpas(int amount, int timeout = 60)
            {
                var tags = new List<CommandTag>()
                {
                    new CommandTag(1,3,timeout),
                    new CommandTag(4,12,amount),
                    new CommandTag(57,06,InreaseIdentity())
                };
                var cmd = new CommandInfomation("C610", "R610", tags);
                return cmd.ToString();
            }
            public static string CheckCpasBalance()
            {
                var tags = new List<CommandTag>()
                {
                    new CommandTag(57,06,InreaseIdentity())
                };
                var cmd = new CommandInfomation("C610", "R610", tags);
                return cmd.ToString();
            }

            public static string Void(string invoiceNumber)
            {
                var tags = new List<CommandTag>()
                {
                    new CommandTag(62,06,invoiceNumber),
                    new CommandTag(57,06,InreaseIdentity()),
                };
                var cmd = new CommandInfomation("C300", "R300", tags);
                return cmd.ToString();
            }

            public static string Settlement()
            {
                var tags = new List<CommandTag>()
                {
                    new CommandTag(57,06,InreaseIdentity())
                };
                var cmd = new CommandInfomation("C700", "R700", tags);
                return cmd.ToString();
            }

            public static int InreaseIdentity()
            {
                CmdIdentity++;
                if (CmdIdentity == 999999)
                    CmdIdentity = 0;
                return CmdIdentity;
            }
            public class CommandInfomation
            {
                public CommandInfomation(string command, string responseCommand, List<CommandTag> tags)
                {
                    Command = command;
                    ResponseCommand = responseCommand;
                    Tags = tags;
                }
                public string Command { get; set; }
                public string ResponseCommand { get; set; }
                public List<CommandTag> Tags { get; set; }

                public override string ToString()
                {
                    var tags = string.Empty;
                    if (Tags != null && Tags.Count > 0)
                    {
                        Tags.ForEach(x => tags += x.ToString());
                    }
                    return $"{Command}{tags}";
                }
            }

            public class CommandTag
            {
                public CommandTag(int tag, int length, object value)
                {
                    Tag = tag;
                    Length = length;
                    Value = value;
                }
                public int Tag { get; set; }
                public int Length { get; set; }
                public object Value { get; set; }

                public override string ToString()
                {
                    var tag = Tag.ToString().PadLeft(2, '0');
                    var length = Length.ToString().PadLeft(2, '0');
                    var value = Value?.ToString().PadLeft(Length, '0');

                    return $"{tag}{length}{value}";
                }
            }

            public enum ResponseResult
            {
                FINISHED,
                NACK,
                TIMEOUT,
                FAIL_TO_GOT_ACK,
                UNEXPECTED_ERROR
            }
        }
        #endregion
    }


    #region EXT


    #endregion

}
