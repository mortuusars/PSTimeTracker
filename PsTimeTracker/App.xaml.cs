using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using PSTimeTracker.Services;
using PSTimeTracker.Core;
using PSTimeTracker.Models;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace PSTimeTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string APP_NAME = "PSTimeTracker";

        public static Version Version { get; private set; } = new Version("1.2.0");

        private const string RECORDS_FOLDER_NAME = "records";
        private const string CRASHES_FOLDER_NAME = "crash-reports";
        private const int TOOLTIP_DELAY = 500;

        public static readonly string SESSION_ID = new Random((int)DateTimeOffset.Now.ToUnixTimeSeconds()).Next().ToString();

        // Paths to Local/APPNAME and nested folders
        public static readonly string APP_FOLDER_PATH = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/{APP_NAME}/";
        public static readonly string APP_RECORDS_FOLDER_PATH = $"{APP_FOLDER_PATH}{RECORDS_FOLDER_NAME}/";
        public static readonly string APP_CRASHES_FOLDER_PATH = $"{APP_FOLDER_PATH}{CRASHES_FOLDER_NAME}/";

        private ConfigManager _configManager;

        private ProcessInfoService _processInfoService;

        private ITracker _tracker;
        private TrackingService _trackingService;
        private RecordManager _recordManager;

        private ViewManager _viewManager;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SetupSettings();

            ObservableCollection<PsFile> filesList = new ObservableCollection<PsFile>();

            _configManager = new ConfigManager();
            _configManager.ConfigChanged += OnConfigChanged;

            _processInfoService = new ProcessInfoService();

            _recordManager = new RecordManager(filesList);
            ChooseAndCreateTrackingMethod();
            _trackingService = new TrackingService(ref filesList, _processInfoService, _tracker);
            SetTrackerSettings();

            _viewManager = new ViewManager(filesList, _trackingService, _recordManager);
            _viewManager.ShowMainView();
        }

        private void ChooseAndCreateTrackingMethod()
        {
            if (ConfigManager.Config.UseLegacyTrackingMethod)
                _tracker = new TitleTracker(_processInfoService);
            else
                _tracker = new ComTracker();
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
            _trackingService.IgnoreAFK = ConfigManager.Config.IgnoreAFKTimer;
            _trackingService.IgnoreWindowState = ConfigManager.Config.IgnoreWindowState;
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
