using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using PSTimeTracker.Core;

namespace PSTimeTracker.UI
{
    public class RecordManager
    {
        /// <summary>
        /// If set to true, list will be written to the file that was restored.
        /// </summary>
        public bool SaveToLastLoadedFile { get; set; }

        /// <summary>
        /// How many records(files) will be kept. 1 - 200. Default: 6
        /// </summary>
        public int NumberOfRecordsToKeep
        {
            get => numberOfRecordsToKeep;
            set {
                if (value < 1 || value > 200)
                    throw new ArgumentOutOfRangeException();

                numberOfRecordsToKeep = value;
            }
        }
        int numberOfRecordsToKeep = 6;


        /// <summary>
        /// Delay in milliseconds between savings to file.
        /// </summary>
        public int WriteInterval { get; set; } = 3000;

        ObservableCollection<PsFile> _collectionToSave;
        FileInfo[] recordFiles;

        string lastRestoredFileName = "";

        public RecordManager(ObservableCollection<PsFile> collectionToSave)
        {
            _collectionToSave = collectionToSave;

            LoadFiles();
            RemoveExcessFiles();
        }

        /// <summary>
        /// Repeatedly saving to file with a <see cref="WriteInterval"/>.
        /// </summary>
        public async void StartSaving()
        {
            while (true)
            {
                Save();
                await Task.Delay(WriteInterval);
            }
        }

        /// <summary>
        /// Adds info about record files to recordFiles list.
        /// </summary>
        private void LoadFiles()
        {
            try
            {
                recordFiles = new DirectoryInfo(App.APP_RECORDS_PATH).GetFiles();
            }
            catch (Exception ex)
            {
                App.DisplayErrorMessage(ex.Message);
            }
        }

        private void RemoveExcessFiles()
        {
            while (recordFiles.Length > NumberOfRecordsToKeep)
            {
                var file = recordFiles.OrderBy(f => f.LastWriteTime).FirstOrDefault();
                LoadFiles();
                file.Delete();
            }
        }

        /// <summary>
        /// Writes collection to json file.
        /// </summary>
        public void Save()
        {
            if (_collectionToSave.Count < 1)
                return;


            string jsonContents = JsonSerializer.Serialize(_collectionToSave, new JsonSerializerOptions() { WriteIndented = true });

            try
            {
                if (SaveToLastLoadedFile && string.IsNullOrWhiteSpace(lastRestoredFileName) == false)
                    File.WriteAllText(lastRestoredFileName, jsonContents);
                else
                    File.WriteAllText(App.APP_RECORDS_PATH + "/" + App.SESSION_ID + ".json", jsonContents);
            }
            catch (Exception ex)
            {
                App.DisplayErrorMessage("Error saving record to file: " + ex.Message);
            }
        }

        public ObservableCollection<PsFile> LoadLast()
        {
            ObservableCollection<PsFile> newList = new ObservableCollection<PsFile>();

            var fileToRead = recordFiles.LastOrDefault();

            if (fileToRead == null)
                return newList;

            try
            {
                string jsonString = File.ReadAllText(fileToRead.FullName);
                newList = JsonSerializer.Deserialize<ObservableCollection<PsFile>>(jsonString);
                lastRestoredFileName = fileToRead.FullName;
            }
            catch (Exception ex)
            {
                App.DisplayErrorMessage("Cannot restore previous records: " + ex.Message);
            }

            return newList;
        }

    }
}
