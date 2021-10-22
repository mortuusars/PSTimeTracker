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
using PSTimeTracker.Tracking;
using System.Linq;

namespace PSTimeTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string APP_NAME = "PSTimeTracker";
        public static Version Version { get; private set; } = new Version("1.3");
        public static readonly string APP_FOLDER_PATH = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/{APP_NAME}/";
        public static readonly string SESSION_ID = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

        private ConfigManager _configManager;

        private PsTracking.ITracker _tracker;
        private RecordManager _recordManager;

        private ITrackingHandler _trackingHandler;

        private ViewManager _viewManager;


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Directory.CreateDirectory(APP_FOLDER_PATH); // Create folder for app files, if it does not exists already.

            CreateObjectInstances();

            CheckForUpdatesAsync();

            _viewManager.ShowMainView();
        }

        private async void CheckForUpdatesAsync()
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
            _configManager = new ConfigManager();
            //_configManager.ConfigChanged += (s, e) => SetTrackerSettings();

            _trackingHandler = new TrackingHandler(new Tracker(new TrackerConfiguration()));
            _viewManager = new ViewManager(_trackingHandler);
        }


        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) => CrashManager.HandleCrash(e);
    }
}
