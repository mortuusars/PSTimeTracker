using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using PSTimeTracker.Services;
using PSTimeTracker.Core;
using PSTimeTracker.Models;

namespace PSTimeTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string APP_NAME = "PSTimeTracker";

        private const string RECORDS_FOLDER_NAME = "records";
        private const string CRASHES_FOLDER_NAME = "crash-reports";
        private const int TOOLTIP_DELAY = 500;

        public static readonly string SESSION_ID = new Random((int)DateTimeOffset.Now.ToUnixTimeSeconds()).Next().ToString();

        // Paths to Local/APPNAME and nested folders
        public static readonly string APP_FOLDER_PATH = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/{APP_NAME}/";
        public static readonly string APP_RECORDS_FOLDER_PATH = $"{APP_FOLDER_PATH}{RECORDS_FOLDER_NAME}/";
        public static readonly string APP_CRASHES_FOLDER_PATH = $"{APP_FOLDER_PATH}{CRASHES_FOLDER_NAME}/";

        private ConfigManager _configManager;

        private ITrackingService _trackingService;
        private RecordManager _recordManager;
        private MainViewViewModel _mainWindowViewModel;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SetupSettings();

            ObservableCollection<PsFile> recordCollection = new ObservableCollection<PsFile>();

            _configManager = new ConfigManager();
            _configManager.ConfigChanged += OnConfigChanged;

            _recordManager = new RecordManager(recordCollection);
            _trackingService = new ComTrackingService(recordCollection, new ProcessInfoService());
            SetTrackerSettings();

            _mainWindowViewModel = new MainViewViewModel(recordCollection, _trackingService, _recordManager);

            MainWindow = new MainView() { DataContext = _mainWindowViewModel };
            MainWindow.Show();
        }

        

        public static void DisplayErrorMessage(string message)
        {
            MessageBox.Show(message, APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            new CrashReportManager().CleanUpFolder();
            base.OnExit(e);
        }



        private void OnConfigChanged(object sender, EventArgs e)
        {
            SetTrackerSettings();
        }

        private void SetTrackerSettings()
        {
            _trackingService.CheckAFK = ConfigManager.Config.StopWhenAFK;
            _trackingService.OnlyCheckActiveProcess = ConfigManager.Config.TrackOnlyWhenWindowActive;
        }

        private static void SetupSettings()
        {
            Directory.CreateDirectory(APP_FOLDER_PATH); // Create folder for app files, if it does not exists already.
            
            // Increase tooltip delay
            ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(TOOLTIP_DELAY));
        }

        // Create crash-report and shutdown application on unhandled exception.
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var crashMessage = $"HResult: {e.Exception.HResult}\nError: {e.Exception}\n Inner: {e.Exception.InnerException}";

            new CrashReportManager().ReportCrash(crashMessage);

            if (ConfigManager.Config.DisplayErrorMessage)
                DisplayErrorMessage(crashMessage);

            this.Shutdown();
        }
    }
}
