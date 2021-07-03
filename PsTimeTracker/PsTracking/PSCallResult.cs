namespace PSTimeTracker.PsTracking
{
    public record PSCallResult
    {
        public PSResponse PSResponse { get; init; }
        public string Filename { get; init; }

        public PSCallResult(PSResponse psResponse, string filename)
        {
            PSResponse = psResponse;
            Filename = filename;
        }
    }
}
