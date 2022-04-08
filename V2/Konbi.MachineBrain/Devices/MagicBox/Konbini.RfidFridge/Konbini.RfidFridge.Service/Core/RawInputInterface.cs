using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Threading;


using KonbiBrain.Messages;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Hid = SharpLib.Hid;
using SharpLib.Win32;
using System.Runtime.InteropServices;


namespace Konbini.RfidFridge.Service.Core
{
    using System.Diagnostics;
    using System.Windows.Forms;
    using System.Windows.Interop;

    public class RawInputInterface
    {
        private LogService LogService;
        public RawInputInterface(LogService logService)
        {
            LogService = logService;
        }
        /// <summary>
        /// Can be used to register for WM_INPUT messages and parse them.
        /// For testing purposes it can also be used to solely register for WM_INPUT messages.
        /// </summary>
        private Hid.Handler iHidHandler;
        public Action<Keys> OnKeyPress { get; set; }

        /// <summary>
        /// Just using another handler to check that one can use the parser without registering.
        /// That's useful cause only one windows per application can register for a range of WM_INPUT apparently.
        /// See: http://stackoverflow.com/a/9756322/3288206
        /// </summary>
        public Hid.Handler iHidParser;

        public delegate void OnHidEventDelegate(object aSender, Hid.Event aHidEvent);

        public RawInputDevice RegisteredDevice { get; set; }


        public List<RawInputDevice> GetDevicesList()
        {
            var devices = new List<RawInputDevice>();

            try
            {
                //Get our list of devices
                RAWINPUTDEVICELIST[] ridList = null;
                uint deviceCount = 0;
                int res = SharpLib.Win32.Function.GetRawInputDeviceList(ridList, ref deviceCount, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICELIST)));
                if (res == -1)
                {
                    //Just give up then
                    return null;
                }

                ridList = new RAWINPUTDEVICELIST[deviceCount];
                res = SharpLib.Win32.Function.GetRawInputDeviceList(ridList, ref deviceCount, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICELIST)));
                if (res != deviceCount)
                {
                    //Just give up then
                    return null;
                }

                //For each our device add a node to our treeview
                foreach (RAWINPUTDEVICELIST device in ridList)
                {
                    SharpLib.Hid.Device hidDevice;

                    //Try create our HID device.
                    try
                    {
                        hidDevice = new SharpLib.Hid.Device(device.hDevice);
                    }
                    catch /*(System.Exception ex)*/
                    {
                        //Just skip that device then
                        continue;
                    }

                    uint mceUsageId = (uint)Hid.UsagePage.WindowsMediaCenterRemoteControl << 16 | (uint)Hid.UsageCollection.WindowsMediaCenter.WindowsMediaCenterRemoteControl;
                    uint consumerUsageId = (uint)Hid.UsagePage.Consumer << 16 | (uint)Hid.UsageCollection.Consumer.ConsumerControl;
                    uint gamepadUsageId = (uint)Hid.UsagePage.GenericDesktopControls << 16 | (uint)Hid.UsageCollection.GenericDesktop.GamePad;

                    if (hidDevice.IsKeyboard || (hidDevice.IsHid && (hidDevice.UsageId == mceUsageId || hidDevice.UsageId == consumerUsageId || hidDevice.UsageId == gamepadUsageId)))
                    {
                        var deviceObj = new RawInputDevice()
                        {
                            Name = hidDevice.Name,
                            FriendlyName = hidDevice.FriendlyName,
                            ProductId = "" + hidDevice.ProductId.ToString(),
                            VendorId = "" + hidDevice.VendorId.ToString(),
                            Version = hidDevice.Version.ToString(),
                            Device = hidDevice,
                            Manufacturer = hidDevice.Manufacturer
                        };
                        devices.Add(deviceObj);

                        Console.WriteLine(deviceObj.ToString()) ;
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.LogInfo(ex.ToString());
            }


            return devices;
        }

        public IntPtr WindowHandle;
        public void RegisterDevice(RawInputDevice selectedDevice, System.Windows.Window window, System.Action OnRegisterSuccess = null, System.Action OnRegisterFail = null)
        {
            try
            {
                DisposeHandlers();

                Hid.Device device = (Hid.Device)selectedDevice.Device;
                int i = 0;
                RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[1];

                //HwndSource source = PresentationSource.FromVisual(window) as HwndSource;
                //source.AddHook(WndProc);

               // IntPtr windowHandle =  Process.GetCurrentProcess().MainWindowHandle;//new WindowInteropHelper(window).Handle;
                rid[i].usUsagePage = device.UsagePage;
                rid[i].usUsage = device.UsageCollection;
                rid[i].dwFlags = (RawInputDeviceFlags)0;
                rid[i].hwndTarget = WindowHandle;


                iHidHandler = new SharpLib.Hid.Handler(rid, false, -1, -1);
                if (!iHidHandler.IsRegistered)
                {
                    Console.WriteLine("Failed to register raw input devices: " + Marshal.GetLastWin32Error().ToString());
                    OnRegisterFail?.Invoke();
                    return;
                }

                iHidParser = iHidHandler;

                RegisteredDevice = selectedDevice;

                iHidParser.OnHidEvent += HandleHidEventThreadSafe;
                OnRegisterSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                LogService.LogInfo(ex.ToString());
            }

        }


        System.Windows.Forms.Message message = new System.Windows.Forms.Message();

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (iHidParser != null)
            {
                message.HWnd = hwnd;
                message.Msg = msg;
                message.LParam = lParam;
                message.WParam = wParam;

                switch (message.Msg)
                {
                    case Const.WM_INPUT:
                        {
                            Console.WriteLine("INPUT MSG");
                            iHidParser.ProcessInput(ref message);
                        }
                        break;
                }

            }
            return IntPtr.Zero;
        }

        public void HandleHidEventThreadSafe(object aSender, SharpLib.Hid.Event aHidEvent)
        {
            if (aHidEvent.IsStray)
            {
                //Stray event just ignore it
                return;
            }
            if (aHidEvent.Device.FriendlyName == RegisteredDevice.FriendlyName)
            {
                Console.WriteLine((Keys)aHidEvent.VirtualKey);
                if (aHidEvent.IsButtonDown)
                {
                    var key = (Keys)aHidEvent.VirtualKey;
                    OnKeyPress?.Invoke(key);
                }
            }

        }
        private void DisposeHandlers()
        {
            if (iHidHandler != null)
            {
                //First de-register
                iHidHandler.Dispose();
                iHidHandler = null;
            }

            if (iHidParser != null)
            {
                //First de-register
                iHidParser.Dispose();
                iHidParser = null;
            }

        }
    }

    public class RawInputDevice
    {
        public string FriendlyName;
        public string Name;
        public string Manufacturer;
        public string ProductId;
        public string VendorId;
        public string Version;
        public object Device;

        public override string ToString()
        {
            return $"{Manufacturer} - {FriendlyName} PID:{ProductId}, VID:{VendorId}";
        }
    }

}