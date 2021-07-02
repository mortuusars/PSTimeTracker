using System;
using System.Windows.Input;
using PSTimeTracker.Services;

namespace PSTimeTracker.ViewModels
{
    internal class ConfigViewModel
    {
        public Config Config { get; set; } = ConfigManager.Config;

        public ICommand SaveConfigCommand { get; }
    }
}
