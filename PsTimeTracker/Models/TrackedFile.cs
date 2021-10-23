using System;
using PropertyChanged;

namespace PSTimeTracker.Models
{
    [AddINotifyPropertyChangedInterface]
    public class TrackedFile
    {
        public string? FileName { get; set; }
        public int TrackedSeconds { get; set; }
        public DateTimeOffset FirstActiveTime { get; set; }
        public DateTimeOffset LastActiveTime { get; set; }
        public bool IsCurrentlyActive { get; set; }

        public static TrackedFile Empty { get => new TrackedFile("") { 
            TrackedSeconds = -1, 
            FirstActiveTime = DateTimeOffset.MinValue,
            LastActiveTime = DateTimeOffset.MinValue,
            IsCurrentlyActive = false }; 
        }

        public TrackedFile() { }

        public TrackedFile(string fileName)
        {
            FileName = fileName;
            FirstActiveTime = DateTimeOffset.Now;
            LastActiveTime = FirstActiveTime;
        }
    }
}
