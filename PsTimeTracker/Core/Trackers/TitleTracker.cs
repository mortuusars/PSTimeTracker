using System.IO;
using System.Text.RegularExpressions;

namespace PSTimeTracker.Core
{
    public class TitleTracker : ITracker
    {
        private ProcessInfoService _processInfoService;

        public TitleTracker(ProcessInfoService processInfoService)
        {
            _processInfoService = processInfoService;
        }

        public PSCallResult GetFileName()
        {
            string title = _processInfoService.GetPhotoshopWindowTitle();
            //Log("Title: " + title);

            if (title == null)
                return new PSCallResult(PSResponse.NoActiveDocument, string.Empty);


            // Match the first part of PS window name up to a @ sign.
            string filename = Regex.Match(title, @".*\s@").Value.Replace(" @", "");

            if (title == string.Empty)
                return new PSCallResult(PSResponse.Failed, string.Empty);

            //Log("FileName: " + title);
            return new PSCallResult(PSResponse.Success, filename);
        }

        private void Log(string message)
        {
            File.AppendAllText("log.txt", "\n" + message);
        }
    }
}
