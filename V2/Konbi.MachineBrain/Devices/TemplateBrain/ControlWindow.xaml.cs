using System.Windows;
using System.Windows.Forms.Integration;

namespace TemplateBrain
{
    /// <summary>
    /// Interaction logic for ControlWindow.xaml
    /// </summary>
    public partial class ControlWindow : Window
    {
        private WindowsFormsHost formHost;
        public ControlWindow()
        {
            InitializeComponent();
            System.Windows.Forms.Integration.WindowsFormsHost host =
     new System.Windows.Forms.Integration.WindowsFormsHost();

            //// Create the MaskedTextBox control.
            //MaskedTextBox mtbDate = new MaskedTextBox("00/00/0000");

            // Assign the MaskedTextBox control as the host control's child.
            //host.Child = mtbDate;

            // Add the interop host control to the Grid
            // control's collection of child controls.
            this.grid1.Children.Add(host);
            formHost = host;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Integration.WindowsFormsHost host =
       new System.Windows.Forms.Integration.WindowsFormsHost();

            //// Create the MaskedTextBox control.
            //MaskedTextBox mtbDate = new MaskedTextBox("00/00/0000");

            // Assign the MaskedTextBox control as the host control's child.
            //host.Child = mtbDate;

            // Add the interop host control to the Grid
            // control's collection of child controls.
            this.grid1.Children.Add(host);
            formHost = host;

        }

        public WindowsFormsHost FormHost { get { return formHost; } }
    }
}
