using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using PSTimeTracker.Core;
using PSTimeTracker.Models;
using PSTimeTracker.ViewModels;
using PSTimeTracker.Views;
using FileIO;

namespace PSTimeTracker.Services
{
    internal class ViewManager : IViewManager
    {

        private ObservableCollection<PsFile> _FilesList;
        private readonly TrackingService _trackingService;
        private readonly RecordManager _recordManager;

        private MainView _mainView;
        private ConfigView _configView;

        public ViewManager(ObservableCollection<PsFile> FilesList, TrackingService trackingService, RecordManager recordManager)
        {
            _FilesList = FilesList;
            _trackingService = trackingService;
            _recordManager = recordManager;
        }

        #region MainView

        private const string MAIN_WINDOW_STATE_FILENAME = "mainWindowState.";
        private readonly string MAIN_WINDOW_STATE_FILEPATH = App.APP_FOLDER_PATH + MAIN_WINDOW_STATE_FILENAME;

        public void ShowMainView()
        {
            MainViewViewModel mainWindowViewModel = new MainViewViewModel(ref _FilesList, this, _trackingService, _recordManager);

            _mainView = new MainView() { DataContext = mainWindowViewModel };

            var state = LoadMainViewState();
            SetMainViewFromState(state);

            _mainView.Show();
        }

        public void MinimizeMainView()
        {
            _mainView.WindowState = System.Windows.WindowState.Minimized;
        }

        public void CloseMainView()
        {
            SaveMainViewState();
            _mainView.Close();
        }

        private void SaveMainViewState()
        {
            MainViewState mainViewState = new MainViewState()
            {
                Left = _mainView.Left,
                Top = _mainView.Top,
                Width = _mainView.ActualWidth,
                Height = _mainView.ActualHeight,
                AlwaysOnTop = _mainView.Topmost,
            };

            JsonManager.SerializeAndWrite(mainViewState, MAIN_WINDOW_STATE_FILEPATH);
        }

        private MainViewState LoadMainViewState()
        {
            var mainViewState = JsonManager.ReadAndDeserialize<MainViewState>(MAIN_WINDOW_STATE_FILEPATH);
            return mainViewState ?? new MainViewState();
        }

        private void SetMainViewFromState(MainViewState state)
        {
            _mainView.Left = state.Left;
            _mainView.Top = state.Top;
            _mainView.Width = state.Width;
            //_mainView.Height = state.Height;
        }

        #endregion

        public void ShowConfigView()
        {
            ConfigViewModel configViewModel = new ConfigViewModel(this);
            _configView = new ConfigView() { DataContext = configViewModel };
            _configView.Show();
        }

        public void CloseConfigView()
        {
            _configView.Close();
        }
    }
}
