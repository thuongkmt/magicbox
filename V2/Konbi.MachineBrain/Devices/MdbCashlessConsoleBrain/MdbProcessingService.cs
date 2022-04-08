using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using KonbiBrain.Messages;
using Konbini.Messages.Commands;
using Konbini.Messages.Enums;
using System.IO.Ports;
using Konbi.Common.Interfaces;
using KonbiBrain.Interfaces;
using KonbiBrain.Common.Messages.Payment;

namespace MdbCashlessBrain
{
    public class MdbProcessingService
    {
        // Private fields for device.
        private string portName;
        private SerialPort port;
        private const int baudRate = 9600;
        private BackgroundWorker mdbDeviceBackgroundWorker;
        private MdbCashlessResponseResult mdbCashlessResponseResult;
        private string responseHexData = "";
        private List<string> incommingSignals;
        private bool isWaitPurchase;
        private Object thisLock = new Object();
        private readonly string MDB_RESPONSE_END = "0D 0A";

        public MdbProcessingService()
        {
            port = new SerialPort();
            port.BaudRate = baudRate;
            port.Parity = Parity.None;
            port.StopBits = StopBits.One;
            port.DataBits = 8;
            port.DataReceived += Port_DataReceived;

            InitialiseBackgroundWorkers();
            ResetIncomingData();
        }

        public decimal? PaymentAmount { get; set; }
        public Guid? TransactionId { get; set; }
        public Guid? CommandId { get; set; }

        public MdbCashlessResponseResult MdbCashlessResponseResult
        {
            get { return mdbCashlessResponseResult; }
            set
            {
                mdbCashlessResponseResult = value;
            }
        }
        public IKonbiBrainLogService LogService { get; set; }
        public IMessageProducerService NsqMessageProducerService { get; set; }
      
        public void InitMdb()
        {
            //Cancel reader first
            WriteAndCheckResponse("1402");
            //Reset
            WriteAndCheckResponse("10");
            //Configure
            WriteAndCheckResponse("110001000000");

            //Set max price
            WriteAndCheckResponse("1101FFFF0000");

            //Enable Card reader
            WriteAndCheckResponse("1401");
            Thread.Sleep(3000);

            //Disable
            DisableReader();
            ////Reset
            //WriteAndCheckResponse("10");
        }

        public void CancelTransaction()
        {
            IsWaitPurchase = false;
            //Reset
            //WriteAndCheckResponse("10");
        }
        public void EndTransaction()
        {
            WriteAndCheckResponse("1304");
        }

        public void EnableReader()
        {
            //WriteAndCheckResponse("1401");
            EnableAndCheckMdb();

            var command = new NsqPaymentACKResponseCommand(this.CommandId.Value,"MdbCashless");
            NsqMessageProducerService.SendNsqCommand(NsqTopics.PAYMENT_RESPONSE_TOPIC, command);

            var response = new NsqEnablePaymentResponseCommand(PaymentType.MdbCashless.ToString());
            response.Code = 1;
            NsqMessageProducerService.SendNsqCommand(NsqTopics.PAYMENT_RESPONSE_TOPIC, response);

        }

        public void DisableReader()
        {

            WriteAndCheckResponse("1400");
        }
        public bool IsWaitPurchase {
            get { return isWaitPurchase; }
            set
            {
                isWaitPurchase = true;
                //LogService.LogInfo($"Set MDB IsWaitPurchase {isWaitPurchase}");
            }
        }

        public void WaitSwipe()
        {
            ResetIncomingData();
        }

        public bool SendPurchase(decimal amount)
        {
            if (amount == 0) return false;
            Console.WriteLine($"SendPurchase {amount}");
            return SendPurchaseData((int)Math.Round(100 * amount));
        }

        private bool SendPurchaseData(int cents, int tryingTime = 0)
        {
            if (cents == 0) return false;
            //cents = 1;
            LogService.LogInfo("send purchase cents " + cents);
            //LogService.LogInfo("SendPurchase "+cents);
            //1300000A0001 //TODO: learn to set the price
            //WriteData("13 00 000A 0001"); //000A - Price in cents 0001 - Item number
            var hexCents = cents.ToString("X4");
            var dataSending = "1300" + hexCents + "0001";
            //WriteData("1300000A0001");
            var beginTime = DateTime.Now;

            var sendPurchaseSignals = WriteAndCheckResponse(dataSending);
            //LogService.LogInfo("sent purchase cents " + dataSending);
            Console.WriteLine("sent purchase cents " + dataSending);
            while (true)
            {
                if ((DateTime.Now - beginTime).TotalSeconds > 90)//Default is 45 + 45 //Terminal dials to pri host then sec host
                {
                    return false;
                }
                string[] incomingSignalCloned = ClonedIncomingData();
                foreach (var afterPurchase in incomingSignalCloned)
                {
                    if (!afterPurchase.Trim().EndsWith(MDB_RESPONSE_END)) continue;
                    var afterPurchaseFifthByte = GetFifthByte(afterPurchase);
                    if (afterPurchaseFifthByte == "4" || afterPurchaseFifthByte == "6" || afterPurchaseFifthByte == "8")//04 cancel request, 06 - Vend Denied, 08 - Cancelled
                    {
                        SendResultToMain(MdbCashlessResponseResult.Failed);
                        Console.WriteLine("Vend Denied");
                        LogService.LogInfo("MDB Vend Denied");
                        WriteAndCheckResponse("1304");

                        Task.Factory.StartNew(() =>//after sucess, disable reader
                        {
                            Thread.Sleep(3000);
                            DisableReader();
                        });
                        return false;
                    }
                    else if (afterPurchaseFifthByte == "5")//Vend Approved
                    {
                        LogService.LogInfo("MDB VendApproved");
                        return SendVendApproved();
                    }

                }
                Thread.Sleep(100);
            }

        }

        public void ProcessCheckMDB()
        {
            LogService.LogInfo("ProcessCheckMDB");

            var data = WriteAndCheckResponse("1401");

            if (data.Count() < 2)
                return;

            foreach (var item in data)
            {
                switch (item.Trim())
                {
                    case "30 30 20 0D 0A":
                        // ACK do nothing.
                        break;
                    case "31 30 20 30 42 0D 0A":
                        LogService.LogInfo("ProcessCheckMDB::case 42:: Send reset cmd");
                        // send reset cmd
                        InitMdb();
                        break;
                }
            }
        }
        public void EnableAndCheckMdb()
        {
            LogService.LogInfo("ProcessCheckMDB");

            var data = WriteAndCheckResponse("1401");

            if (data.Count() < 2)
                return;

            foreach (var item in data)
            {
                switch (item.Trim())
                {
                    case "30 30 20 0D 0A":
                        // ACK do nothing.
                        break;
                    case "31 30 20 30 42 0D 0A":
                        LogService.LogInfo("ProcessCheckMDB::case 42:: Send reset cmd");
                        // send reset cmd
                        InitMdb();
                        break;
                }
            }
        }

        public bool DisableAndCheckResponse()
        {
            LogService.LogInfo("Disable and check response");

            var data = QuickWriteAndCheckResponse("1400");
            return data.Any(x => x.Trim().EndsWith("30 30 20 0D 0A"));
        }


        private bool SendVendApproved(int tryingTime = 0)
        {

            SendResultToMain(MdbCashlessResponseResult.VendApproved);
            Console.WriteLine("Vend Approved");
            var beginTime = DateTime.Now;
            WriteAndCheckResponse("13020001");//SendDispatchSuccess
            Console.WriteLine("vend success");
            Thread.Sleep(500);
            WriteAndCheckResponse("1304");

            while (true)
            {
                if ((DateTime.Now - beginTime).TotalSeconds > 90)//Default is 45 + 45 //Terminal dials to pri host then sec host
                {
                    WriteAndCheckResponse("1304");
                    return false;
                }
                string[] incomingSignalCloned = ClonedIncomingData();
                foreach (var sendDispatchSuccess in incomingSignalCloned)
                {
                    if (!sendDispatchSuccess.Trim().EndsWith(MDB_RESPONSE_END)) continue;

                    var sendDispatchSuccessByte = GetFifthByte(sendDispatchSuccess);
                    if (sendDispatchSuccessByte == "7")//vend success
                    {
                        LogService.LogInfo("MDB VendSuccess");
                        SendResultToMain(MdbCashlessResponseResult.VendSuccess);
                        WriteAndCheckResponse("1304");
                        Task.Factory.StartNew(() =>//after sucess, disable reader
                        {
                            Thread.Sleep(3000);
                            DisableReader();
                        });
                        
                        return true;
                    }
                    else if ((DateTime.Now - beginTime).TotalSeconds > 60)//try to send every 30 seconds for 3 times
                    {
                        return false;
                    }
                }

                Thread.Sleep(500);
            }
        }

        private List<string> WriteAndCheckResponse(string hexString, int tryingTime = 0)
        {
            Console.WriteLine("WriteAndCheckResponse " + hexString);
            LogService.LogInfo($"MDB: WriteAndCheckResponse - {hexString}");
            //Cancel reader first
            ResetIncomingData();
            var beginTime = DateTime.Now;
            WriteData(hexString);
            while (true)
            {
                string[] incomingSignalCloned = ClonedIncomingData();
                
                if (incomingSignalCloned.Any(x => x.StartsWith("46"))) {
                    return WriteAndCheckResponse(hexString, tryingTime + 1);
                }
                if (incomingSignalCloned.Count() > 0 && incomingSignalCloned.Last().Trim().EndsWith(MDB_RESPONSE_END))
                {
                    var res = incomingSignalCloned.ToList();
                    if (hexString == "1101FFFF0000" || hexString == "1401" || hexString == "1400" || (hexString.StartsWith("1300") && hexString.EndsWith("0001")))
                        return res;
                    else if (incomingSignalCloned.Count() >= 2) {
                        //if ((hexString.StartsWith("1300") && hexString.EndsWith("0001")))//Send purchase need 3 responses
                        //{
                        //    if (res.Count >= 3) return res;
                        //}
                        //else return res;
                        return res;
                    }
                    else if (hexString == "1401")
                    {
                        // wait 1 second to 
                        if ((DateTime.Now - beginTime).TotalSeconds > 1)
                            return res;
                    }
                }
                if ((DateTime.Now - beginTime).TotalSeconds > 10)//try to send every 30 seconds for 3 times
                {
                    if (tryingTime <= 2) return WriteAndCheckResponse(hexString, tryingTime + 1);
                    else return new List<string>();
                }
                Thread.Sleep(500);
            }
        }

        private List<string> QuickWriteAndCheckResponse(string hexString, int tryingTime = 0)
        {
            Console.WriteLine("WriteAndCheckResponse " + hexString);
            LogService.LogInfo($"MDB: WriteAndCheckResponse - {hexString}");
            //Cancel reader first
            ResetIncomingData();
            var beginTime = DateTime.Now;
            WriteData(hexString);
            while (true)
            {
                string[] incomingSignalCloned = ClonedIncomingData();

                if (incomingSignalCloned.Any(x => x.StartsWith("46")))
                {
                    return WriteAndCheckResponse(hexString, tryingTime + 1);
                }
                if (incomingSignalCloned.Count() > 0 && incomingSignalCloned.Last().Trim().EndsWith(MDB_RESPONSE_END))
                {
                    return incomingSignalCloned.ToList();
                }
                if (++tryingTime == 3)
                {
                    return incomingSignalCloned.ToList();
                }
                Thread.Sleep(300);
            }
        }

        #region Methods
        /// <summary>
        /// Initialises the background workers that communicate with the devices.
        /// </summary>
        private void InitialiseBackgroundWorkers()
        {
            mdbDeviceBackgroundWorker = new BackgroundWorker();
            mdbDeviceBackgroundWorker.WorkerSupportsCancellation = true;
            mdbDeviceBackgroundWorker.DoWork += MdbDeviceBackgroundWorker_DoWork; ;
        }
        /// <summary>
        /// Sets the port name used for communication with the Ezlink device.
        /// </summary>
        /// <param name="portName"></param>
        public void SetPort(string portName)
        {
            if (port != null && port.IsOpen) port.Close();
            this.portName = portName;
            port.PortName = this.portName;
        }

        public void StartBackgroundWorker()
        {
            if (!mdbDeviceBackgroundWorker.IsBusy)
                mdbDeviceBackgroundWorker.RunWorkerAsync();
        }

        public void Stop()
        {
            if (port != null)
            {
                if (port.IsOpen)
                {
                    try
                    {
                        port.Close();
                    }
                    catch (Exception ex)
                    {
                        LogService.LogMdbError(ex);
                    }
                }
            }
        }

        public bool WriteData(string message)
        {
            if (port != null)
            {
                if (port.IsOpen == true)
                {
                    //LogService.LogInfo("Write data: " + message);
                    Console.WriteLine("Write data: " + message);
                    port.DtrEnable = true;
                    //convert the message to byte array
                    byte[] newMsg = HexToByte(message);
                    //send the message to the port
                    port.Write(newMsg, 0, newMsg.Length);
                    //convert back to hex and display
                    port.DtrEnable = false;
                    responseHexData = string.Empty;
                    //return DeviceResponseAcknowledge();
                    return true;
                }
            }
            return false;
        }

        public bool IsListenIncomingData { get; set; }
        private void ProcessIncomingData(string hexData)
        {
            try
            {
                Console.WriteLine($"Incoming data: {hexData}");

                //check fifth byte of incoming data
                //31 30 20 30 33 20 43 31 
                //31 B0 20 30 33 20 32 B7 20 B1 30 8D 0A
                //31 30 20 30 33 20 42 46 20 43 C2 0D 0A
                AddIncomingData(hexData);
                Task.Factory.StartNew(() => ValidateCardSwipe());

            }
            catch (Exception ex)
            {
                LogService.LogException(ex);
            }
        }

        private void ValidateCardSwipe()
        {
            var signals = ClonedIncomingData();
            var count = signals.Count();
            if (count == 0) return;
            if (!signals[count - 1].Trim().EndsWith(MDB_RESPONSE_END)) return;
            var hexData = signals[count - 1];
            var fifthByte = GetFifthByte(hexData);
           
            //if (fifthByte == "3" || fifthByte == "6" || fifthByte == "7")
            if (fifthByte == "3")
            {
                ProceedCardSwipe();
            }
        }

        private void ProceedCardSwipe()
        {
            LogService.LogInfo("ProceedCardSwipe");
            SendResultToMain(MdbCashlessResponseResult.CardSwiped);

            WriteAndCheckResponse("1304"); //end transaction
           
        }

        private void SendResultToMain(MdbCashlessResponseResult cashlessResponseResult)
        {

            var command = new NsqPaymentCallbackResponseCommand("MdbCashless");
            var state = GetPaymentStateFromMdbResponse(cashlessResponseResult);
            command.Response.Message = state.Item2;
            command.Response.OtherInfo = $"MdbCashlessResponseResult: {cashlessResponseResult}";
            command.Response.State = state.Item1;
            
            NsqMessageProducerService.SendNsqCommand(NsqTopics.PAYMENT_RESPONSE_TOPIC, command);

            MdbCashlessResponseResult = cashlessResponseResult;

            if (cashlessResponseResult == MdbCashlessResponseResult.CardSwiped && PaymentAmount.HasValue)
            {
                SendPurchase(PaymentAmount.Value);
                PaymentAmount = null;
            }
        }

        private Tuple<PaymentState,string> GetPaymentStateFromMdbResponse(MdbCashlessResponseResult response)
        {
            switch (response)
            {
                
                case MdbCashlessResponseResult.CardSwiped:
                    return new Tuple<PaymentState, string>(PaymentState.InProgress, "Processing, please do not remove your card!");
                case MdbCashlessResponseResult.Failed:
                    return new Tuple<PaymentState, string>(PaymentState.Failure, "Payment error, please try again!");
                case MdbCashlessResponseResult.VendApproved:
                    return new Tuple<PaymentState, string>(PaymentState.Success, "Payment success.");
                case MdbCashlessResponseResult.VendSuccess:
                    return new Tuple<PaymentState, string>(PaymentState.Success, "Payment success.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(response), response, null);
            }
        }
        private string GetFifthByte(string hexData)
        {
            //31 B0 20 30 33 20 32 B7 20 B1 30 8D 0A
            try
            {
                var x = hexData.Substring(13, 1);
                return x;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private byte[] HexToByte(string msg)
        {
            //remove any spaces from the string
            msg = msg.Replace(" ", "");
            //create a byte array the length of the
            //divided by 2 (Hex is 2 characters in length)
            byte[] comBuffer = new byte[msg.Length / 2];
            //loop through the length of the provided string
            for (int i = 0; i < msg.Length; i += 2)
                //convert each set of 2 characters to a byte
                //and add to the array
                comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
            //return the array
            return comBuffer;
        }

        private string ByteToHex(byte[] comByte)
        {
            //create a new StringBuilder object
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            //loop through each byte in the array
            foreach (byte data in comByte)
                //convert the byte to a string and add to the stringbuilder
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
            //return the converted value
            return builder.ToString().ToUpper();
        }

        private void LockAndAccessIncomingData(System.Action a)
        {
            lock (thisLock)
            {
                a();
            }
        }

        private void ResetIncomingData()
        {
            LockAndAccessIncomingData(() => incommingSignals = new List<string>());
        }

        private string[] ClonedIncomingData()
        {
            string[] incomingSignalCloned;
            lock (thisLock)
            {
                incomingSignalCloned = new string[incommingSignals.Count];
                incommingSignals.CopyTo(incomingSignalCloned);
            }
            return incomingSignalCloned;
        }

        private void AddIncomingData(string hexData)
        {
            LogService.LogInfo($"MDB incoming: {hexData}");
            LockAndAccessIncomingData(() => {
                var count = incommingSignals.Count();
                if (count > 0 && !incommingSignals[count - 1].Trim().EndsWith(MDB_RESPONSE_END))
                {
                    var last = incommingSignals[count - 1] + hexData;
                    incommingSignals[count - 1] = last;
                    Console.WriteLine("Modify last: " +last);
                }
                else incommingSignals.Add(hexData);
            });
        }
       
        #endregion

        #region Events
        private void MdbDeviceBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (mdbDeviceBackgroundWorker.CancellationPending == false)
            {
                if (port != null)
                {
                    if (port.IsOpen == false)
                    {
                        try
                        {
                            port.Open();
                            //Reset();
                        }
                        catch (Exception ex)
                        {
                            LogService.LogException(ex);
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }
        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //retrieve number of bytes in the buffer
            int bytes = port.BytesToRead;
            //create a byte array to hold the awaiting data
            byte[] comBuffer = new byte[bytes];
            //read the data and store it
            port.Read(comBuffer, 0, bytes);
            //display the data to the user
            //DisplayData(MessageType.Incoming, ByteToHex(comBuffer) + "\n");
            var hex = ByteToHex(comBuffer);

            ProcessIncomingData(hex);
        }
        #endregion

    }
}
