using System;

namespace PSTimeTracker.ViewModels
{
    public class AboutViewModel
    {
        public string Version { get; set; }

        public AboutViewModel()
        {
            SetupVersionText();
        }

        private void SetupVersionText()
        {
            Version appVersion = App.Version;

            if (appVersion.Build == 0)
                Version = $"Version {appVersion.Major}.{appVersion.Minor}";
            else
                Version = $"Version {appVersion.Major}.{appVersion.Minor}.{appVersion.Build}";
        }
    }
}
