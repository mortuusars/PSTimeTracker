using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using PSTimeTracker.Core;

namespace PSTimeTracker.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string APP_NAME = "PSTimeTracker";

        private const string RECORD_FOLDER_NAME = "Records";
        private const int TOOLTIP_DELAY = 500;

        public static readonly string SESSION_ID = new Random((int)DateTimeOffset.Now.ToUnixTimeSeconds()).Next().ToString();

        // Paths to Temp\APPNAME\ and Temp\APPNAME\Records folders. Ends with backslash
        public static readonly string APP_FOLDER_PATH = Path.GetTempPath() + APP_NAME + @"\";
        public static readonly string APP_RECORDS_PATH = APP_FOLDER_PATH + RECORD_FOLDER_NAME + @"\";

        CollectorService _collectorService;
        RecordManager _recordManager;
        MainWindowViewModel _mainWindowViewModel;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SetupAppProperties();
            CreateTempFolder();


            ObservableCollection<PsFile> recordCollection = new ObservableCollection<PsFile>();

            _recordManager = new RecordManager(recordCollection);
            _collectorService = new CollectorService(recordCollection);


            _mainWindowViewModel = new MainWindowViewModel(recordCollection, _collectorService, _recordManager);

            MainWindow = new MainView() { DataContext = _mainWindowViewModel };
            MainWindow.Show();
        }

        private static void SetupAppProperties()
        {
            // Increase tooltip delay
            ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(TOOLTIP_DELAY));
        }

        #region Utility
        public static void DisplayErrorMessage(string message)
        {
            MessageBox.Show(message, APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void CreateTempFolder() {
            try {
                Directory.CreateDirectory(APP_FOLDER_PATH);
                Directory.CreateDirectory(APP_FOLDER_PATH + "/" + RECORD_FOLDER_NAME);
            }
            catch (Exception ex) {
                MessageBox.Show($"Cannot write to Temp folder: {ex.Message}", APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
