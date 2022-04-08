using KonbiBrain.Messages;
using NsqSharp;
using StompSharp;
using StompSharp.Messages;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace BillAcceptorBrain
{
    public class BillAcceptorService:IHandler,IDisposable
    {
        private const string HEART_BEAT = "33 30 20 30 39 0D 0A";
        private const string FAILED_RESPONSE = "46 46";
        private const string ACK = "30 30 20 0D 0A";
        private const string RECEIPT_MONEY_PREFIX = "33 30 20 38 3";
        private string serialResponse = "";
        private readonly Consumer consumer=null;
        private StompClient client;
        private static HttpClient httpClient;


        /// <summary>
        /// 33 30 20 38 32 20 30 39 0D 0A  => 10$
        /// 33 30 20 38 30 20 30 39 0D 0A => 2$
        /// 33 30 20 38 31 20 30 39 0D 0A => 5$
        /// 33 30 20 38 33 20 30 39 0D 0A => 50$
        /// </summary>
        public readonly SerialPort serialPort;

        private readonly Queue<CommandInfo> commandQueue;
        private readonly System.Timers.Timer writeSerialPortTimer;

        public BillAcceptorService(string portName)
        {
            commandQueue=new Queue<CommandInfo>();
            serialPort=new SerialPort(portName, 9600,Parity.None,8,StopBits.One);
            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.ErrorReceived += SerialPort_ErrorReceived;
            //serialPort.Open();
            Task.Factory.StartNew(() =>
            {
                //first check if the port is already open
                //if its open then close it
                if (serialPort.IsOpen) serialPort.Close();
                serialPort.ReceivedBytesThreshold = 1;
                serialPort.Handshake = Handshake.None;
                //now open the port
                serialPort.Open();
            });

            writeSerialPortTimer=new System.Timers.Timer();
            writeSerialPortTimer.Interval = 1000;
            writeSerialPortTimer.Elapsed += WriteSerialPortTimer_Elapsed;
            writeSerialPortTimer.Enabled = true;
            writeSerialPortTimer.Start();


            consumer = new Consumer(NsqTopics.BILL_ACCEPTOR_TOPIC, NsqConstants.NsqDefaultChannel);
            consumer.AddHandler(this);
            Console.WriteLine(NsqConstants.NsqUrlConsumer);
            consumer.ConnectToNsqLookupd(NsqConstants.NsqUrlConsumer);

            client = new StompClient(ConfigurationManager.AppSettings["RabbitMqServerIp"], 61613);
        }

        private void WriteSerialPortTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var cmd = commandQueue.Peek();
            if (cmd != null && !cmd.IsSent)
            {
                Console.WriteLine($"Sending to port {cmd.HexCommand}");
                WriteData(cmd.HexCommand);                                            
                commandQueue.Peek().IsSent = true;
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //retrieve number of bytes in the buffer
            int bytes = serialPort.BytesToRead;
            //create a byte array to hold the awaiting data
            byte[] comBuffer = new byte[bytes];
            //read the data and store it
            serialPort.Read(comBuffer, 0, bytes);
            //display the data to the user
            var hexStr = ByteToHex(comBuffer);
            Console.WriteLine($"Receiving ...{hexStr}");
            serialResponse += " " + hexStr;
            serialResponse = serialResponse.Trim();

            var trimResponse = serialResponse.Trim().Replace(" ", "");
            if (trimResponse.Contains("0D0A"))
            {
                Console.WriteLine($"Received ...{serialResponse}!");
                if (serialResponse.Contains(HEART_BEAT))
                {
                    Console.WriteLine($"HEART BEAT");
                    serialResponse = "";
                    if (commandQueue.Count>0 && !commandQueue.Peek().NeedAck)
                    {
                        commandQueue.Dequeue();
                    }
                }
                else if (serialResponse.StartsWith(FAILED_RESPONSE))
                {
                    serialResponse = "";
                    var cmd = commandQueue.Peek().HexCommand;
                    Console.WriteLine($"Detected failed response, resend {cmd}");
                    commandQueue.Dequeue();
                    //Thread.Sleep(100);
                    //serialPort.Write(cmd);                    
                }
                else if(serialResponse.Contains(ACK))
                {
                    Console.WriteLine("Detected ACK");
                    //command acknowledge, deque
                    Console.WriteLine("Release current command");
                    serialResponse = "";
                    commandQueue.Dequeue();
                    //var cmd = commandQueue.Peek();
                    //if (!cmd.NeedWaitResponse)
                    //{
                    //    Console.WriteLine("Release current command");
                    //    serialResponse = "";
                    //    commandQueue.Dequeue();
                    //}
                }
                else if(serialResponse.StartsWith(RECEIPT_MONEY_PREFIX))
                {
                    Console.WriteLine("Received money");
                    //parse money received
                    ParseMoneyReceived(serialResponse);
                    serialResponse = "";
                }
                else
                {
                    Console.WriteLine($"Reset response");
                    serialResponse = "";
                }
            }
            
        }

        public void Reset()
        {
            commandQueue.Clear();
            SendCommand("30",needAck:false);
        }

        public void Enable()
        {
             SendCommand("34 00 1F 00 00",true, RECEIPT_MONEY_PREFIX);
        }

        public void Disable()
        {
            SendCommand("34 00 00 00 00");
        }

        /// <summary>
        /// 33 30 20 38 32 20 30 39 0D 0A => 10$
        /// 33 30 20 38 30 20 30 39 0D 0A => 2$
        /// 33 30 20 38 31 20 30 39 0D 0A => 5$
        /// 33 30 20 38 33 20 30 39 0D 0A => 50$
        /// </summary>
        private void ParseMoneyReceived(string response)
        {
            var data = response.Remove(0, RECEIPT_MONEY_PREFIX.Length);
            var moneyReceived = data.Substring(0, 1);
            var cents = 0;
            if (moneyReceived == "0")
            {
                Console.WriteLine("Received 2$");
                cents = 200;
            }

            if (moneyReceived == "1")
            {
                Console.WriteLine("Received 5$");
                cents = 500;
            }

            if (moneyReceived == "2")
            {
                Console.WriteLine("Received 10$");
                cents = 1000;
            }

            if (moneyReceived == "3")
            {
                Console.WriteLine("Received 50$");
                cents = 5000;
            }

            httpClient=new HttpClient();
            var url = ConfigurationManager.AppSettings["MachineAdminBackendUrl"];
            url += $"/api/services/app/UserCredits/AddTopup?userName=hadoan&cents={cents}";

            httpClient.PostAsync(url, null).Wait();
            SendStompMessage($"HADOAN_TOPUP_{cents}").Wait();
        }

        private void SendCommand(string hexCmd,bool waitResponse=false,string responePrefix="",bool needAck=true)
        {
            var cmd = new CommandInfo()
            {
                NeedAck = needAck,
                HexCommand = hexCmd,
                SendingTime = DateTime.Now,
                NeedWaitResponse = waitResponse,
                WaitResponsePrefix = responePrefix
            };
            Console.WriteLine($"Sending command... {hexCmd}");
            commandQueue.Enqueue(cmd);
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

        public void WriteData(string msg)
        {
            serialPort.DtrEnable = true;
            //convert the message to byte array
            byte[] newMsg = HexToByte(msg);
            //send the message to the port
            serialPort.Write(newMsg, 0, newMsg.Length);
            //DisplayBytesData(MessageType.Outgoing, newMsg);
            serialPort.DtrEnable = false;
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

        public void HandleMessage(NsqSharp.IMessage message)
        {
            string msg = Encoding.UTF8.GetString(message.Body);
            msg = msg.Trim('\"');
            if(msg=="enable") Enable();
            else if(msg=="disable") Disable();
            else if(msg=="reset") Reset();
            
        }

        public void LogFailedMessage(NsqSharp.IMessage message)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            consumer?.Stop();
            serialPort?.Close();
            client?.Dispose();
        }


        private async Task SendStompMessage(string message)
        {
            var destination = client.GetDestination("/topic/demo", client.SubscriptionBehaviors.AutoAcknowledge);
            IReceiptBehavior receiptBehavior =
                new ReceiptBehavior(destination.Destination, client.Transport.IncommingMessages);
            receiptBehavior = NoReceiptBehavior.Default;
            var bodyOutgoingMessage =
                (new BodyOutgoingMessage(Encoding.ASCII.GetBytes(message))).WithPersistence();
            await destination.SendAsync(bodyOutgoingMessage, receiptBehavior);

        }
    }

    
}
