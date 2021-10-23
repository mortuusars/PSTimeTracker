namespace PSTimeTracker.Tracking
{
    public class TrackingEventArgs
    {
        public string TrackedFileName { get; set; }
        public TrackResponse TrackResponse { get; set; }

        public static TrackingEventArgs Failed => new TrackingEventArgs(string.Empty, TrackResponse.Failed);
        public static TrackingEventArgs NotRunning => new TrackingEventArgs(string.Empty, TrackResponse.PSNotRunning);

        public TrackingEventArgs(string fileName, TrackResponse response)
        {
            TrackedFileName = fileName;
            TrackResponse = response;
        }
    }
}
