namespace PSTimeTracker.Tracking
{
    public record TrackedFileInfo
    {
        public string FileName { get; init; }

        public static TrackedFileInfo Empty { get => new TrackedFileInfo(string.Empty); }

        public TrackedFileInfo(string fileName) => FileName = fileName;
    }
}
