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

        public string ItemsCount { get => itemsCount; set { itemsCount = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemsCount))); } }
        public ObservableCollection<PsFileInfo> FilesList { get => _counter.Files; }

        public ICommand RemoveItemCommand { get; private set; }
        public ICommand AlwaysOnTopCommand { get; private set; }

        private string itemsCount;
        private int summarySeconds;
        private bool alwaysOnTop;

        private TimeCounter _counter;


        public MainWindowViewModel(TimeCounter counter) {
            _counter = counter;
            _counter.SummaryChanged += (_, seconds) => SummarySeconds = seconds;

            RemoveItemCommand = new RelayCommand(p => RemoveItem(p));
            AlwaysOnTopCommand = new RelayCommand(_ => AlwaysOnTopToggle());

            FilesList.CollectionChanged += (s, e) => SetFilesCountString();

            _counter.CalculateSummarySeconds();
            SetFilesCountString();
        }

        private void SetFilesCountString()
        {
            int filesCount = FilesList.Count;

            if (filesCount == 1)
                ItemsCount = filesCount + " file";
            else if (filesCount > 1)
                ItemsCount = filesCount + " files";
            else 
                ItemsCount = "";
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
    }
}