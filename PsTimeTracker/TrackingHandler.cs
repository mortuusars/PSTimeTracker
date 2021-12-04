using Microsoft.Toolkit.Mvvm.ComponentModel;
using PropertyChanged;
using PSTimeTracker.Configuration;
using PSTimeTracker.Models;
using PSTimeTracker.Tracking;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PSTimeTracker
{
    public interface ITrackingHandler
    {
        /// <summary>
        /// Collection of the tracked files.
        /// </summary>
        ObservableCollection<TrackedFile> TrackedFiles { get; }

        /// <summary>
        /// Combined time of all tracked files.
        /// </summary>
        long SummarySeconds { get; }

        /// <summary>
        /// 
        /// </summary>
        void StartTracking();
        void StopTracking();

        /// <summary>
        /// Merge several files into one, combining their time and removing input files.
        /// </summary>
        /// <param name="destination">File to which others would be merged to.</param>
        /// <param name="inputs">List of files, that would be merged into destination. They will be removed in the process.</param>
        void MergeFiles(TrackedFile destination, IEnumerable<TrackedFile> inputs);
        /// <summary>
        /// Removes files from TrackedFiles collection.
        /// </summary>
        /// <param name="filesToRemove"></param>
        void RemoveFiles(IEnumerable<TrackedFile> filesToRemove);
    }

    public class TrackingHandler : ObservableObject, ITrackingHandler
    {
        public ObservableCollection<TrackedFile> TrackedFiles { get; }
        public long SummarySeconds { get => _summarySeconds; private set { _summarySeconds = value; OnPropertyChanged(nameof(SummarySeconds)); } }
        public TrackingStatus TrackingStatus { get; private set; }

        public bool IsTracking { get => _isTracking; set { _isTracking = value; OnPropertyChanged(nameof(IsTracking)); } }

        public TimeSpan TrackingInterval { get => _trackingInterval; set { _trackingInterval = value; OnPropertyChanged(nameof(TrackingInterval)); _trackingTimer.Interval = value; } }

        private long _summarySeconds;
        private bool _isTracking;

        private TrackedFile _lastKnownFile;
        private int _psInactiveTime;
        private TimeSpan _trackingInterval;
        private readonly DispatcherTimer _trackingTimer;

        private readonly ITracker _tracker;
        private readonly Config _config;

        public TrackingHandler(Config config)
        {
            _tracker = new Tracker();
            _config = config;

            _trackingTimer = new DispatcherTimer();
            _trackingTimer.Interval = TimeSpan.FromSeconds(1);
            _trackingTimer.Tick += OnTrackingTick;

            _lastKnownFile = TrackedFile.Empty;
            _psInactiveTime = _config.PsInactiveWindowTimeout + 1;
            TrackedFiles = new ObservableCollection<TrackedFile>();
        }

        public void StartTracking()
        {
            IsTracking = true;
            _trackingTimer.Start();
        }

        public void StopTracking()
        {
            IsTracking = false;
            _trackingTimer.Stop();
        }

        private async void OnTrackingTick(object? sender, EventArgs e)
        {
            if (UserIsAFK())
            {
                _lastKnownFile.Deactivate();
                TrackingStatus = TrackingStatus.UserIsAFK;
                return;
            }

            if (_config.KeepTrackingWhenWindowInactive is false && PSWindowIsNotActive())
            {
                _lastKnownFile.Deactivate();
                TrackingStatus = TrackingStatus.PsIsNotActive;
                return;
            }

            TrackFileResult result = await _tracker.TrackFilenameAsync();

            TrackingStatus = result.Status switch
            {
                Status.Success => string.IsNullOrWhiteSpace(result.Filename) ? UseLastKnownFile() : UseKnownFile(result),
                Status.Busy or Status.TimedOut => UseLastKnownFile(),
                Status.PSNotRunning => TrackingStatus.NotRunning,
                Status.NoActiveDocument => TrackingStatus.NoDocuments,
                _ => TrackingStatus.Failed
            };

            if (TrackingStatus is TrackingStatus.NoDocuments or TrackingStatus.NotRunning or TrackingStatus.Failed)
                _lastKnownFile.Deactivate();

            UpdateSummary();

            //Console.WriteLine(TrackingStatus);
            //Console.ReadLine();
            //Debug.WriteLine(TrackingStatus);
        }

        public void RemoveFiles(IEnumerable<TrackedFile> filesToRemove)
        {
            foreach (var file in filesToRemove)
            {
                if (_lastKnownFile == file)
                    _lastKnownFile = TrackedFile.Empty;

                TrackedFiles.Remove(file);
            }
        }

        public void MergeFiles(TrackedFile destination, IEnumerable<TrackedFile> inputs)
        {
            if (TrackedFiles.Contains(destination))
            {
                foreach (var inputFile in inputs)
                {
                    destination.TrackedSeconds += inputFile.TrackedSeconds;
                    TrackedFiles.Remove(inputFile);
                }
            }
        }

        private TrackingStatus UseKnownFile(TrackFileResult result)
        {
            _lastKnownFile.Deactivate();

            TrackedFile trackedFile = GetOrCreateTrackedFile(result.Filename);
            trackedFile.ActivateFor((int)_trackingTimer.Interval.TotalSeconds);
            _lastKnownFile = trackedFile;

            return TrackingStatus.Success;
        }

        private TrackingStatus UseLastKnownFile()
        {
            if (_lastKnownFile.IsEmpty())
                return TrackingStatus.Failed;

            TrackedFile trackedFile = GetOrCreateTrackedFile(_lastKnownFile.FileName);
            trackedFile.ActivateFor((int)_trackingTimer.Interval.TotalSeconds);
            return TrackingStatus.LastKnown;
        }

        private bool UserIsAFK()
        {
            return _config.IgnoreAFK is false && LastInputInfo.IdleTime.TotalSeconds > _config.MaxAFKTime;
        }

        private bool PSWindowIsNotActive()
        {
            if (ProcessUtils.IsWindowActive("photoshop"))
                _psInactiveTime = 0;
            else
                _psInactiveTime++;

            return _psInactiveTime >= App.Config.PsInactiveWindowTimeout;
        }

        private void UpdateSummary()
        {
            SummarySeconds = TrackedFiles.Aggregate(0, (total, next) => next.TrackedSeconds);

            //int summary = 0;

            //foreach (var file in TrackedFiles)
            //{
            //    summary += file.TrackedSeconds;
            //}

            //SummarySeconds = summary;
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
        Failed,
        LastKnown
    }
}
