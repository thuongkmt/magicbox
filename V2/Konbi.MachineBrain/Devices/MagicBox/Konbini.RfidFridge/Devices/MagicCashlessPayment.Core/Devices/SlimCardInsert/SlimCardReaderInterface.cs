using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicCashlessPayment.Core.Devices.SlimCardInsert
{
    public partial class SlimCardReaderInterface
    {
        public string ComportName { get; set; }

        public SerialPort Port;
        public Action<string> LogInfo { get; set; }
        public Action<string> LogHardware { get; set; }
        public Action<string> LogError { get; set; }

        public bool DebugMode { get; set; }
        public StatusReponse Status { get; set; }
        public DateTime LastPoll { get; set; }


        private System.Timers.Timer _timer = new System.Timers.Timer();
        private object _lock = new object();
        private const int ACK_TIMEOUT = 2000;
        private const int CMD_TIMEOUT = 2000;
        private Queue<byte[]> ResponseCommandsQueue = new Queue<byte[]>();
        private StringBuilder _cmdBuilder = new StringBuilder();
        #region Serial Port
        public void Disconnect()
        {
            Port?.Close();
        }
        public bool ConnectPort(string port)
        {
            var result = false;

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
                        OnDataReceived(dst);
                    }
                    catch (Exception ex)
                    {
                        LogInfo?.Invoke(ex.ToString());
                    }
                    if (Port.IsOpen) StartReadData();
                }, null));

                if (Port.IsOpen)
                {
                    StartReadData();
                }

                ComportName = port;
            }
            catch (Exception ex)
            {
                LogInfo?.Invoke(ex.ToString());
                Port.Dispose();
            }

            return result;
        }

        private void OnDataReceived(byte[] bytes)
        {
            try
            {
                var dataOnThisSession = new List<byte>();

                foreach (var Data in bytes)
                {
                    if (Data == ACK)
                    {
                        LogInfo?.Invoke("RX: ACK");

                        QueueResponse(new byte[] { ACK });
                        return;
                    }
                    if (Data == 0x15)
                    {
                        LogInfo?.Invoke("RX: NACK");
                        QueueResponse(new byte[] { NACK });
                        return;
                    }

                    dataOnThisSession.Add(Data);
                }

                if (DebugMode)
                {
                    LogInfo?.Invoke("<<< " + dataOnThisSession.ToArray().ToHexString());
                }
                _cmdBuilder.Append(dataOnThisSession.ToArray().ToHexStringNoSpace().Trim());


                var currentCmdBuilder = _cmdBuilder.ToString().Trim();
                if (currentCmdBuilder.StartsWith("02"))
                {
                    if(currentCmdBuilder.Length > 6)
                    {
                        var lengthString = currentCmdBuilder.Substring(2, 4);
                        var length = int.Parse(lengthString);

                        var endIndex = length * 2 + 10;
                        if (endIndex >= 0)
                        {
                            var cmd = _cmdBuilder.ToString().Substring(0, endIndex);
                            if (DebugMode)
                            {
                                LogInfo?.Invoke("CMD:" + cmd + "|");
                            }
                            _cmdBuilder = _cmdBuilder.Remove(0, cmd.Length);
                            if (DebugMode)
                            {
                                LogInfo?.Invoke("LEFT:" + _cmdBuilder + "|");
                            }

                            QueueResponse(cmd.Trim().StringToByteArray());
                        }
                    }
                    
                }
            }
            catch(System.ArgumentOutOfRangeException)
            {

            }
            catch (Exception ex)
            {
                if(DebugMode)
                {
                    LogInfo?.Invoke(ex.ToString());
                }
            }
        }
        #endregion

        public void CurrentStatus(Action<byte[]> callback = null, Action<string> errorCallback = null)
        {
            var cmd = Commands.CheckStatus.CardStatus();
            SendCommand(cmd, "CHECK STATUS", callback, errorCallback);
        }

        public void Eject()
        {
            var cmd = Commands.ControlCommand.EjectCard();
            SendCommand(cmd, "EJECT CARD");
        }

        public void OpenLatch(bool open)
        {
            if(open)
            {
                Eject();
            }
        }

        public void Enq()
        {
            LogHardware?.Invoke($"TX: ENQ");
            SerialPortWriteData(Commands.Generic.EQN());
        }

        public void Test()
        {
            CurrentStatus((response) =>
            {
                Status = new StatusReponse(response);
                LastPoll = DateTime.Now;

                Console.WriteLine("IsCardInserted: " + Status.IsCardInserted());
                Console.WriteLine("CMD: " + Status.ToString());
            });
          

        }


        public void StartPoll(int interval)
        {
            void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                CurrentStatus((response) =>
                {
                    Status = new StatusReponse(response);
                    LastPoll = DateTime.Now;
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


        #region Serial Port processing
        private void SendCommand(byte[] cmd, string cmdName, Action<byte[]> callback = null,Action<string> errorCallback = null)
        {
            Thread thread = new Thread(() => SendCommandThread(cmd, cmdName, callback, errorCallback));
            thread.Start();
        }

        private void SendCommandThread(byte[] cmd, string cmdName, Action<byte[]> callback = null, Action<string> errorCallback = null)
        {
            lock (_lock)
            {
                CleanCommandQueue();
                var result = WriteCommand(cmd, cmdName, callback, errorCallback);
                if (!result)
                {
                    LogHardware?.Invoke($"Failed to send command [{cmdName}]");
                    errorCallback?.Invoke($"Failed to send command [{cmdName}]");
                }
                Thread.Sleep(20);
            }
        }

        public bool WriteCommand(byte[] cmd, string cmdName, Action<byte[]> callback = null, Action<string> errorCallback = null)
        {
            //lock (_lockSend)
            //{
            try
            {
                var timeOut = 0;
                var tryingTime = 0;

            RESEND:
                // 1. Write command
                SerialPortWriteData(cmd);

                LogHardware?.Invoke($"TX: =====================[{cmdName}]=====================");
                LogHardware?.Invoke($"TX: {cmd.ToHexString()}");



                // 2. Waiting for ACK
                while (true)
                {
                    Thread.Sleep(20);
                    timeOut += 20;
                    if (timeOut >= ACK_TIMEOUT)
                    {
                        // Resend command
                        if (++tryingTime <= 3)
                        {
                            LogHardware?.Invoke("Timedout to get ACK, trying to resend command | Trytime: " + tryingTime);
                            var s = ResponseCommandsQueue.Select(x => x.ToHexString());
                            var ss = string.Join("|", s);
                            LogHardware?.Invoke($"Q data: {ss}");
                            goto RESEND;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    // Try to get ACK
                    if (ResponseCommandsQueue.Count > 0)
                    {
                        var ack = ResponseCommandsQueue.Peek();


                        if (ack[0] != ACK)
                        {
                            if (++tryingTime <= 3)
                            {
                                var message = $"Falied to get ACK, GOT {new byte[] { ack[0] }.ToHexString()}, trying to resend command";
                                LogError?.Invoke(message);
                                LogHardware?.Invoke(message);
                                goto RESEND;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            ResponseCommandsQueue.Dequeue();
                            //LogHardware?.Invoke("RX: ACK");
                            break;
                        }
                    }

                }
                // 3. Send ENQ
                Enq();

                // 4. Waiting for response
                timeOut = 0;
                tryingTime = 0;
                while (true)
                {
                    Thread.Sleep(20);
                    timeOut += 20;
                    if (timeOut >= CMD_TIMEOUT)
                    {
                        // Resend command
                        if (++tryingTime <= 3)
                        {
                            LogHardware?.Invoke("Timedout to get RESPONSE, trying to resend command");

                            var s = ResponseCommandsQueue.Select(x => x.ToHexString());
                            var ss = string.Join("|", s); 
                            LogHardware?.Invoke($"Q data: {ss}");

                            goto RESEND;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    if (ResponseCommandsQueue.Count > 0)
                    {
                        // Try to get ACK
                        var command = ResponseCommandsQueue.Peek();
                        if (command[0] != STX)
                        {
                            if (++tryingTime <= 3)
                            {
                                LogHardware?.Invoke($"Falied to get COMMAND, GOT {command.ToHexString()}, trying to resend command");
                                goto RESEND;
                            }
                            else
                            {
                                return false;
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

                return true;
            }
            catch (Exception ex)
            {
                LogHardware?.Invoke(ex.ToString());

                return false;
            }
        }

        private void SerialPortWriteData(byte[] cmd)
        {
            if (Port.IsOpen)
            {
                Port?.Write(cmd, 0, cmd.Length);
            }
            else
            {
                LogInfo?.Invoke("Port is closed, retry to send command");
                ConnectPort(Port.PortName);
                Port?.Write(cmd, 0, cmd.Length);
            }
        } 
        #endregion


    }
}
