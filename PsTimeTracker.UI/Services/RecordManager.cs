using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PSTimeTracker.Core;

namespace PSTimeTracker.UI.Services
{
    public class RecordManager
    {
        #region Properties

        /// <summary>If set to true, list will be written to the file that was restored.</summary>
        public bool SaveToLastLoadedFile { get; set; }

        /// <summary>True if records are present and we can restore them.</summary>
        public bool IsRestoringAvailable { get; private set; }

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
        public int WriteInterval { get; set; } = 3000;

        #endregion

        private readonly ObservableCollection<PsFile> _collectionToSave;

        private bool shouldSave;
        private string lastRestoredFileName = "";

        /// <summary>Manages reading/writing of records to files.</summary>
        /// <param name="collectionToSave">Collection that will be saved to a file.</param>
        public RecordManager(ObservableCollection<PsFile> collectionToSave)
        {
            _collectionToSave = collectionToSave;

            Directory.CreateDirectory(App.APP_RECORDS_FOLDER_PATH);

            FileInfo[] files = GetOrderedRecordFiles();
            if (files.Length > 0) IsRestoringAvailable = true;
            RemoveExcessRecordFiles(files);
        }

        #region Public Methods

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
        public void StopSaving()
        {
            shouldSave = false;
        }

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
                    File.WriteAllText(App.APP_RECORDS_FOLDER_PATH + "/" + App.SESSION_ID + ".json", jsonContents);
            }
            catch (Exception ex)
            {
                App.DisplayErrorMessage("Error saving record to file: " + ex.Message);
            }
        }

        /// <summary>Load most recent record from file.</summary>
        public ObservableCollection<PsFile> LoadLastRecord()
        {
            ObservableCollection<PsFile> newList = new ObservableCollection<PsFile>();

            var fileToRead = GetOrderedRecordFiles().LastOrDefault();

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

        #endregion

        #region Private Methods

        /// <summary>Gets array of record files from a folder. Orders it by last written time - oldest first.</summary>
        private FileInfo[] GetOrderedRecordFiles()
        {
            FileInfo[] recordFiles = Array.Empty<FileInfo>();

            try
            {
                recordFiles = new DirectoryInfo(App.APP_RECORDS_FOLDER_PATH).GetFiles().OrderBy(f => f.LastWriteTime).ToArray();
            }
            catch (Exception ex)
            {
                App.DisplayErrorMessage(ex.Message);
            }

            return recordFiles;
        }

        /// <summary> Deletes oldest records. Stops when number of records is under allowed amount.</summary>
        private void RemoveExcessRecordFiles(FileInfo[] files)
        {
            while (files.Length > NumberOfRecordsToKeep)
            {
                var file = files.FirstOrDefault();
                file?.Delete();

                // Refresh files list
                GetOrderedRecordFiles();
            }
        }

        #endregion
    }
}
