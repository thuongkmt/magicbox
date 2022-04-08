using System.ComponentModel;
using System.Windows;
using Caliburn.Micro;
using ChillerBrain.ViewModels;

namespace ChillerBrain.Views
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
            ////ShowInTaskbar = false;
            //var svc = IoC.Get<IModuleManagementService>();
            //svc.MonitorChilledDisplay(this);
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

        private void ButtonOpenControl_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = IoC.Get<ShellViewModel>();
            var controlWindow = new ControlWindow();
            var control = vm.GetControl();
            controlWindow.FormHost.Child = control;
            controlWindow.ShowDialog();
        }
    }
}
