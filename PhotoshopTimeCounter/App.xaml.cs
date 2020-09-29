using System;
using System.IO;
using System.Windows;

namespace PhotoshopTimeCounter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string APP_NAME = "PhotoshopTimeCounter";

        /// <summary>
        /// Path to Temp\PhotoshopTimeCounter\ folder.  Ends with backslash.
        /// </summary>
        public static string APP_FOLDER_PATH { get; private set; } = Path.GetTempPath() + APP_NAME + @"\";


        TimeCounter _counter;

        MainWindowViewModel _mainWindowViewModel;

        private void Application_Startup(object sender, StartupEventArgs e) {

            CreateTempFolder();

            _counter = new TimeCounter();
            _counter.Start();


            MainWindow = new MainWindow();

            _mainWindowViewModel = new MainWindowViewModel(_counter);
            _mainWindowViewModel.AlwaysOnTop = MainWindow.Topmost;  // Workaround to setting saved AlwaysOnTop on load.

            MainWindow.DataContext = _mainWindowViewModel;
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
