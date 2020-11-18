using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using PSTimeTracker.Services;
using PSTimeTracker.Core;
using PSTimeTracker.Models;
using System.Collections.Generic;
using PSTimeTracker.ViewModels;
using System.Linq;

namespace PSTimeTracker
{
    public class MainViewViewModel : INotifyPropertyChanged
    {
#pragma warning disable 0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 0067

        #region Properties

        public MenuViewModel MenuViewModel { get; }

        public int SummarySeconds { get; private set; }
        public ObservableCollection<PsFile> PsFilesList { get; }
        public bool ListIsEmpty { get; set; } = true;
        public bool CanRestorePreviousList { get; private set; }
        public bool MenuIsOpen { get; private set; }
        public string ItemsInfo { get; set; } = "PS Time Tracker";

        public ICommand RestoreAndStartCommand { get; }
        public ICommand StartWithoutRestoringCommand { get; }
        public ICommand SelectionChangedCommand { get; }
        public ICommand RemoveItemsCommand { get; }
        public ICommand ClearCommand { get; }

        public ICommand MenuCommand { get; }

        #endregion

        private int selectedItemsCount;

        private readonly ITrackingService _trackingService;
        private readonly RecordManager _recordManager;


        public MainViewViewModel(ObservableCollection<PsFile> psFilesList, ITrackingService trackingService, RecordManager recordManager, MenuViewModel menuViewModel)
        {
            PsFilesList = psFilesList;
            //PsFilesList.CollectionChanged += (s, e) => OnCollectionChanged();

            MenuViewModel = menuViewModel;

            _trackingService = trackingService;
            _trackingService.SummarySecondsChanged += (_, seconds) => UpdateInfo(seconds);

            _recordManager = recordManager;

            #region Commands

            RestoreAndStartCommand = new RelayCommand(_ => { Restore(); StartTracking(); });
            StartWithoutRestoringCommand = new RelayCommand(_ => StartTracking());

            SelectionChangedCommand = new RelayCommand(items => RefreshSelectedItemsInfo(items));
            RemoveItemsCommand = new RelayCommand(_ => RemoveSelectedItems());
            ClearCommand = new RelayCommand(_ => PsFilesList.Clear());

            MenuCommand = new RelayCommand(_ => MenuViewModel.IsMenuOpen = !MenuViewModel.IsMenuOpen);

            #endregion

            //SetFilesCountString();

            if (_recordManager.IsRestoringAvailable)
                CanRestorePreviousList = true;
            else
                StartTracking();
        }

        private void UpdateInfo(int seconds)
        {
            if (selectedItemsCount == 0)
                ItemsInfo = TimeFormatter.GetTimeStringFromSecods(seconds);
        }

        //private void OnCollectionChanged()
        //{
        //    //SetFilesCountString();
        //    ListIsEmpty = PsFilesList.Count < 1;
        //}

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

        private void RefreshSelectedItemsInfo(object selectedItems)
        {
            System.Collections.IList justList = (System.Collections.IList)selectedItems;
            var list = justList.Cast<PsFile>().ToList();

            if (list == null || list.Count < 1)
            {
                selectedItemsCount = 0;
            }
            else
            {
                int summary = 0;

                foreach (var item in list)
                {
                    if (item.IsSelected)
                        summary += item.TrackedSeconds;
                }

                var count = list.Count;

                ItemsInfo = $"Files: {count} | {TimeFormatter.GetTimeStringFromSecods(summary)}";
                selectedItemsCount = count;
            }
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