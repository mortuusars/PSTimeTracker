using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace PSTimeTracker.Tracking
{
    public class PhotoshopTitle : IPhotoshop
    {
        private static readonly Regex _regex = new(@".*\s+@\s+");

        public PSFileNameResult GetActiveDocumentName()
        {
            Process? psProc = Process.GetProcessesByName("photoshop").FirstOrDefault();

            if (psProc is null)
                return new PSFileNameResult(PSResponse.PSNotRunning, string.Empty);

            string filename = MatchFilename(psProc.MainWindowTitle);

            if (filename.Length > 0)
                return new PSFileNameResult(PSResponse.Success, filename);

            return new PSFileNameResult(PSResponse.Failed, filename);
        }

        private static string MatchFilename(string title)
        {
            Match match = _regex.Match(title);
            return match.Success ? match.Value[0..^3] : string.Empty;
        }
    }
}
