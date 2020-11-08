using System;
using System.ComponentModel;

namespace PSTimeTracker.Core
{
    public class PsFile : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string FileName { get; set; }
        public int TrackedSeconds { get; set; }
        public DateTimeOffset FirstActiveTime { get; set; }
        public DateTimeOffset LastActiveTime { get; set; }
        public bool IsCurrentlyActive { get; set; }
    }
}
