using System.ComponentModel;
using System.Windows;
namespace TemplateBrain.Views
{
    public partial class ShellView:Window
    {
        
        private void MyNotifyIcon_OnTrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
         
        }

        private void ShellView_OnLoaded(object sender, RoutedEventArgs e)
        {
          
        }

        private void ShellView_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
           
        }

        private void ShellView_OnClosing(object sender, CancelEventArgs e)
        {
            Caliburn.Micro.Action.Invoke(DataContext, "DisposeObjects");
        }

        private void ButtonOpenControl_OnClick(object sender, RoutedEventArgs e)
        {
          
        }
    }
}
