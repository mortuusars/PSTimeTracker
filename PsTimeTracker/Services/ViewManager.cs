using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using PSTimeTracker.Core;
using PSTimeTracker.Models;
using PSTimeTracker.ViewModels;
using PSTimeTracker.Views;

namespace PSTimeTracker.Services
{
    internal class ViewManager : IViewManager
    {

        private readonly ObservableCollection<PsFile> _psFilesList;
        private readonly ITrackingService _trackingService;
        private readonly RecordManager _recordManager;

        private MainView _mainView;
        private ConfigView _configView;

        public ViewManager(ObservableCollection<PsFile> psFilesList, ITrackingService trackingService, RecordManager recordManager)
        {
            _psFilesList = psFilesList;
            _trackingService = trackingService;
            _recordManager = recordManager;
        }

        public void ShowMainView()
        {
            MenuViewModel menuViewModel = new MenuViewModel(this);
            MainViewViewModel mainWindowViewModel = new MainViewViewModel(_psFilesList, _trackingService, _recordManager, menuViewModel);

            _mainView = new MainView() { DataContext = mainWindowViewModel };
            _mainView.Show();
        }

        public void CloseMainView()
        {
            _mainView.Close();
            //TODO Handle saving state of mainview
        }


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
