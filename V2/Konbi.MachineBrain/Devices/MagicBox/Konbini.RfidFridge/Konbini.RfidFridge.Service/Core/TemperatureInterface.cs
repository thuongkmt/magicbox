using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Konbini.RfidFridge.Service.Core
{
    public class TemperatureInterface
    {
        [DllImport("Temperature.dll", EntryPoint = "GetTempCount", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetTempCount();

        [DllImport("Temperature.dll", EntryPoint = "GetTemp", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetTemp(int count);
        private double Rs232Temp1 { get; set; }
        private double Rs232Temp2 { get; set; }
        private Timer _timer = new Timer();
        private Timer _timerCheckTempAbnormal = new Timer();
        private bool _reportErrorFlag;
        private int index;
        private int Offset;
        public enum ProbeType
        {
            Hid,
            Rs232,
            FridgeLock
        }
        public string ProbeComport { get; set; }

        public ProbeType ThermalProbeType { get; set; }
        SerialPort Port;
        public Action<string> OnError { get; set; }
        public List<double> Temperatures { get; set; }
        public Action<List<double>> OnTemperaturesReport { get; set; }
        public Action<List<double>> AlarmTemperatureAbnormal { get; set; }


        public int NormalTemperature { get; set; }


        public LogService LogService;
        public FridgeLockInterface FridgeLockInterface;

        public TemperatureInterface(LogService logService, FridgeLockInterface fridgeLockInterface)
        {
            LogService = logService;
            FridgeLockInterface = fridgeLockInterface;
        }

        /// <summary>
        /// Init parameters
        /// </summary>
        /// <param name="interval">Mins</param>
        public void Init(int interval, int normalTemperature, int index, int temperatureOffset)
        {
            this.index = index;
            this.Offset = temperatureOffset;
            if (ThermalProbeType == ProbeType.Rs232)
            {
                ConnectToRs232();
            }


            Temperatures = new List<double>() { -100, -100 };
            NormalTemperature = normalTemperature;

            _timer.Interval = interval;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = true;
            _timer.Start();


            _timerCheckTempAbnormal.Interval = 10 * 60 * 1000;
            _timerCheckTempAbnormal.Elapsed += _timerCheckTempAbnormal_Elapsed; ;
            _timerCheckTempAbnormal.Enabled = true;
            _timerCheckTempAbnormal.Start();
        }

        public bool ConnectToRs232()
        {
            try
            {
                Port = new SerialPort(ProbeComport)
                {
                    BaudRate = 9600,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    DtrEnable = true,
                    NewLine = "\n\r",
                    ReceivedBytesThreshold = 1024
                };
                Port.Open();

                if (!Port.IsOpen) return false;

                Action StartReadData = null;
                byte[] buffer = new byte[2000];
                StartReadData = (() => Port.BaseStream.BeginRead(buffer, 0, buffer.Length, delegate (IAsyncResult ar)
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
                        LogService.LogError(ex);
                    }
                    if (Port.IsOpen) StartReadData();
                }, null)); if (Port.IsOpen) StartReadData();

                return Port.IsOpen;
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                Port.Dispose();
                return false;
            }
        }

        private string TmpData { get; set; }

        private void RaiseAppSerialDataEvent(byte[] Data)
        {
            try
            {
                string result = Encoding.Default.GetString(Data);
                string t1Data;
                TmpData += result;

                //Console.WriteLine("============================");
                //Console.WriteLine(TmpData);

                //var a = TmpData.Contains("\n\r");
                //Console.WriteLine("Contain rn: " + a);
                //Console.WriteLine("Contain enviroment: " + TmpData.Contains(Environment.NewLine));

                if (TmpData.Contains("\n\r"))
                {
                
                    var splitedData = TmpData.Split(new[] { "\n\r" }, StringSplitOptions.RemoveEmptyEntries);
                    t1Data = splitedData.LastOrDefault(x => x.StartsWith("t1"));
                    //t2Data = splitedData.LastOrDefault(x => x.StartsWith("t2"));
                    //|| t2Data != null
                    if (t1Data != null )
                    {
                        TmpData = string.Empty;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }

                if (t1Data!= null && t1Data.Contains("t1="))
                {
                    var temp = t1Data.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1];
                    double rs232Temp = 0;
                    if (temp.StartsWith("+"))
                    {
                        temp = temp.Replace("+", string.Empty);
                        temp = temp.Trim();
                        double.TryParse(temp, out rs232Temp);

                    }
                    if (temp.StartsWith("-"))
                    {
                       
                        temp = temp.Replace("-", string.Empty);
                        temp = temp.Trim();
                        double.TryParse(temp, out rs232Temp);
                        rs232Temp = rs232Temp * -1;
                    }
                    //Console.WriteLine("===================================");
                    //Console.WriteLine(rs232Temp);
                    if (rs232Temp > -30)
                    {
                        Rs232Temp1 = rs232Temp;
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
        }

        private double GetRs232Temp1()
        {
            var tmp = Math.Round(Rs232Temp1 / 1f, 2);
            return tmp;
        }

        private double GetRs232Temp2()
        {
            var tmp = Math.Round(Rs232Temp2 / 1f, 2);
            return tmp;
        }

        private double GetTemp1()
        {
            var tmp = Math.Round(GetTemp(1) / 10f, 2);
            return tmp;
        }

        private double GetTemp2()
        {
            var tmp = Math.Round(GetTemp(2) / 10f, 2);
            return tmp;
        }

        private float GetFridgeLockTemp()
        {
            return FridgeLockInterface.GetTemperature();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ThermalProbeType == ProbeType.Hid)
            {
                if (GetTempCount() == 0)
                {
                    LogService.LogTemp("Can't detect temperature thermal probes!!!");
                    if (!_reportErrorFlag)
                    {
                        // Send to slack
                        OnError?.Invoke("Can't detect temperature thermal probes!!!");
                        _reportErrorFlag = true;
                    }
                }
                else
                {
                    _reportErrorFlag = false;
                    Temperatures[0] = GetTemp1();
                    Temperatures[1] = GetTemp2();
                    OffsetTmp();
                    OnTemperaturesReport?.Invoke(Temperatures);
                }
            }

            if (ThermalProbeType == ProbeType.Rs232)
            {
                if (!Port.IsOpen)
                {
                    LogService.LogTemp("Port is close!!!");
                    if (!_reportErrorFlag)
                    {
                        // Send to slack
                        OnError?.Invoke("Port is close!!!");
                        _reportErrorFlag = true;
                    }
                }
                else
                {
                    _reportErrorFlag = false;
                    Temperatures[0] = GetRs232Temp1();
                    Temperatures[1] = GetRs232Temp2();
                    OffsetTmp();
                    OnTemperaturesReport?.Invoke(Temperatures);
                }
            }

            if (ThermalProbeType == ProbeType.FridgeLock)
            {
                if (!true)
                {
                    LogService.LogTemp("Device not connected!!!");
                    if (!_reportErrorFlag)
                    {
                        // Send to slack
                        OnError?.Invoke("Device not connected!!!");
                        _reportErrorFlag = true;
                    }
                }
                else
                {
                    _reportErrorFlag = false;
                    Temperatures[0] = GetFridgeLockTemp();
                    Temperatures[1] = 0;
                    OffsetTmp();
                    OnTemperaturesReport?.Invoke(Temperatures);
                }
            }

        }

        private void OffsetTmp()
        {
            try
            {
                if (Temperatures != null)
                {
                    Temperatures[0] = Temperatures[0] + Offset;
                    Temperatures[1] = Temperatures[1] + Offset;
                }
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
        }


        private void _timerCheckTempAbnormal_Elapsed(object sender, ElapsedEventArgs e)
        {

            if (Temperatures[index] >= NormalTemperature)
            {
                AlarmTemperatureAbnormal?.Invoke(Temperatures);
            }
        }

        #region RS232

        #endregion
    }
}
