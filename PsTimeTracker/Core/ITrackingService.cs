using System;

namespace PSTimeTracker.Core
{
    public interface ITrackingService
    {
        int AFKTime { get; set; }
        bool IgnoreWindowState { get; set; }
        bool IgnoreAFK { get; set; }
        int MaxTimeSinceLastActive { get; set; }

        event EventHandler<int> SummarySecondsChanged;

        void CountSummarySeconds();
        void StartTracking();
        void StopTracking();
    }
}