using PSTimeTracker.Services;
using System;
using System.Diagnostics;
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

        public bool KeepTrackingWhenWindowInactive { get => ignoreActiveWindow; set { ignoreActiveWindow = value; OnConfigPropertyChanged(); } }
        private bool ignoreActiveWindow;
        public bool IgnoreAFK { get => ignoreAFKTimer; set { ignoreAFKTimer = value; OnConfigPropertyChanged(); } }
        private bool ignoreAFKTimer;

        public bool IgnoreFileExtension { get => ignoreFileExtension; set { ignoreFileExtension = value; OnConfigPropertyChanged(); } }
        private bool ignoreFileExtension;

        public int MaxAFKTime { get => maxAFKTime; set { maxAFKTime = value; OnConfigPropertyChanged(); } }
        private int maxAFKTime;

        public int PsInactiveWindowTimeout { get => psActiveWindowTimeout; set { psActiveWindowTimeout = value; OnConfigPropertyChanged(); } }
        private int psActiveWindowTimeout;

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

                using (JsonDocument jdoc = JsonDocument.Parse(file))
                {
                    config.checkForUpdates = GetPropertyByName(jdoc, nameof(CheckForUpdates))?.GetBoolean() ?? config.checkForUpdates;
                    config.ignoreAFKTimer = GetPropertyByName(jdoc, nameof(IgnoreAFK))?.GetBoolean() ?? config.ignoreAFKTimer;
                    config.ignoreActiveWindow = GetPropertyByName(jdoc, nameof(KeepTrackingWhenWindowInactive))?.GetBoolean() ?? config.ignoreActiveWindow;
                    config.ignoreFileExtension = GetPropertyByName(jdoc, nameof(IgnoreFileExtension))?.GetBoolean() ?? config.IgnoreFileExtension;
                    config.maxAFKTime = GetPropertyByName(jdoc, nameof(MaxAFKTime))?.GetInt32() ?? config.maxAFKTime;
                    config.psActiveWindowTimeout = GetPropertyByName(jdoc, nameof(PsInactiveWindowTimeout))?.GetInt32() ?? config.psActiveWindowTimeout;
                }

                return config;
            }
            catch (Exception ex)
            {
                ViewManager.DisplayErrorMessage("Failed lo load config: " + ex.Message);
                config.Save();
                return config;
            }
        }

        private static JsonElement? GetPropertyByName(JsonDocument jsonDocument, string propertyName)
        {
            try
            {
                return jsonDocument.RootElement.GetProperty(propertyName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static Config CreateDefault()
        {
            Config config = new();
            config.checkForUpdates = true;
            config.ignoreActiveWindow = false;
            config.ignoreAFKTimer = false;
            config.ignoreFileExtension = false;
            config.maxAFKTime = 6;
            config.psActiveWindowTimeout = 3;

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
