using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using PSTimeTracker.Services;
using PSTimeTracker.Core;
using PSTimeTracker.Models;

namespace PSTimeTracker
{
    public class MainViewViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties

        public int SummarySeconds { get; private set; }
        public string ItemsCount { get; private set; }
        public ObservableCollection<PsFile> PsFilesList { get; }
        public bool CanRestorePreviousList { get; private set; }

        public ICommand RestoreAndStartCommand { get; }
        public ICommand StartWithoutRestoringCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand ClearCommand { get; }

        #endregion

        private readonly CollectorService _collector;
        private readonly RecordManager _recordManager;

        public MainViewViewModel(ObservableCollection<PsFile> psFilesList, CollectorService collector, RecordManager recordManager)
        {
            PsFilesList = psFilesList;
            PsFilesList.CollectionChanged += (s, e) => SetFilesCountString();

            _collector = collector;
            _collector.SummarySecondsChanged += (_, seconds) => SummarySeconds = seconds;

            _recordManager = recordManager;

            RestoreAndStartCommand = new RelayCommand(_ => { Restore(); StartTracking(); });
            StartWithoutRestoringCommand = new RelayCommand(_ => StartTracking());

            RemoveItemCommand = new RelayCommand(p => RemoveItem(p));
            ClearCommand = new RelayCommand(_ => PsFilesList.Clear());

            SetFilesCountString();

            if (_recordManager.IsRestoringAvailable)
                CanRestorePreviousList = true;
            else
                StartTracking();
        }

        private void Restore()
        {
            PsFilesList.Clear();

            foreach (var file in _recordManager.LoadLastRecord())
            {
                PsFilesList.Add(file);
            }

            _recordManager.SaveToLastLoadedFile = true;
        }

        private void StartTracking()
        {
            _collector.StartCollecting();
            _recordManager.StartSaving();
        }

        private void SetFilesCountString()
        {
            int filesCount = PsFilesList.Count;

            if (filesCount == 1)
                ItemsCount = filesCount + " file";
            else if (filesCount > 1)
                ItemsCount = filesCount + " files";
            else
                ItemsCount = "";
        }

        private void RemoveItem(object parameter)
        {
            try
            {
                PsFilesList.Remove((PsFile)parameter);
            }
            catch (Exception ex)
            {
                App.DisplayErrorMessage("Error removing item from list: " + ex.Message);
            }
        }
    }
}