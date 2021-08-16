namespace PSTimeTracker.Tracking
{
    public class TrackerConfiguration
    {
        public bool IgnoreAFK { get; set; }
        public int AFKTimeout { get; set; }
        public bool IgnoreActiveWindow { get; set; }
        public int ActiveWindowTimeout { get; set; }
        public int CallTimeoutMilliseconds { get; set; }

        public TrackerConfiguration(bool ignoreAFK, int afkTimeout, bool ignoreActiveWindow, int activeWindowTimeout, int callTimeoutMilliseconds)
        {
            this.IgnoreAFK = ignoreAFK;
            this.AFKTimeout = afkTimeout;
            this.IgnoreActiveWindow = ignoreActiveWindow;
            this.ActiveWindowTimeout = activeWindowTimeout;
            this.CallTimeoutMilliseconds = callTimeoutMilliseconds;
        }

        public TrackerConfiguration()
        {
            AFKTimeout = 10;
            ActiveWindowTimeout = 3;
            CallTimeoutMilliseconds = 100;
        }
    }



}
