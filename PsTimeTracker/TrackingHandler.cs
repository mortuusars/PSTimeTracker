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
        void StartTrackingAsync();
    }

    [AddINotifyPropertyChangedInterface]
    public class TrackingHandler : ITrackingHandler
    {
        public ObservableCollection<TrackedFile> TrackedFiles { get; private set; }
        public long SummarySeconds { get; private set; }

        public TrackingStatus Status { get; private set; }

        private readonly ITracker _tracker;

        private TrackedFile _lastKnownFile;
        private int _psInactiveTime = App.Config.PsActiveWindowTimeout + 1;

        public TrackingHandler()
        {
            _tracker = new Tracker();
            _tracker.TrackingTick += OnTrackingTick;

            _lastKnownFile = TrackedFile.Empty;
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

            if (!App.Config.IgnoreAFKTimer && LastInputInfo.IdleTime.TotalSeconds > App.Config.MaxAFKTime)
            {
                Status = TrackingStatus.UserIsAFK;
                return;
            }

            if (!App.Config.IgnoreActiveWindow)
            {
                if (ProcessUtils.IsWindowActive("photoshop"))
                    _psInactiveTime = 0;
                else
                    _psInactiveTime++;

                if ( _psInactiveTime >= App.Config.PsActiveWindowTimeout)
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
                //if (trackingArgs.TrackedFileName?.Length == 0)
                //    return;

                string trackedFileName = App.Config.IgnoreFileExtension ? 
                    trackingArgs.TrackedFileName[..trackingArgs.TrackedFileName.LastIndexOf('.')] : 
                    trackingArgs.TrackedFileName;

                TrackedFile trackedFile = GetOrCreateTrackedFile(trackedFileName);
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
            if (_lastKnownFile is not null && _lastKnownFile.FileName == trackedFileName)
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
