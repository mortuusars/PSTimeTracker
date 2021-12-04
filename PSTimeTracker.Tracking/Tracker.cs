using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PSTimeTracker.Tracking
{
    public interface ITracker
    {
        /// <summary>
        /// Gets currenly active photoshop filename asynchronously.
        /// </summary>
        Task<TrackFileResult> TrackFilenameAsync();
    }

    public class Tracker : ITracker
    {
        private readonly IPhotoshop _photoshopCOM;
        private readonly IPhotoshop _photoshopTitle;

        /// <summary>
        /// Creates instance of a Tracker with specific interval.
        /// </summary>
        public Tracker()
        {
            _photoshopCOM = new PhotoshopCOM();
            _photoshopTitle = new PhotoshopTitle();
        }

        public Task<TrackFileResult> TrackFilenameAsync()
        {
            return Task.Run(() => GetFileNameResult());
        }

        private TrackFileResult GetFileNameResult()
        {
            if (Process.GetProcessesByName("photoshop").Length == 0)
                return TrackFileResult.NotRunning;

            var comResult = GetComFileNameWithTimeout(100);

            if (comResult.Status is not Status.TimedOut)
                return comResult;
            else
                return _photoshopTitle.GetActiveDocumentName();
        }

        private TrackFileResult GetComFileNameWithTimeout(int timeoutMilliseconds)
        {
            var task = Task.Run(() => _photoshopCOM.GetActiveDocumentName());
            if (task.Wait(timeoutMilliseconds))
                return task.Result;
            else
                return new TrackFileResult(Status.TimedOut, string.Empty);
        }
    }
}
