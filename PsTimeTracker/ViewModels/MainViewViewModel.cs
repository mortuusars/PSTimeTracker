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
using System.Windows.Data;

namespace PSTimeTracker
{
    public class MainViewViewModel : INotifyPropertyChanged
    {
#pragma warning disable 0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 0067

        #region Properties

        public bool AlwaysOnTop { get; set; }

        public ICollectionView FilesCollectionView { get; }
        public ObservableCollection<PsFile> FilesList { get; set; }
        public int SummarySeconds { get; private set; }
        public bool ListIsEmpty { get; set; } = true;
        public bool CanRestorePreviousList { get; private set; }
        public bool IsMenuOpen { get; private set; }
        public string ItemsInfo { get; set; } = "PS Time Tracker";

        public Sorting CurrentSorting { get; set; }

        public ICommand PinCommand { get; }

        public ICommand RestoreAndStartCommand { get; }
        public ICommand StartWithoutRestoringCommand { get; }
        public ICommand SelectionChangedCommand { get; }
        public ICommand RemoveItemsCommand { get; }
        public ICommand ClearCommand { get; }

        public ICommand SortListCommand { get; }

        public ICommand MenuCommand { get; }
        public ICommand OpenPreviousCommand { get; }
        public ICommand OpenConfigCommand { get; }
        public ICommand OpenAboutCommand { get; }

        public ICommand MinimizeWindowCommand { get; }
        public ICommand CloseWindowCommand { get; }

        #endregion

        private int selectedItemsCount;

        private readonly IViewManager _viewManager;
        private readonly ITrackingService _trackingService;
        private readonly RecordManager _recordManager;


        public MainViewViewModel(ref ObservableCollection<PsFile> filesList, IViewManager viewManager, ITrackingService trackingService, RecordManager recordManager)
        {
            FilesList = filesList;
            FilesCollectionView = CollectionViewSource.GetDefaultView(FilesList);

            _viewManager = viewManager;
            _trackingService = trackingService;
            _trackingService.SummarySecondsChanged += (_, seconds) => UpdateInfo(seconds);

            _recordManager = recordManager;

            #region Commands

            PinCommand  = new RelayCommand(_ => Pin());

            RestoreAndStartCommand = new RelayCommand(_ => { Restore(); StartTracking(); });
            StartWithoutRestoringCommand = new RelayCommand(_ => StartTracking());

            SelectionChangedCommand = new RelayCommand(items => RefreshSelectedItemsInfo(items));
            RemoveItemsCommand = new RelayCommand(_ => RemoveSelectedItems());
            ClearCommand = new RelayCommand(_ => FilesList.Clear());

            SortListCommand = new RelayCommand(_ => SortList());

            MenuCommand = new RelayCommand(_ => IsMenuOpen = !IsMenuOpen);
            OpenConfigCommand = new RelayCommand(_ => OnOpenConfigCommand());

            MinimizeWindowCommand = new RelayCommand(_ => _viewManager.MinimizeMainView());
            CloseWindowCommand = new RelayCommand(_ => _viewManager.CloseMainView());

            #endregion

            if (_recordManager.IsRestoringAvailable)
                CanRestorePreviousList = true;
            else
                StartTracking();
        }

        private void OnOpenConfigCommand()
        {
            IsMenuOpen = false;
            _viewManager.ShowConfigView();
        }

        private void Pin()
        {
            AlwaysOnTop = !AlwaysOnTop;
        }

        private void SortList()
        {
            Sorting sortBy;
            if (CurrentSorting >= Enum.GetValues(typeof(Sorting)).Cast<Sorting>().Max())
                sortBy = (Sorting)0;
            else
            {
                sortBy = CurrentSorting + 1;
            }

            CurrentSorting = sortBy;

            FilesCollectionView.SortDescriptions.Clear();

            switch (CurrentSorting)
            {
                case Sorting.FirstAdded:
                    FilesCollectionView.SortDescriptions.Add(new SortDescription(nameof(PsFile.FirstActiveTime), ListSortDirection.Ascending));
                    break;
                case Sorting.LastAdded:
                    FilesCollectionView.SortDescriptions.Add(new SortDescription(nameof(PsFile.FirstActiveTime), ListSortDirection.Descending));
                    break;
                case Sorting.NameAZ:
                    FilesCollectionView.SortDescriptions.Add(new SortDescription("FileName", ListSortDirection.Ascending));
                    break;
                case Sorting.NameZA:
                    FilesCollectionView.SortDescriptions.Add(new SortDescription("FileName", ListSortDirection.Descending));
                    break;
                case Sorting.TimeShorter:
                    FilesCollectionView.SortDescriptions.Add(new SortDescription(nameof(PsFile.TrackedSeconds), ListSortDirection.Ascending));
                    break;
                case Sorting.TimeLonger:
                    FilesCollectionView.SortDescriptions.Add(new SortDescription(nameof(PsFile.TrackedSeconds), ListSortDirection.Descending));
                    break;
            }

            FilesCollectionView.Refresh();
        }

        private void UpdateInfo(int seconds)
        {
            if (selectedItemsCount == 0)
                ItemsInfo = TimeFormatter.GetTimeStringFromSecods(seconds);
        }

        private void Restore()
        {
            FilesList.Clear();

            foreach (var file in _recordManager.LoadLastRecord())
            {
                FilesList.Add(file);
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
            for (int i = FilesList.Count - 1; i >= 0; i--)
            {
                if (FilesList[i].IsSelected)
                    FilesList.RemoveAt(i);
            }
        }
    }
}