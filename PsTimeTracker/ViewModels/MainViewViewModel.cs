using System.Windows.Input;
using PSTimeTracker.Services;
using PropertyChanged;

namespace PSTimeTracker.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class MainViewViewModel
    {
        public ITrackingHandler TrackingHandler { get; }

        public bool IsRestoreAvailable { get; private set; }

        public ICommand OpenConfigCommand { get; }
        public ICommand OpenAboutCommand { get; }

        public ICommand RestoreSessionCommand { get; }

        private readonly ViewManager _viewManager;

        public MainViewViewModel(ITrackingHandler trackingHandler, ViewManager viewManager)
        {
            TrackingHandler = trackingHandler;

            _viewManager = viewManager;

            OpenConfigCommand = new RelayCommand(_ => _viewManager.ShowConfigView());
            OpenAboutCommand = new RelayCommand(_ => _viewManager.ShowAboutView());

            RestoreSessionCommand = new RelayCommand(_ => System.Console.WriteLine());

            TrackingHandler.StartTrackingAsync();
        }
    }
}