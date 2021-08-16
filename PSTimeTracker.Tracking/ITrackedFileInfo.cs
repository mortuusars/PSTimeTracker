using System;

namespace PSTimeTracker.Tracking
{
    public interface ITrackedFileInfo
    {
        public string FileName { get; }
        public int TrackedSeconds { get; set; }
        public DateTimeOffset FirstActiveTime { get; set; }
        public DateTimeOffset LastActiveTime { get; set; }
        public bool IsCurrentlyActive { get; set; }
    }
}
