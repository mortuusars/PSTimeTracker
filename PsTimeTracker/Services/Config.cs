using System.ComponentModel;

namespace PSTimeTracker.Services
{
    public class Config : INotifyPropertyChanged
    {
        #pragma warning disable 0067
        public event PropertyChangedEventHandler PropertyChanged;
        #pragma warning restore 0067

        public bool DisplayErrorMessage { get; set; } = true;
        public int NumberOfRecordsToKeep { get; set; } = 6;
        public bool StopWhenAFK { get; set; } = true;
        public bool TrackOnlyWhenWindowActive { get; set; } = true;
    }
}
