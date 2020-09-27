using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using PhotoshopTimeCounter.Commands;

namespace PhotoshopTimeCounter
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool AlwaysOnTop{ get => alwaysOnTop; set { alwaysOnTop = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AlwaysOnTop))); }}
        public int SummarySeconds { get => summarySeconds; set { summarySeconds = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SummarySeconds))); } }
        public ObservableCollection<PsFileInfo> FilesList { get => _counter.Files; }
        public ICommand RemoveItemCommand { get; private set; }
        public ICommand AlwaysOnTopCommand { get; private set; }


        private int summarySeconds;
        private bool alwaysOnTop;

        private TimeCounter _counter;

        public MainWindowViewModel(TimeCounter counter) {
            _counter = counter;
            _counter.SummaryChanged += (s, seconds) => SummarySeconds = seconds;

            RemoveItemCommand = new RelayCommand(p => RemoveItem(p));
            AlwaysOnTopCommand = new RelayCommand(p => AlwaysOnTopToggle());

            _counter.CalculateSummarySeconds();
        }

        private void RemoveItem(object parameter) {
            PsFileInfo item;

            try {
                item = (PsFileInfo)parameter;
            }
            catch (System.Exception) {
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