namespace PSTimeTracker.Tracking
{
    public enum TrackResponse
    {
        Success,
        LastKnown,
        NoActiveDocument,
        PSNotRunning,
        Failed,
        PsNotActive,
        UserIsAFK
    }
}
