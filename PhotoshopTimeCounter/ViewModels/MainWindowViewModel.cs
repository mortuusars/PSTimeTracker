using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using PhotoshopTimeCounter.Commands;

namespace PhotoshopTimeCounter
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool AlwaysOnTop{ get => alwaysOnTop; set { alwaysOnTop = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AlwaysOnTop))); }}
        public int SummarySeconds { get => summarySeconds; set { summarySeconds = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SummarySeconds))); } }
        //public string CurrentSorting { get => currentSorting; set { currentSorting = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentSorting))); } }

        //public bool SortingPopupIsVisible { get => sortingPopupIsVisible; set { sortingPopupIsVisible = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SortingPopupIsVisible))); } }

        public ObservableCollection<PsFileInfo> FilesList { get => _counter.Files; }

        public ICommand RemoveItemCommand { get; private set; }
        public ICommand AlwaysOnTopCommand { get; private set; }
        //public ICommand SortListCommand { get; private set; }

        private int summarySeconds;
        private bool alwaysOnTop;
        //private bool sortingPopupIsVisible;
        //private string currentSorting = "";

        private TimeCounter _counter;
        //private SortingTypes previousSorting = 0;


        public MainWindowViewModel(TimeCounter counter) {
            _counter = counter;
            _counter.SummaryChanged += (s, seconds) => SummarySeconds = seconds;

            RemoveItemCommand = new RelayCommand(p => RemoveItem(p));
            AlwaysOnTopCommand = new RelayCommand(p => AlwaysOnTopToggle());
            //SortListCommand = new RelayCommand(p => SortList());

            _counter.CalculateSummarySeconds();
            //`_counter.Files.CollectionChanged += (s, e) => SortingPopupIsVisible = _counter.Files.Count == 0 ? false : true;
        }

        private void RemoveItem(object parameter) {
            PsFileInfo item;

            try {
                item = (PsFileInfo)parameter;
            }
            catch (Exception) {
                item = null;
            }

            FilesList.Remove(item);

            _counter.CalculateSummarySeconds();
        }

        private void AlwaysOnTopToggle() {
            AlwaysOnTop = !AlwaysOnTop;
        }

        /*
        private void SortList() {

            SortingTypes sortBy;

            if (previousSorting >= Enum.GetValues(typeof(SortingTypes)).Cast<SortingTypes>().Max())
                sortBy = (SortingTypes)0;
            else {
                sortBy = previousSorting + 1;
            }

            previousSorting = sortBy;

            _counter.SortList(sortBy);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilesList)));

            CurrentSorting = sortBy.ToString();
        }
        */
    }
}