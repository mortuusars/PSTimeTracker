using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PhotoshopTimeCounter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string APP_NAME = "PhotoshopTimeCounter";
        private const int TOOLTIP_DELAY = 500;

        /// <summary>
        /// Path to Temp\PhotoshopTimeCounter\ folder.  Ends with backslash.
        /// </summary>
        public static string APP_FOLDER_PATH { get; private set; } = Path.GetTempPath() + APP_NAME + @"\";


        TimeCounter _counter;

        MainWindowViewModel _mainWindowViewModel;

        private void Application_Startup(object sender, StartupEventArgs e) {

            // Increase tooltip delay
            ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(TOOLTIP_DELAY));

            CreateTempFolder();

            _counter = new TimeCounter();
            _counter.Start();

            _mainWindowViewModel = new MainWindowViewModel(_counter);

            MainWindow = new MainWindow() { DataContext = _mainWindowViewModel};
            MainWindow.Show();
        }

        private void CreateTempFolder() {
            try {
                Directory.CreateDirectory(APP_FOLDER_PATH);
            }
            catch (Exception ex) {
                MessageBox.Show($"Cannot write to Temp folder: {ex.Message}", APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
