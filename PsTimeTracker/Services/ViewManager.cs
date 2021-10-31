using PSTimeTracker.ViewModels;
using PSTimeTracker.Views;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PSTimeTracker.Update;

namespace PSTimeTracker.Services
{
    public class ViewManager
    {
        private readonly ITrackingHandler _trackingHandler;

        private MainView _mainView;
        private ConfigView? _configView;
        private AboutView? _aboutView;

        public static void DisplayErrorMessage(string message) => MessageBox.Show(message, App.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);

        public ViewManager(ITrackingHandler trackingHandler)
        {
            _trackingHandler = trackingHandler;

            _mainView = new MainView() { DataContext = new MainViewViewModel(_trackingHandler, this) };

            SetTooltipDelay(500);
        }

        public static T? GetFirstCreatedWindowOfType<T>() where T : Window
        {
            return App.Current.Windows.OfType<T>().FirstOrDefault();
        }

        public static bool ActivateFirstWindowOfType<T>() where T : Window
        {
            var window = GetFirstCreatedWindowOfType<T>();

            if (window is T)
                return window.Activate();
            else
                return false;
        }

        public void ShowUpdateView(VersionInfo versionInfo)
        {
            new UpdateView()
            {
                DataContext = new UpdateViewModel($"Version: {versionInfo.Version}", versionInfo.Description)
            }.Show();
        }

        public void ShowMainView() => _mainView.Show();

        public void ShowConfigView()
        {
            if (!ActivateFirstWindowOfType<ConfigView>())
            {
                ConfigViewModel configViewModel = new ConfigViewModel();
                _configView = new ConfigView() { DataContext = configViewModel };
                _configView.Owner = _mainView;
                _configView.Show();
                _configView.Top += 200;
            }
        }

        public void ShowAboutView()
        {
            if (!ActivateFirstWindowOfType<AboutView>())
            {
                AboutViewModel aboutViewModel = new AboutViewModel();
                _aboutView = new AboutView() { DataContext = aboutViewModel };
                _aboutView.Owner = _mainView;
                _aboutView.Show();
            }
        }

        private static void SetTooltipDelay(int delayMS) =>
            ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(delayMS));
    }
}
