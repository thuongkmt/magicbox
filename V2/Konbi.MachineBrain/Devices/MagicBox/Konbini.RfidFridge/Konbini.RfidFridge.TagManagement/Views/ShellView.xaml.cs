using Caliburn.Micro;
using Konbini.RfidFridge.TagManagement.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace Konbini.RfidFridge.TagManagement.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        private int CountToAdmin = 0;

        public ShellView()
        {
            InitializeComponent();
        }

        private void ShellView_OnLoaded(object sender, RoutedEventArgs e)
        {
            ShowInTaskbar = true;
            Title = "Tag Management v2.0";
        }


        public void ActiveMainWindow()
        {
            Activate();
            Focus();
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12)
            {
                if (++CountToAdmin >= 5)
                {
                    EnableAdmin();
                    CountToAdmin = 0;
                }
            }

            Caliburn.Micro.Action.Invoke(DataContext, "OnKeyPress", null, null, null, new [] { sender, e });
        }

        public void EnableAdmin()
        {
            var viewModel = (ShellViewModel)DataContext;
            viewModel.IsAdminMode = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var viewModel = IoC.Get<MainViewModel>();
            viewModel.RfidReaderInterface.StopRecord();
        }
    }
}
