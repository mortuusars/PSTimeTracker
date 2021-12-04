using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace PSTimeTracker.Tracking;

internal class PhotoshopTitle : IPhotoshop
{
    private TitleFilenameMatcher _filenameMatcher = new TitleFilenameMatcher();

    public TrackFileResult GetActiveDocumentName()
    {
        Process? psProc = Process.GetProcessesByName("photoshop").FirstOrDefault();

        if (psProc is null)
            return new TrackFileResult(Status.PSNotRunning, string.Empty);

        string filename = _filenameMatcher.GetFilename(psProc.MainWindowTitle);

        if (filename.Length > 0)
            return new TrackFileResult(Status.Success, filename);

        return new TrackFileResult(Status.Failed, filename);
    }
}

internal class TitleFilenameMatcher
{
    private static readonly Regex _regex = new(@".*\s+@\s+");

    /// <summary>
    /// Extracts filename from Photoshop title. 'Untitled-1 @ 45% (Layer 1, RGB/8)' => 'Untitled-1'
    /// </summary>
    /// <param name="title">Photoshop title.</param>
    /// <returns>Filename portion of a title.</returns>
    public string GetFilename(string title)
    {
        Match match = _regex.Match(title);
        return match.Success ? match.Value[0..^3] : string.Empty;
    }
}