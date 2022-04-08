﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using Caliburn.Micro;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Hid = SharpLib.Hid;
using SharpLib.Win32;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace RawInputBrain
{
    using RawInputBrain.ViewModels;
    using System.Diagnostics;
    using System.Text;
    using System.Windows;
    using System.Windows.Forms;

    public class RawInputInterface
    {

        public IEventAggregator EventAggregator { get; set; }


        /// <summary>
        /// Can be used to register for WM_INPUT messages and parse them.
        /// For testing purposes it can also be used to solely register for WM_INPUT messages.
        /// </summary>
        private Hid.Handler iHidHandler;
        public Action<Keys, string> OnKeyPress { get; set; }

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
                            ProductId = hidDevice.ProductId.ToString(),
                            VendorId = hidDevice.VendorId.ToString(),
                            Version = hidDevice.Version.ToString(),
                            Device = hidDevice,
                            Manufacturer = hidDevice.Manufacturer
                        };
                        devices.Add(deviceObj);

                        Console.WriteLine(deviceObj.FriendlyName);
                    }
                }
            }
            catch (Exception ex)
            {
                // LogService.LogRawInputInfo(ex.ToString());
            }


            return devices;
        }

        public IntPtr WindowHandle;
        public void RegisterDevice(RawInputDevice selectedDevice, Window window, System.Action OnRegisterSuccess = null, System.Action OnRegisterFail = null)
        {
            try
            {
                DisposeHandlers();

                Hid.Device device = (Hid.Device)selectedDevice.Device;
                int i = 0;
                RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[1];

                //HwndSource source = PresentationSource.FromVisual(window) as HwndSource;
                //source.AddHook(WndProc);

                IntPtr windowHandle = WindowHandle; //new WindowInteropHelper(window).Handle;
                rid[i].usUsagePage = device.UsagePage;
                rid[i].usUsage = device.UsageCollection;
                rid[i].dwFlags = (RawInputDeviceFlags)256;
                rid[i].hwndTarget = windowHandle;


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
                //LogService.LogRawInputInfo(ex.ToString());
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
            if (aHidEvent.Device != null)
            //if (aHidEvent.Device.FriendlyName == RegisteredDevice.FriendlyName)

            {
                // Endsure correct device
                var devicePrefix = aHidEvent.Device.FriendlyName + aHidEvent.Device.ProductId + aHidEvent.Device.VendorId;
                var registeredDevicePrefix = RegisteredDevice.FriendlyName + RegisteredDevice.ProductId + RegisteredDevice.VendorId;
                Console.WriteLine("devicePrefix: " + devicePrefix);
                Console.WriteLine("registeredDevicePrefix: " + registeredDevicePrefix);

                //if (devicePrefix == registeredDevicePrefix)
                //{
                if (aHidEvent.IsButtonDown)
                {
                    var key = (Keys)aHidEvent.VirtualKey;
                    var c = GetCharsFromKeys(key, false);
                    OnKeyPress?.Invoke(key, c);
                }
                // }
            }


        }


        public void Lock(int ms)
        {
            while (true)
            {

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

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int ToUnicode(
            uint virtualKeyCode,
            uint scanCode,
            byte[] keyboardState,
            StringBuilder receivingBuffer,
            int bufferSize,
            uint flags
            );

        static string GetCharsFromKeys(Keys keys, bool shift)
        {
            var buf = new StringBuilder(256);
            var keyboardState = new byte[256];
            if (shift)
            {
                keyboardState[(int)Keys.ShiftKey] = 0xff;
            }
            ToUnicode((uint)keys, 0, keyboardState, buf, 256, 0);
            return buf.ToString();
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

}