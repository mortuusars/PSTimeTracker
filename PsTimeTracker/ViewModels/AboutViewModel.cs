using System;

namespace PSTimeTracker.ViewModels
{
    public class AboutViewModel
    {
        public string Version { get; set; }

        public AboutViewModel()
        {
            Version = BuildVersionString();
        }

        private static string BuildVersionString()
        {
            Version appVersion = App.Version;

            if (appVersion.Build == 0)
                return $"Version {appVersion.Major}.{appVersion.Minor}";
            else
                return $"Version {appVersion.Major}.{appVersion.Minor}.{appVersion.Build}";
        }
    }
}
