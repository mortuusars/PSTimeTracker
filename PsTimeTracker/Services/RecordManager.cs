using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PSTimeTracker.Models;

namespace PSTimeTracker.Services
{
    public class RecordManager
    {
        private const string RECORDS_FOLDER_NAME = "records";
        private static readonly string APP_RECORDS_FOLDER_PATH = $"{App.APP_FOLDER_PATH}{RECORDS_FOLDER_NAME}/";

        /// <summary>If set to true, list will be written to the file that was restored.</summary>
        public bool SaveToLastLoadedFile { get; set; }

        /// <summary>True if records are present and we can restore them.</summary>
        public bool IsRestoringAvailable { get; }

        /// <summary>How many records(files) will be kept. 1 - 200. Default: 6 </summary>
        public int NumberOfRecordsToKeep
        {
            get => numberOfRecordsToKeep;
            set {
                if (value < 1 || value > 200)
                    throw new ArgumentOutOfRangeException();

                numberOfRecordsToKeep = value;
            }
        }
        private int numberOfRecordsToKeep = 6;

        /// <summary>Delay in milliseconds between savings to file.</summary>
        public int WriteInterval { get; set; } = 10000;

        private readonly ObservableCollection<TrackedFile> _collectionToSave;

        private bool shouldSave;
        private string lastRestoredFileName = "";

        /// <summary>Manages reading/writing of records to files.</summary>
        /// <param name="collectionToSave">Collection that will be saved to a file.</param>
        public RecordManager(ObservableCollection<TrackedFile> collectionToSave)
        {
            _collectionToSave = collectionToSave;

            Directory.CreateDirectory(APP_RECORDS_FOLDER_PATH);

            FileInfo[] files = GetOrderedRecordFiles();
            if (files.Length > 0) IsRestoringAvailable = true;
            RemoveOldestRecordsIfAboveLimit(files);
        }

        /// <summary>Repeatedly saving to file with a <see cref="WriteInterval"/>.</summary>
        public async void StartSaving()
        {
            shouldSave = true;

            while (shouldSave)
            {
                Save();
                await Task.Delay(WriteInterval);
            }
        }

        /// <summary>Stops repeated saving of a record collection.</summary>
        public void StopSaving() => shouldSave = false;

        /// <summary>Writes collection to json file.</summary>
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
                    File.WriteAllText(APP_RECORDS_FOLDER_PATH + "/" + App.SESSION_ID + ".json", jsonContents);
            }
            catch (Exception ex)
            {
                ViewManager.DisplayErrorMessage("Error saving record to file: " + ex.Message);
            }
        }

        /// <summary>Load most recent record from file.</summary>
        public ObservableCollection<TrackedFile> LoadLastRecord()
        {
            ObservableCollection<TrackedFile> newList = new ObservableCollection<TrackedFile>();

            var fileToRead = GetOrderedRecordFiles().LastOrDefault();

            if (fileToRead == null)
                return newList;

            try
            {
                string jsonString = File.ReadAllText(fileToRead.FullName);
                newList = JsonSerializer.Deserialize<ObservableCollection<TrackedFile>>(jsonString);
                lastRestoredFileName = fileToRead.FullName;
            }
            catch (Exception ex)
            {
                ViewManager.DisplayErrorMessage("Cannot restore previous records: " + ex.Message);
            }

            return CleanListProperties(newList);
        }

        /// <summary>Gets array of record files from a folder. Orders it by last written time - oldest first.</summary>
        private FileInfo[] GetOrderedRecordFiles()
        {
            FileInfo[] recordFiles = Array.Empty<FileInfo>();

            try
            {
                recordFiles = new DirectoryInfo(APP_RECORDS_FOLDER_PATH).GetFiles().OrderBy(f => f.LastWriteTime).ToArray();
            }
            catch (Exception ex)
            {
                ViewManager.DisplayErrorMessage(ex.Message);
            }

            return recordFiles;
        }

        private void RemoveOldestRecordsIfAboveLimit(FileInfo[] files)
        {
            while (files.Length > NumberOfRecordsToKeep)
            {
                var file = files.FirstOrDefault();
                file?.Delete();
                // Refresh files list
                files = GetOrderedRecordFiles();
            }
        }

        private ObservableCollection<TrackedFile> CleanListProperties(ObservableCollection<TrackedFile> listToClean)
        {
            //foreach (var item in listToClean)
            //{
            //    item.IsSelected = false;
            //    item.IsCurrentlyActive = false;
            //}

            return listToClean;
        }
    }
}
