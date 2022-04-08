using Caliburn.Micro;
using System.ComponentModel;
using System.Windows;
using System;

namespace RawInputBrain.Views
{
    using SharpLib.Hid;
    using SharpLib.Win32;
    using System.Windows.Interop;

    public partial class ShellView : Window
    {

        private void MyNotifyIcon_OnTrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            //bool isMinimized = this.WindowState == WindowState.Minimized;
            //this.WindowState = (isMinimized) ? WindowState.Normal : WindowState.Minimized;
        }

        //private Handler iHidParser;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);

            var a = IoC.Get<RawInputInterface>();
            a.WindowHandle = new WindowInteropHelper(this).Handle;
        }
        System.Windows.Forms.Message message = new System.Windows.Forms.Message();
        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            Handler iHidParser = IoC.Get<RawInputInterface>().iHidParser;

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
        
        private void ShellView_OnLoaded(object sender, RoutedEventArgs e)
        {
            ////ShowInTaskbar = false;
            //var svc = IoC.Get<IModuleManagementService>();
            //svc.MonitorRawInputDisplay(this);
        }

        private void ShellView_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //bool isMinimized = this.WindowState == WindowState.Minimized;
            //this.ShowInTaskbar = !isMinimized;
        }

        private void ShellView_OnClosing(object sender, CancelEventArgs e)
        {
            Caliburn.Micro.Action.Invoke(DataContext, "DisposeObjects");
        }
    }
}
