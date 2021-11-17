using System;
using PropertyChanged;

namespace PSTimeTracker.Models
{
    [AddINotifyPropertyChangedInterface]
    public class TrackedFile
    {
        public string? FileName { get; set; }
        public int TrackedSeconds { get; set; }
        public DateTimeOffset AddedTime { get; set; }
        public DateTimeOffset LastActiveTime { get; set; }
        public bool IsCurrentlyActive { get; set; }

        public static TrackedFile Empty { get => new TrackedFile("") { 
            TrackedSeconds = -1, 
            AddedTime = DateTimeOffset.MinValue,
            LastActiveTime = DateTimeOffset.MinValue,
            IsCurrentlyActive = false }; 
        }

        public TrackedFile() 
        {
            FileName = "";
            AddedTime = DateTimeOffset.Now;
            LastActiveTime = DateTimeOffset.MinValue;
        }

        public TrackedFile(string fileName) : this() { FileName = fileName; }
    }
}
