using System.Windows.Input;
using PSTimeTracker.Services;
using PropertyChanged;
using System.Diagnostics;
using System;

namespace PSTimeTracker.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class MainViewViewModel
    {
        public ITrackingHandler TrackingHandler { get; }

        public bool IsRestoreAvailable { get; private set; }

        public ICommand OpenConfigCommand { get; }
        public ICommand OpenAboutCommand { get; }

        public ICommand MergeCommand { get; set; }
        public object OnMergeCommand { get; private set; }

        //public ICommand RestoreSessionCommand { get; }

        private readonly ViewManager _viewManager;

        public MainViewViewModel(ITrackingHandler trackingHandler, ViewManager viewManager)
        {
            TrackingHandler = trackingHandler;

            _viewManager = viewManager;

            OpenConfigCommand = new RelayCommand(_ => _viewManager.ShowConfigView());
            OpenAboutCommand = new RelayCommand(_ => _viewManager.ShowAboutView());

            MergeCommand = new RelayCommand(item => MergeFiles(item));

            //RestoreSessionCommand = new RelayCommand(_ => System.Console.WriteLine());

            TrackingHandler.StartTrackingAsync();
        }

        private void MergeFiles(object item)
        {
            throw new NotImplementedException();
        }
    }
}