﻿using System;
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
        #pragma warning disable 0067
        public event PropertyChangedEventHandler PropertyChanged;
        #pragma warning restore 0067

        #region Properties

        public int SummarySeconds { get; private set; }
        public string ItemsCount { get; private set; }
        public ObservableCollection<PsFile> PsFilesList { get; }
        public bool ListIsEmpty { get; set; } = true;
        public bool CanRestorePreviousList { get; private set; }

        public ICommand RestoreAndStartCommand { get; }
        public ICommand StartWithoutRestoringCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand ClearCommand { get; }

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

            RemoveItemCommand = new RelayCommand(p => RemoveItem(p));
            ClearCommand = new RelayCommand(_ => PsFilesList.Clear());

            TrackOnlyOnActiveCommand = new RelayCommand(_ => ConfigManager.Config.OnlyActiveWindow = !ConfigManager.Config.OnlyActiveWindow);

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