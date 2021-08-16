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
        public static Version Version { get; private set; } = new Version("1.2.3");
        public static readonly string APP_FOLDER_PATH = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/{APP_NAME}/";
        public static readonly string SESSION_ID = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

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

            CreateObjectInstances();

            CheckForUpdates();

            _viewManager.ShowMainView();
        }

        private async void CheckForUpdates()
        {
            if (ConfigManager.Config.CheckForUpdates)
            {
                var updateChecker = new UpdateChecker();
                if (await updateChecker.IsUpdateAvailable())
                    _viewManager.ShowUpdateView(updateChecker.NewVersionInfo);
            }
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


        //TODO: Move to crash report class
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
