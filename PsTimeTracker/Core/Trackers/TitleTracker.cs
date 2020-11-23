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

        public string GetFileName()
        {
            string title = _processInfoService.GetPhotoshopWindowTitle();

            if (title == null)
                return null;

            return Regex.Match(title, @".*\s@").Value.Replace(" @", "");
        }
    }
}
