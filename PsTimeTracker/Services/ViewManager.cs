using System.Collections.ObjectModel;
using PSTimeTracker.Models;
using PSTimeTracker.ViewModels;
using PSTimeTracker.Views;
using PSTimeTracker.PsTracking;
using PSTimeTracker.Configuration;
using FileIO;
using System.Linq;

namespace PSTimeTracker.Services
{
    public class ViewManager
    {

        private ObservableCollection<PsFile> _FilesList;
        private readonly TrackingService _trackingService;
        private readonly RecordManager _recordManager;

        private MainView _mainView;
        private ConfigView _configView;
        private AboutView _aboutView;

        public ViewManager(ObservableCollection<PsFile> FilesList, TrackingService trackingService, RecordManager recordManager)
        {
            _FilesList = FilesList;
            _trackingService = trackingService;
            _recordManager = recordManager;
        }

        #region Main View

        private const string MAIN_WINDOW_STATE_FILENAME = "mainWindowState.";
        private readonly string MAIN_WINDOW_STATE_FILEPATH = App.APP_FOLDER_PATH + MAIN_WINDOW_STATE_FILENAME;

        public void ShowMainView()
        {
            MainViewViewModel mainWindowViewModel = new MainViewViewModel(ref _FilesList, this, _trackingService, _recordManager);
            mainWindowViewModel.AlwaysOnTop = ConfigManager.Config.AlwaysOnTop;
            mainWindowViewModel.CurrentSorting = ConfigManager.Config.SortBy;

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
            _mainView?.Close();
        }

        private void SaveMainViewState()
        {
            MainViewState mainViewState = new MainViewState()
            {
                Left = _mainView.Left,
                Top = _mainView.Top,
                Width = _mainView.ActualWidth
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
        }

        #endregion

        #region Config View

        public void ShowConfigView()
        {
            var alreadyOpenedWindow = App.Current.Windows.OfType<ConfigView>().FirstOrDefault();
            if (alreadyOpenedWindow != null)
                alreadyOpenedWindow.Activate();
            else
            {
                ConfigViewModel configViewModel = new ConfigViewModel();
                _configView = new ConfigView() { DataContext = configViewModel };
                _configView.Owner = _mainView;
                _configView.Show();
                _configView.Top -= 400;
            }
        }

        public void CloseConfigView()
        {
            _configView?.Close();
        }

        #endregion

        #region About View

        public void ShowAboutView()
        {
            var alreadyOpenedWindow = App.Current.Windows.OfType<AboutView>().FirstOrDefault();
            if (alreadyOpenedWindow != null)
                alreadyOpenedWindow.Activate();
            else
            {
                AboutViewModel aboutViewModel = new AboutViewModel();
                _aboutView = new AboutView() { DataContext = aboutViewModel };
                _aboutView.Owner = _mainView;
                _aboutView.Show();
                _aboutView.Top -= 400;
            }
        }

        public void CloseAboutView()
        {
            _aboutView?.Close();
        }

        #endregion
    }
}
