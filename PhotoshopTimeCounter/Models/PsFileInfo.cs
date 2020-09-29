using System;
using System.ComponentModel;

namespace PhotoshopTimeCounter
{
    public class PsFileInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string FileName { get; set; }
        public int SecondsActive { get => secondsActive; set { secondsActive = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SecondsActive))); }}
        public DateTimeOffset FirstOpenTime { get; set; }
        public bool IsCurrentlyActive { get => isCurrentlyActive; set { isCurrentlyActive = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCurrentlyActive))); } }

        private int secondsActive;
        private bool isCurrentlyActive;
    }
}
