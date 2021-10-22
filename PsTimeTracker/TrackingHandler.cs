using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSTimeTracker.Models;
using PSTimeTracker.Tracking;
using PropertyChanged;
using System.Diagnostics;

namespace PSTimeTracker
{
    public interface ITrackingHandler
    {
        ObservableCollection<TrackedFile> TrackedFiles { get; }
        long SummarySeconds { get; }

        void RemoveFiles();
        void StartTracking();
    }

    [AddINotifyPropertyChangedInterface]
    public class TrackingHandler : ITrackingHandler
    {
        public ObservableCollection<TrackedFile> TrackedFiles { get; private set; }
        public long SummarySeconds { get; private set; }

        private readonly ITracker _tracker;

        private TrackedFile? _lastKnownFile;

        public TrackingHandler(ITracker tracker)
        {
            _tracker = tracker;
            _tracker.TrackingTick += OnTrackingTick;

            TrackedFiles = new ObservableCollection<TrackedFile>();
        }

        public async void StartTracking()
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

        }

        private void OnTrackingTick(object? sender, TrackingEventArgs trackingArgs)
        {
            //Debug.WriteLine($"{trackingArgs.TrackedFile?.FileName} + {trackingArgs.TrackResponse}");

            if (_lastKnownFile is not null)
                _lastKnownFile.IsCurrentlyActive = false;

            if (trackingArgs.TrackResponse is TrackResponse.Success or TrackResponse.LastKnown)
            {
                if (trackingArgs.TrackedFile.FileName?.Length == 0)
                    return;

                TrackedFile trackedFile = GetOrCreateTrackedFile(trackingArgs.TrackedFile);
                trackedFile.TrackedSeconds++;
                trackedFile.LastActiveTime = DateTimeOffset.Now;
                trackedFile.IsCurrentlyActive = true;

                _lastKnownFile = trackedFile;
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

        private TrackedFile GetOrCreateTrackedFile(TrackedFileInfo trackedFile)
        {
            if (_lastKnownFile is not null && _lastKnownFile.FileName == trackedFile.FileName)
                return _lastKnownFile;

            TrackedFile? file = TrackedFiles.FirstOrDefault(f => f.FileName == trackedFile?.FileName);

            if (file is not null)
                return file;
            else
            {
                var newFile = new TrackedFile(trackedFile.FileName);
                TrackedFiles.Add(newFile);
                return newFile;
            }
        }
    }
}
