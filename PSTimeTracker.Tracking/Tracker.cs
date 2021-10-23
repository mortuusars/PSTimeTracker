using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PSTimeTracker.Tracking
{
    public interface ITracker
    {
        event EventHandler<TrackingEventArgs>? TrackingTick;
        bool IsTracking { get; set; }
        Task TrackAsync();
    }

    public class Tracker : ITracker
    {
        public event EventHandler<TrackingEventArgs>? TrackingTick;

        public bool IsTracking { get; set; }

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

        private TrackingEventArgs TrackFile()
        {
            //if (Process.GetProcessesByName("photoshop").FirstOrDefault() is null)
            //    return TrackingEventArgs.NotRunning;

            PSGetNameResult result = GetFileNameInTime(100);
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
                    var titleResult = _photoshopTitle.GetActiveDocumentName();

                    if (titleResult.PSResponse == PSResponse.Success)
                    {
                        currentFile = titleResult.Filename;
                        trackResponse = TrackResponse.Success;
                    }
                    else
                    {
                        currentFile = _lastKnownFile;
                        trackResponse = TrackResponse.LastKnown;
                    }
                    break;
                default:
                    trackResponse = TrackResponse.Failed;
                    break;
            }

            //if (Config.IgnoreExtension)
            //    currentFile = currentFile[..currentFile.LastIndexOf('.')];

            _lastKnownFile = currentFile;

            return new TrackingEventArgs(currentFile, trackResponse);
        }

        //private TrackResponse SetCurrentFileAndResponse(ref string currentlyTrackedFile, PSGetNameResult result)
        //{
        //    switch (result.PSResponse)
        //    {
        //        case PSResponse.Success:
        //            currentlyTrackedFile = result.Filename;
        //            return TrackResponse.Success;
        //        case PSResponse.NoActiveDocument:
        //            _secondsWithoutDocuments++;
        //            return TrackResponse.NoActiveDocument;
        //        case PSResponse.PSNotRunning:
        //            return TrackResponse.PSNotRunning;
        //        case PSResponse.Busy:
        //        case PSResponse.Failed:
        //        case PSResponse.TimedOut:
        //            var titleResult = _photoshopTitle.GetActiveDocumentName();

        //            if (titleResult.PSResponse == PSResponse.Success)
        //            {
        //                currentlyTrackedFile = titleResult.Filename;
        //                return TrackResponse.Success;
        //            }
        //            else
        //            {
        //                currentlyTrackedFile = _lastKnownFile;
        //                return TrackResponse.LastKnown;
        //            }
        //        default:
        //            return TrackResponse.Failed;
        //    }
        //}

        //private bool IsUserAFK() => Config.IgnoreAFK ? false : LastInputInfo.IdleTime.TotalSeconds >= Config.AFKTimeout;

        //private bool IsWindowActive()
        //{
        //    if (Config.IgnoreActiveWindow)
        //        return true;

        //    _psInactiveTimeSeconds = ProcessUtils.IsWindowActive("photoshop") ? 0 : _psInactiveTimeSeconds++;
        //    return _psInactiveTimeSeconds <= Config.ActiveWindowTimeout;
        //}

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
