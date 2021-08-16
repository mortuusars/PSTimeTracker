using System;

namespace PSTimeTracker.Tracking
{
    public class TrackedFileInfo
    {
        public string? FileName { get; set; }
        public int TrackedSeconds { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset LastActiveTime { get; set; }
        public bool IsCurrentlyActive { get; set; }
    }
}
