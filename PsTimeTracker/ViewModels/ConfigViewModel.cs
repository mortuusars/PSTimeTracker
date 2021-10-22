using PSTimeTracker.Configuration;

namespace PSTimeTracker.ViewModels
{
    internal class ConfigViewModel
    {
        public Config Config { get; } = ConfigManager.Config;
    }
}
