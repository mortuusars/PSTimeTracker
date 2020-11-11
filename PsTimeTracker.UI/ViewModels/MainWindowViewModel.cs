using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using PSTimeTracker.Core;

namespace PSTimeTracker.UI
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int SummarySeconds { get; private set; } = 0;
        public string ItemsCount { get; private set; }
        public ObservableCollection<PsFile> PsFilesList { get; private set; }
        public bool CanRestorePreviousList { get; set; }

        public ICommand RemoveItemCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand RestoreAndStartCommand { get; }
        public ICommand StartWithoutRestoringCommand { get; }



        private CollectorService _collector;
        private RecordManager _recordManager;

        ObservableCollection<PsFile> restoredCollection;


        public MainWindowViewModel(ObservableCollection<PsFile> collection, CollectorService collector, RecordManager recordManager)
        {
            PsFilesList = collection;

            _collector = collector;
            _collector.SummarySecondsChanged += (_, seconds) => SummarySeconds = seconds;

            _recordManager = recordManager;

            RemoveItemCommand = new RelayCommand(p => RemoveItem(p));
            ClearCommand = new RelayCommand(_ => PsFilesList.Clear());

            RestoreAndStartCommand = new RelayCommand(_ => { Restore(); StartTracking();});
            StartWithoutRestoringCommand = new RelayCommand(_ => StartTracking());

            PsFilesList.CollectionChanged += (s, e) => SetFilesCountString();

            SetFilesCountString();


            restoredCollection = _recordManager.LoadLastRecord();

            if (restoredCollection.Count > 0)
                CanRestorePreviousList = true;
            else 
                StartTracking();
        }

        private void Restore()
        {
            PsFilesList.Clear();

            foreach (var file in restoredCollection)
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