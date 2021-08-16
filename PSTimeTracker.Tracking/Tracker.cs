using PSTimeTracker.Tracking.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PSTimeTracker.Tracking
{
    public class Tracker
    {
        public event EventHandler? TrackingTick;

        public bool IsTracking { get; set; }
        public IList<TrackedFileInfo> TrackedFiles { get; set; }
        public long NoDocumentsTime { get; private set; }

        public TrackerConfiguration Config { get; set; }

        private readonly IPhotoshop _photoshopAPI;

        private TrackedFileInfo? _lastTrackedFile;
        private int _psInactiveTimeSeconds = 0;

        public Tracker(IPhotoshop photoshopAPI, TrackerConfiguration trackerConfiguration)
        {
            _photoshopAPI = photoshopAPI;
            Config = trackerConfiguration;

            TrackedFiles = new List<TrackedFileInfo>();
        }

        public async Task TrackAsync()
        {
            Stopwatch stopwatch = new();

            IsTracking = true;
            while (IsTracking)
            {
                stopwatch.Restart();

                if (!ProcessUtils.IsProcessRunning("photoshop"))
                    return;

                if (IsNotAFK() && IsWindowActive())
                    TrackFile();

                int delay = Math.Max(1000 - (int)stopwatch.ElapsedMilliseconds, 0);
                await Task.Delay(delay);
            }
        }

        public void Stop() => IsTracking = false;

        private void TrackFile()
        {
            TrackedFileInfo currentlyTrackedFile;
            var result = GetFileNameInTime(Config.CallTimeoutMilliseconds);

            switch (result.PSResponse)
            {
                case PSResponse.Success:
                    currentlyTrackedFile = GetOrCreateTrackedFile(result.Filename);
                    break;
                case PSResponse.PSNotRunning:
                    return;
                case PSResponse.NoActiveDocument:
                    NoDocumentsTime++;
                    return;
                case PSResponse.Busy:
                case PSResponse.Failed:
                case PSResponse.TimedOut:
                default:
                    if (_lastTrackedFile is null)
                        return;

                    currentlyTrackedFile = _lastTrackedFile;
                    break;
            }

            if (_lastTrackedFile is not null)
                _lastTrackedFile.IsCurrentlyActive = false;

            UpdateTrackedFile(ref currentlyTrackedFile);
            _lastTrackedFile = currentlyTrackedFile;

            TrackingTick?.Invoke(this, EventArgs.Empty);
        }

        private TrackedFileInfo GetOrCreateTrackedFile(string fileName)
        {
            if (_lastTrackedFile is not null && _lastTrackedFile.FileName == fileName)
                return _lastTrackedFile;

            TrackedFileInfo? currentlyOpenedFile = TrackedFiles.FirstOrDefault(f => f.FileName == fileName);

            if (currentlyOpenedFile is null)
            {
                currentlyOpenedFile = new TrackedFileInfo() { FileName = fileName, CreatedTime = DateTimeOffset.Now };
                TrackedFiles.Add(currentlyOpenedFile);
            }

            return currentlyOpenedFile;
        }

        private void UpdateTrackedFile(ref TrackedFileInfo currentlyTrackedFile)
        {
            currentlyTrackedFile.TrackedSeconds++;
            currentlyTrackedFile.IsCurrentlyActive = true;
            currentlyTrackedFile.LastActiveTime = DateTimeOffset.Now;
        }

        private bool IsNotAFK() => Config.IgnoreAFK || LastInputInfo.IdleTime.TotalSeconds < Config.AFKTimeout;

        private bool IsWindowActive()
        {
            if (Config.IgnoreActiveWindow)
                return true;

            _psInactiveTimeSeconds = ProcessUtils.IsWindowActive("photoshop") ? 0 : _psInactiveTimeSeconds++;
            return _psInactiveTimeSeconds <= Config.ActiveWindowTimeout;
        }

        private PSGetNameResult GetFileNameInTime(int timeoutMilliseconds)
        {
            var task = Task.Run(() => _photoshopAPI.GetActiveDocumentName());
            if (task.Wait(timeoutMilliseconds))
                return task.Result;
            else
                return new PSGetNameResult(PSResponse.TimedOut, string.Empty);
        }
    }
}
