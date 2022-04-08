using Caliburn.Micro;
using System.ComponentModel;
using System.Windows;

namespace MdbCashlessBrain.Views
{
    public partial class ShellView:Window
    {
        
        private void MyNotifyIcon_OnTrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            //bool isMinimized = this.WindowState == WindowState.Minimized;
            //this.WindowState = (isMinimized) ? WindowState.Normal : WindowState.Minimized;
        }

        private void ShellView_OnLoaded(object sender, RoutedEventArgs e)
        {
            //var svc = IoC.Get<IModuleManagementService>();
            //svc.MonitorMdbDisplay(this);
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
