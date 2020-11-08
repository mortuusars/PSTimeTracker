using System.Windows.Controls;

namespace PSTimeTracker.UI
{
    /// <summary>
    /// Interaction logic for RestoreDialog.xaml
    /// </summary>
    public partial class RestoreDialog : UserControl
    {
        public RestoreDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
