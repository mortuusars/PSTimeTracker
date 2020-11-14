using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace PSTimeTracker.Services
{
    public static class ConfigManager
    {
        private const string CFG_FILENAME = "config.json";
        private static readonly string CFG_FILE_PATH = App.APP_FOLDER_PATH + CFG_FILENAME;

        public static Config Config;

        /// <summary>Loads config from file to <see cref="Config"/>. Loads default if something went wrong.</summary>
        public static void Load()
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
        public static void Save()
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

    public class Config : INotifyPropertyChanged
    {
        #pragma warning disable 0067
        public event PropertyChangedEventHandler PropertyChanged;
        #pragma warning restore 0067

        public int NumberOfRecordsToKeep { get; set; } = 6;
        public bool CheckAFK { get; set; } = true;
        public bool OnlyActiveWindow { get; set; } = true;
    }
}
