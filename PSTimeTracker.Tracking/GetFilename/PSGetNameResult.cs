namespace PSTimeTracker.Tracking
{
    public record PSGetNameResult
    {
        public PSResponse PSResponse { get; init; }
        public string Filename { get; init; }

        public PSGetNameResult(PSResponse psResponse, string filename)
        {
            PSResponse = psResponse;
            Filename = filename;
        }
    }
}
