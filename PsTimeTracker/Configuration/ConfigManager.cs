using PSTimeTracker.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace PSTimeTracker.Configuration
{
    public class ConfigManager
    {
        //private const string CFG_FILENAME = "config.json";
        //private readonly string CFG_FILE_PATH = Environment.CurrentDirectory + "/" + CFG_FILENAME;

        //public event EventHandler? ConfigChanged;
        //#pragma warning disable CS8618
        //public static Config Config { get; private set; }
        //#pragma warning restore CS8618

        //public ConfigManager()
        //{
        //    Config = Load();
        //    Save();

        //    Config.PropertyChanged += Config_OnPropertyChanged;
        //}

        //private void Config_OnPropertyChanged(object? sender, PropertyChangedEventArgs? e)
        //{
        //    ConfigChanged?.Invoke(this, EventArgs.Empty);
        //    Save();
        //}

        //public Config Load()
        //{
        //    try
        //    {
        //        string configString = File.ReadAllText(CFG_FILE_PATH);
        //        return JsonSerializer.Deserialize<Config>(configString);
        //    }
        //    catch (Exception)
        //    {
        //        return new Config();
        //    }
        //}

        //public void Save()
        //{
        //    string jsonString = JsonSerializer.Serialize(Config, new JsonSerializerOptions() { WriteIndented = true });

        //    try
        //    {
        //        File.WriteAllText(CFG_FILE_PATH, jsonString);
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewManager.DisplayErrorMessage("Error saving config file: " + ex.Message);
        //    }
        //}
    }
}
