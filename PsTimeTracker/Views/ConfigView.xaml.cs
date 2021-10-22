using System.Windows;
using System.Windows.Input;

namespace PSTimeTracker.Views
{
    /// <summary>
    /// Interaction logic for ConfigView.xaml
    /// </summary>
    public partial class ConfigView : Window
    {
        public ConfigView()
        {
            InitializeComponent();
        }

        private void Close_LeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Header_LeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
