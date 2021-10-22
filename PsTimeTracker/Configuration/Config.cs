using PSTimeTracker.Services;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace PSTimeTracker.Configuration
{
    public class Config
    {
        public event Action<string>? ConfigChanged;

        public bool CheckForUpdates { get => checkForUpdates; set { checkForUpdates = value; OnConfigPropertyChanged(); } }
        private bool checkForUpdates;

        public bool IgnoreWindowState { get => ignoreWindowState; set { ignoreWindowState = value; OnConfigPropertyChanged(); } }
        private bool ignoreWindowState;
        public bool IgnoreAFKTimer { get => ignoreAFKTimer; set { ignoreAFKTimer = value; OnConfigPropertyChanged(); } }
        private bool ignoreAFKTimer;

        public bool IgnoreFileExtension { get => ignoreFileExtension; set { ignoreFileExtension = value; OnConfigPropertyChanged(); } }
        private bool ignoreFileExtension;

        private static string _cfgFileName;

        static Config()
        {
            _cfgFileName = "";
        }

        private Config() { }

        public static Config Load(string fileName)
        {
            _cfgFileName = fileName;

            Config config = CreateDefault();

            try
            {
                string file = File.ReadAllText(_cfgFileName);

                var doc = JsonDocument.Parse(file);
                config.checkForUpdates = doc.RootElement.GetProperty(nameof(CheckForUpdates)).GetBoolean();
                config.ignoreAFKTimer = doc.RootElement.GetProperty(nameof(IgnoreAFKTimer)).GetBoolean();
                config.ignoreWindowState = doc.RootElement.GetProperty(nameof(IgnoreWindowState)).GetBoolean();
                config.ignoreFileExtension = doc.RootElement.GetProperty(nameof(IgnoreFileExtension)).GetBoolean();

                return config;
            }
            catch (Exception ex)
            {
                ViewManager.DisplayErrorMessage("Failed lo load config: " + ex.Message);
                return CreateDefault();
            }
        }

        private static Config CreateDefault()
        {
            Config config = new();
            config.checkForUpdates = true;
            config.ignoreWindowState = false;
            config.ignoreAFKTimer = false;
            config.ignoreFileExtension = false;

            return config;
        }

        private void OnConfigPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            ConfigChanged?.Invoke(nameof(propertyName));
            Save();
        }

        private void Save()
        {
            try
            {
                string? dir = Path.GetDirectoryName(_cfgFileName);
                if (dir is null) return;
                Directory.CreateDirectory(dir);

                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true });
                File.WriteAllText(_cfgFileName, json);
            }
            catch (Exception ex)
            {
                ViewManager.DisplayErrorMessage("Failed to save config to " + _cfgFileName + ":\n\n" + ex.Message);
            }
        }
    }
}
