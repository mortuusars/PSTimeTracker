using PSTimeTracker.Tracking.Utils;
using System.Text.RegularExpressions;

namespace PSTimeTracker.Tracking
{
    public class PhotoshopTitle
    {
        private static readonly Regex _regex = new(@".*\s+@\s+");

        public PSGetNameResult GetActiveDocumentName()
        {
            string filename = MatchFilename(ProcessUtils.GetWindowTitle("photoshop"));

            if (filename.Length > 0)
                return new PSGetNameResult(PSResponse.Success, filename);

            return new PSGetNameResult(PSResponse.Failed, filename);
        }

        private static string MatchFilename(string title)
        {
            Match match = _regex.Match(title);
            return match.Success ? match.Value[0..^3] : string.Empty;
        }
    }
}
