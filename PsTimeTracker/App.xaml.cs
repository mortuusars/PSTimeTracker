﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using PSTimeTracker.Services;
using PSTimeTracker.Core;
using PSTimeTracker.Models;
using Photoshop;

namespace PSTimeTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
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

        ITrackingService _trackingService;
        RecordManager _recordManager;
        MainViewViewModel _mainWindowViewModel;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SetupSettings(); // Load config, and other things.

            ObservableCollection<PsFile> recordCollection = new ObservableCollection<PsFile>();

            _recordManager = new RecordManager(recordCollection);
            _trackingService = new ComTrackingService(recordCollection, new ProcessInfoService(), ConfigManager.Config);

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



        private static void SetupSettings()
        {
            Directory.CreateDirectory(APP_FOLDER_PATH); // Create folder for app files, if it does not exists already.

            ConfigManager.Load();
            ConfigManager.Config.PropertyChanged += (s, e) => ConfigManager.Save();
            
            // Increase tooltip delay
            ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(TOOLTIP_DELAY));
        }

        // Create crash-report and shutdown application on unhandled exception.
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            new CrashReportManager().ReportCrash(e.Exception.ToString());
            this.Shutdown();
        }
    }
}