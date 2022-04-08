using Caliburn.Micro;
using Konbini.RfidFridge.TagManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Konbini.RfidFridge.TagManagement.Views
{
    /// <summary>
    /// Interaction logic for LoginViews.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
           
        }

       
    }
}
