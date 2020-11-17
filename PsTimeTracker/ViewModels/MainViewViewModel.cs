using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using PSTimeTracker.Services;
using PSTimeTracker.Core;
using PSTimeTracker.Models;
using System.Collections.Generic;

namespace PSTimeTracker
{
    public class MainViewViewModel : INotifyPropertyChanged
    {
        #pragma warning disable 0067
        public event PropertyChangedEventHandler PropertyChanged;
        #pragma warning restore 0067

        #region Properties

        public int SummarySeconds { get; private set; }
        public string ItemsCount { get; private set; }
        public ObservableCollection<PsFile> PsFilesList { get; }
        public bool ListIsEmpty { get; set; } = true;
        public bool CanRestorePreviousList { get; private set; }
        public bool MenuIsOpen { get; private set; }
        public string SelectedItemsInfo { get; set; } = "PS Time Tracker";

        public ICommand RestoreAndStartCommand { get; }
        public ICommand StartWithoutRestoringCommand { get; }
        public ICommand SelectionChangedCommand { get; }
        public ICommand RemoveItemsCommand { get; }
        public ICommand ClearCommand { get; }

        public ICommand MenuCommand { get; }

        public ICommand TrackOnlyOnActiveCommand { get; }
        

        #endregion

        private readonly ITrackingService _trackingService;
        private readonly RecordManager _recordManager;

        public MainViewViewModel(ObservableCollection<PsFile> psFilesList, ITrackingService trackingService, RecordManager recordManager)
        {
            PsFilesList = psFilesList;
            PsFilesList.CollectionChanged += (s, e) => OnCollectionChanged();

            _trackingService = trackingService;
            _trackingService.SummarySecondsChanged += (_, seconds) => SummarySeconds = seconds;

            _recordManager = recordManager;

            #region Commands

            RestoreAndStartCommand = new RelayCommand(_ => { Restore(); StartTracking(); });
            StartWithoutRestoringCommand = new RelayCommand(_ => StartTracking());

            SelectionChangedCommand = new RelayCommand(_ => RefreshSelectedItemsInfo());
            RemoveItemsCommand = new RelayCommand(_ => RemoveSelectedItems());
            ClearCommand = new RelayCommand(_ => PsFilesList.Clear());

            TrackOnlyOnActiveCommand = new RelayCommand(_ => ConfigManager.Config.TrackOnlyWhenWindowActive = !ConfigManager.Config.TrackOnlyWhenWindowActive);
            MenuCommand = new RelayCommand(_ => MenuIsOpen = !MenuIsOpen);

            #endregion

            SetFilesCountString();

            if (_recordManager.IsRestoringAvailable)
                CanRestorePreviousList = true;
            else
                StartTracking();
        }

        private void OnCollectionChanged()
        {
            SetFilesCountString();
            ListIsEmpty = PsFilesList.Count < 1;
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
            _trackingService.StartTracking();
            _recordManager.StartSaving();
            CanRestorePreviousList = false;
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

        private void RefreshSelectedItemsInfo()
        {

            int itemsCount = 0;
            int summary = 0;

            foreach (var item in PsFilesList)
            {
                if (item.IsSelected)
                {
                    itemsCount++;
                    summary += item.TrackedSeconds;
                }
            }

            if (itemsCount > 0)
                SelectedItemsInfo = $"Files: {itemsCount} | {TimeFormatter.GetTimeStringFromSecods(summary)}";
            else
                SelectedItemsInfo = "PS Time Tracker";
        }

        private void RemoveSelectedItems()
        {
            for (int i = PsFilesList.Count - 1; i >= 0; i--)
            {
                if (PsFilesList[i].IsSelected)
                    PsFilesList.RemoveAt(i);
            }
        }
    }
}