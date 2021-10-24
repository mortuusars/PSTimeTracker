using PropertyChanged;
using PSTimeTracker.Configuration;
using PSTimeTracker.Models;
using PSTimeTracker.Tracking;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace PSTimeTracker
{
    public interface ITrackingHandler
    {
        ObservableCollection<TrackedFile> TrackedFiles { get; }
        long SummarySeconds { get; }

        void RemoveFiles();
        void StartTrackingAsync();
    }

    [AddINotifyPropertyChangedInterface]
    public class TrackingHandler : ITrackingHandler
    {
        public ObservableCollection<TrackedFile> TrackedFiles { get; private set; }
        public long SummarySeconds { get; private set; }
        public TrackingStatus Status { get; private set; }

        private readonly Config _config;
        private readonly ITracker _tracker;

        private TrackedFile _lastKnownFile;
        private int _psInactiveTime;

        public TrackingHandler(Config config)
        {
            _config = config;
            _tracker = new Tracker();
            _tracker.TrackingTick += OnTrackingTick;

            _lastKnownFile = TrackedFile.Empty;
            _psInactiveTime = _config.PsActiveWindowTimeout + 1;
            TrackedFiles = new ObservableCollection<TrackedFile>();
        }

        public async void StartTrackingAsync()
        {
            try
            {
                await _tracker.TrackAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void RemoveFiles()
        {
            throw new NotImplementedException();
        }

        private void OnTrackingTick(object? sender, TrackingEventArgs trackingArgs)
        {
            //Debug.WriteLine($"{trackingArgs.TrackedFile?.FileName} + {trackingArgs.TrackResponse}");
            //Debug.WriteLine(Status.ToString());

            _lastKnownFile.IsCurrentlyActive = false;

            if (!_config.IgnoreAFKTimer && LastInputInfo.IdleTime.TotalSeconds > App.Config.MaxAFKTime)
            {
                Status = TrackingStatus.UserIsAFK;
                return;
            }

            if (!_config.IgnoreActiveWindow)
            {
                if (ProcessUtils.IsWindowActive("photoshop"))
                    _psInactiveTime = 0;
                else
                    _psInactiveTime++;

                if (_psInactiveTime >= App.Config.PsActiveWindowTimeout)
                {
                    Status = TrackingStatus.PsIsNotActive;
                    return;
                }
            }

            if (trackingArgs.TrackResponse is TrackResponse.NoActiveDocument)
            {
                Status = TrackingStatus.NoDocuments;
            }

            if (trackingArgs.TrackResponse is TrackResponse.Failed)
            {
                Status = TrackingStatus.Failed;
                return;
            }

            if (trackingArgs.TrackResponse is TrackResponse.Success or TrackResponse.LastKnown)
            {
                TrackedFile trackedFile = GetOrCreateTrackedFile(trackingArgs.TrackedFileName);
                trackedFile.TrackedSeconds++;
                trackedFile.LastActiveTime = DateTimeOffset.Now;
                trackedFile.IsCurrentlyActive = true;

                _lastKnownFile = trackedFile;
                Status = TrackingStatus.Success;
            }

            UpdateSummary();
        }

        private void UpdateSummary()
        {
            int summary = 0;

            foreach (var file in TrackedFiles)
            {
                summary += file.TrackedSeconds;
            }

            SummarySeconds = summary;
        }

        private TrackedFile GetOrCreateTrackedFile(string trackedFileName)
        {
            if (_config.IgnoreFileExtension)
                trackedFileName = RemoveExtension(trackedFileName);

            if (_lastKnownFile.FileName == trackedFileName)
                return _lastKnownFile;

            TrackedFile? file = TrackedFiles.FirstOrDefault(f => f.FileName == trackedFileName);

            if (file is not null)
                return file;
            else
            {
                var newFile = new TrackedFile(trackedFileName);
                TrackedFiles.Add(newFile);
                return newFile;
            }
        }

        private string RemoveExtension(string trackedFileName)
        {
            int dotIndex = trackedFileName.LastIndexOf('.');
            return dotIndex > 0 ? trackedFileName[..dotIndex] : trackedFileName;
        }
    }

    public enum TrackingStatus
    {
        Success,
        NoDocuments,
        NotRunning,
        PsIsNotActive,
        UserIsAFK,
        Failed
    }
}
