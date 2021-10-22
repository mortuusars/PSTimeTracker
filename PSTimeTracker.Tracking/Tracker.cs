using PSTimeTracker.Tracking.Utils;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PSTimeTracker.Tracking
{
    public interface ITracker
    {
        event EventHandler<TrackingEventArgs>? TrackingTick;

        TrackerConfiguration Config { get; set; }
        bool IsTracking { get; set; }

        void StopTracking();
        Task TrackAsync();
    }

    public class TrackingEventArgs
    {
        public TrackedFileInfo TrackedFile { get; set; }
        public TrackResponse TrackResponse { get; set; }
        public long TimeWithoutDocuments { get; set; }

        public TrackingEventArgs() => TrackedFile = new(string.Empty);
    }

    public class Tracker : ITracker
    {
        public event EventHandler<TrackingEventArgs>? TrackingTick;

        public bool IsTracking { get; set; }
        public TrackerConfiguration Config { get; set; }

        private readonly PhotoshopCOM _photoshopCOM;
        private readonly PhotoshopTitle _photoshopTitle;

        private TrackedFileInfo _lastKnownFile = new(string.Empty);

        private long _secondsWithoutDocuments;
        private int _psInactiveTimeSeconds;

        public Tracker(TrackerConfiguration trackerConfiguration)
        {
            Config = trackerConfiguration;

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

                TrackingEventArgs trackingEventArgs;

                if (!ProcessUtils.IsProcessRunning("photoshop"))
                    trackingEventArgs = CreateTrackingArgs(TrackedFileInfo.Empty, TrackResponse.PSNotRunning, _secondsWithoutDocuments);
                else if (IsUserAFK())
                    trackingEventArgs = CreateTrackingArgs(TrackedFileInfo.Empty, TrackResponse.UserIsAFK, _secondsWithoutDocuments);
                else if (!IsWindowActive())
                    trackingEventArgs = CreateTrackingArgs(TrackedFileInfo.Empty, TrackResponse.PsNotActive, _secondsWithoutDocuments);
                else
                    trackingEventArgs = TrackFile();

                TrackingTick?.Invoke(this, trackingEventArgs);

                int delay = Math.Max(1000 - (int)stopwatch.ElapsedMilliseconds, 0);
                await Task.Delay(delay);
            }
        }

        public void StopTracking() => IsTracking = false;

        private TrackingEventArgs TrackFile()
        {
            TrackedFileInfo currentlyTrackedFile = TrackedFileInfo.Empty;
            PSGetNameResult result = GetFileNameInTime(Config.CallTimeoutMilliseconds);

            TrackResponse trackResponse = SetCurrentFileAndResponse(ref currentlyTrackedFile, result);

            _lastKnownFile = currentlyTrackedFile;

            return CreateTrackingArgs(currentlyTrackedFile, trackResponse, _secondsWithoutDocuments);
        }

        private TrackResponse SetCurrentFileAndResponse(ref TrackedFileInfo currentlyTrackedFile, PSGetNameResult result)
        {
            switch (result.PSResponse)
            {
                case PSResponse.Success:
                    currentlyTrackedFile = new TrackedFileInfo(result.Filename);
                    return TrackResponse.Success;
                case PSResponse.NoActiveDocument:
                    _secondsWithoutDocuments++;
                    return TrackResponse.NoActiveDocument;
                case PSResponse.PSNotRunning:
                    return TrackResponse.PSNotRunning;
                case PSResponse.Busy:
                case PSResponse.Failed:
                case PSResponse.TimedOut:
                    var titleResult = _photoshopTitle.GetActiveDocumentName();

                    currentlyTrackedFile = titleResult.PSResponse == PSResponse.Success ?
                        new TrackedFileInfo(titleResult.Filename) : _lastKnownFile;

                    return TrackResponse.LastKnown;
                default:
                    return TrackResponse.Failed;
            }
        }

        private bool IsUserAFK() => Config.IgnoreAFK ? false : LastInputInfo.IdleTime.TotalSeconds >= Config.AFKTimeout;

        private bool IsWindowActive()
        {
            if (Config.IgnoreActiveWindow)
                return true;

            _psInactiveTimeSeconds = ProcessUtils.IsWindowActive("photoshop") ? 0 : _psInactiveTimeSeconds++;
            return _psInactiveTimeSeconds <= Config.ActiveWindowTimeout;
        }

        private TrackingEventArgs CreateTrackingArgs(TrackedFileInfo trackedFile, TrackResponse response, long secondsWithoutDocuments)
        {
            return new TrackingEventArgs()
            {
                TrackedFile = trackedFile,
                TrackResponse = response,
                TimeWithoutDocuments = secondsWithoutDocuments
            };
        }

        private PSGetNameResult GetFileNameInTime(int timeoutMilliseconds)
        {
            var task = Task.Run(() => _photoshopCOM.GetActiveDocumentName());
            if (task.Wait(timeoutMilliseconds))
                return task.Result;
            else
                return new PSGetNameResult(PSResponse.TimedOut, string.Empty);
        }
    }
}
