using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using PSTimeTracker.Services;
using PSTimeTracker.Models;
using PSTimeTracker.PsTracking;
using PSTimeTracker.Configuration;
using PSTimeTracker.Views;
using PSTimeTracker.ViewModels;
using PSTimeTracker.Update;

namespace PSTimeTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string APP_NAME = "PSTimeTracker";

        public static Version Version { get; private set; } = new Version("1.2.1");

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

        public static void DisplayErrorMessage(string message)
        {
            MessageBox.Show(message, APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Directory.CreateDirectory(APP_FOLDER_PATH); // Create folder for app files, if it does not exists already.
            ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(TOOLTIP_DELAY));

            CreateObjectInstances();

            if (ConfigManager.Config.CheckForUpdates)
                CheckUpdates();

            _viewManager.ShowMainView();
        }

        private async void CheckUpdates()
        {
            var update = await new Update.UpdateChecker().CheckAsync();
            if (update.updateAvailable)
            {
                ShowUpdateView(update.versionInfo);
            }
        }

        private void ShowUpdateView(VersionInfo versionInfo)
        {
            UpdateView updateView = new UpdateView()
            {
                DataContext = new UpdateViewModel()
                {
                    VersionText = $"Version: {versionInfo.Version}",
                    Description = versionInfo.Description
                }
            };
            updateView.Show();
        }

        private void CreateObjectInstances()
        {
            ObservableCollection<PsFile> filesList = new ObservableCollection<PsFile>();

            _configManager = new ConfigManager();
            _configManager.ConfigChanged += (s, e) => SetTrackerSettings();

            _processInfoService = new ProcessInfoService();

            _recordManager = new RecordManager(filesList);
            ChooseAndCreateTrackingMethod();
            _trackingService = new TrackingService(ref filesList, _processInfoService, _tracker);
            SetTrackerSettings();

            _viewManager = new ViewManager(filesList, _trackingService, _recordManager);
        }

        private void ChooseAndCreateTrackingMethod()
        {
            if (ConfigManager.Config.UseLegacyTrackingMethod)
                _tracker = new TitleTracker(_processInfoService);
            else
                _tracker = new ComTracker(_processInfoService);
        }

        private void SetTrackerSettings()
        {
            _trackingService.IgnoreAFK = ConfigManager.Config.IgnoreAFKTimer;
            _trackingService.IgnoreWindowState = ConfigManager.Config.IgnoreWindowState;
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

        protected override void OnExit(ExitEventArgs e)
        {
            new CrashReportManager().CleanUpFolder();
            base.OnExit(e);
        }
    }
}
