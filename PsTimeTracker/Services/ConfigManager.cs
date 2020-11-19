using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace PSTimeTracker.Services
{
    public class ConfigManager
    {
        private const string CFG_FILENAME = "config.json";
        private readonly string CFG_FILE_PATH = App.APP_FOLDER_PATH + CFG_FILENAME;

        public event EventHandler ConfigChanged;
        public static Config Config;

        public ConfigManager()
        {
            Load();
            Save();

            Config.PropertyChanged += Config_OnPropertyChanged;
        }

        private void Config_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ConfigChanged?.Invoke(this, EventArgs.Empty);
            Save();
        }

        /// <summary>Loads config from file to <see cref="Config"/>. Loads default if something went wrong.</summary>
        public void Load()
        {
            try
            {
                string configString = File.ReadAllText(CFG_FILE_PATH);
                Config = JsonSerializer.Deserialize<Config>(configString);
            }
            catch (Exception)
            {
                Config = new Config();
            }
        }

        /// <summary>Saves config to file.</summary>
        public void Save()
        {
            string jsonString = JsonSerializer.Serialize(Config, new JsonSerializerOptions() { WriteIndented = true });

            try
            {
                File.WriteAllText(CFG_FILE_PATH, jsonString);
            }
            catch (Exception ex)
            {
                App.DisplayErrorMessage("Error saving config file: " + ex.Message);
            }
        }
    }
}
