using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PSTimeTracker.Tracking
{
    public interface ITracker
    {
        event EventHandler<TrackingEventArgs>? TrackingTick;
        bool IsTracking { get; }
        Task TrackAsync();
        void Stop();
    }

    public class Tracker : ITracker
    {
        public event EventHandler<TrackingEventArgs>? TrackingTick;

        public bool IsTracking { get; private set; }

        private readonly IPhotoshop _photoshopCOM;
        private readonly IPhotoshop _photoshopTitle;

        private string _lastKnownFile = string.Empty;

        public Tracker()
        {
            _photoshopCOM = new PhotoshopCOM();
            _photoshopTitle = new PhotoshopTitle();
        }

        public async Task TrackAsync()
        {
            Stopwatch stopwatch = new();

            IsTracking = true;
            while (IsTracking)
            {
                stopwatch.Restart();

                TrackingEventArgs trackingEventArgs = TrackFile();
                TrackingTick?.Invoke(this, trackingEventArgs);

                int delay = Math.Max(1000 - (int)stopwatch.ElapsedMilliseconds, 0);
                await Task.Delay(delay);
            }
        }

        public void Stop()
        {
            IsTracking = false;
        }

        private TrackingEventArgs TrackFile()
        {
            if (Process.GetProcessesByName("photoshop").Length == 0)
                return TrackingEventArgs.NotRunning;

            PSFileNameResult result = GetFileName();
            string currentFile = result.Filename;
            TrackResponse trackResponse;

            switch (result.PSResponse)
            {
                case PSResponse.Success:
                    trackResponse = TrackResponse.Success;
                    break;
                case PSResponse.NoActiveDocument:
                    trackResponse = TrackResponse.NoActiveDocument;
                    break;
                case PSResponse.PSNotRunning:
                    trackResponse = TrackResponse.PSNotRunning;
                    break;
                case PSResponse.Busy:
                case PSResponse.Failed:
                case PSResponse.TimedOut:
                    currentFile = _lastKnownFile;
                    trackResponse = TrackResponse.LastKnown;
                    break;
                default:
                    trackResponse = TrackResponse.Failed;
                    break;
            }

            _lastKnownFile = currentFile;

            return new TrackingEventArgs(currentFile, trackResponse);
        }

        private PSFileNameResult GetFileName()
        {
            var comResult = GetComFileNameWithTimeout(100);

            if (comResult.PSResponse != PSResponse.TimedOut)
                return comResult;

            return _photoshopTitle.GetActiveDocumentName();
        }

        private PSFileNameResult GetComFileNameWithTimeout(int timeoutMilliseconds)
        {
            var task = Task.Run(() => _photoshopCOM.GetActiveDocumentName());
            if (task.Wait(timeoutMilliseconds))
                return task.Result;
            else
                return new PSFileNameResult(PSResponse.TimedOut, string.Empty);
        }
    }
}
