namespace PSTimeTracker.Tracking
{
    public record PSFileNameResult
    {
        public PSResponse PSResponse { get; init; }
        public string Filename { get; init; }

        public PSFileNameResult(PSResponse psResponse, string filename)
        {
            PSResponse = psResponse;
            Filename = filename;
        }
    }
}
