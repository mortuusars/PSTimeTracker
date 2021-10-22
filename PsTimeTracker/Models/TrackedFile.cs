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

        public TrackedFile() { }

        public TrackedFile(string fileName)
        {
            FileName = fileName;
            FirstActiveTime = DateTimeOffset.Now;
            LastActiveTime = FirstActiveTime;
        }
    }
}
