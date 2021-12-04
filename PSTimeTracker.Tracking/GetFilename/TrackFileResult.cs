namespace PSTimeTracker.Tracking;

/// <summary>
/// Describes Result of a call to photoshop to get filename.
/// </summary>
public record TrackFileResult
{
    /// <summary>
    /// Status of a call.
    /// </summary>
    public Status Status { get; init; }

    /// <summary>
    /// Filename that was retrieved.
    /// </summary>
    public string Filename { get; init; }

    public static readonly TrackFileResult NotRunning = new (Status.PSNotRunning, string.Empty);
    public static readonly TrackFileResult Failed = new (Status.Failed, string.Empty);

    /// <summary>
    /// Creates an instance of a TrackFileResult with Status and Filename.
    /// </summary>
    /// <param name="status">Status of a call.</param>
    /// <param name="filename">Filename that was retrieved.</param>
    public TrackFileResult(Status status, string filename)
    {
        Status = status;
        Filename = filename;
    }

    /// <summary>
    /// Creates an instance of a TrackFileResult with Status and empty Filename.
    /// </summary>
    /// <param name="status">Status of a call.</param>
    public TrackFileResult(Status status)
    {
        Status = status;
        Filename = string.Empty;
    }
}