using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using PSTimeTracker.Models;

namespace PSTimeTracker.PsTracking
{
    public class TrackingService
    {
        public event EventHandler<int> SummarySecondsChanged;

        #region Last Input Time

        [DllImport("user32.dll", SetLastError = false)]
        private static extern bool GetLastInputInfo(ref Lastinputinfo plii);
        private static readonly DateTime SystemStartup = DateTime.Now.AddMilliseconds(-Environment.TickCount);

        [StructLayout(LayoutKind.Sequential)]
        private struct Lastinputinfo
        {
            public uint cbSize;
            public readonly int dwTime;
        }

        public static DateTime LastInput => SystemStartup.AddMilliseconds(LastInputTicks);
        public static TimeSpan IdleTime => DateTime.Now.Subtract(LastInput);

        private static int LastInputTicks
        {
            get {
                var lastInputInfo = new Lastinputinfo { cbSize = (uint)Marshal.SizeOf(typeof(Lastinputinfo)) };
                GetLastInputInfo(ref lastInputInfo);
                return lastInputInfo.dwTime;
            }
        }

        #endregion

        public int CheckDelayMS { get; set; } = 1000;
        /// <summary><see langword="true"/> by default. Controls if tracking should stop when user is afk for more than <see cref="AFKTime"/> seconds.</summary>
        public bool IgnoreAFK { get; set; }
        /// <summary>Maximum allowed AFK Time in seconds. Default is 6 seconds.</summary>
        public int AFKTime { get; set; } = 6;
        /// <summary><see langword="true"/> by default. Controls if Photoshop should be active. Photoshop still needs to be running, obviously. </summary>
        public bool IgnoreWindowState { get; set; }
        /// <summary>How much time can pass after PS is not active that will still count. Default is 2 seconds</summary>
        public int MaxTimeSinceLastActive { get; set; } = 2;

        public ITracker Tracker { get; set; }

        private bool isRunning;
        private int summarySeconds;
        private int psTimeSinceLastActive = 9999;

        private PsFile lastActiveFile;

        private ObservableCollection<PsFile> _filesList;
        private readonly ProcessInfoService _processInfoService;

        /// <summary>Every second tracks info about opened files in Photoshop. Writes to provided collection.</summary>
        /// <param name="FilesList">Collection to write to.</param>
        public TrackingService(ref ObservableCollection<PsFile> FilesList, ProcessInfoService processInfoService, ITracker tracker)
        {
            _filesList = FilesList;
            _filesList.CollectionChanged += (s, e) => CountSummarySeconds();

            _processInfoService = processInfoService;
            Tracker = tracker;
        }

        public async void StartTracking()
        {
            var stopwatch = new System.Diagnostics.Stopwatch();

            isRunning = true;
            while (isRunning)
            {
                stopwatch.Start();

                if (_processInfoService.PhotoshopIsRunning)
                {
                    if (IgnoreAFK || (IdleTime.TotalSeconds < AFKTime))
                        Track();
                }

                int delay = CheckDelayMS - (int)stopwatch.ElapsedMilliseconds;
                if (delay < 1)
                    delay = 1;

                await Task.Delay(delay);

                stopwatch.Stop();
                stopwatch.Reset();
            }
        }

        public void StopTracking() => isRunning = false;

        public void CountSummarySeconds()
        {
            int newCount = 0;

            foreach (var file in _filesList)
            {
                newCount += file.TrackedSeconds;
            }

            summarySeconds = newCount;
            SummarySecondsChanged?.Invoke(this, summarySeconds);
        }

        private void Track()
        {
            psTimeSinceLastActive = _processInfoService.PhotoshopWindowIsActive ? 0 : psTimeSinceLastActive + 1;
            if (IgnoreWindowState == false && psTimeSinceLastActive > MaxTimeSinceLastActive) return;

            if (lastActiveFile != null)
                lastActiveFile.IsCurrentlyActive = false;

            PsFile currentlyActiveFile;
            var callResult = GetFileNameInTime(100);

            if (callResult.PSResponse == PSResponse.NoActiveDocument) // Stop counting if no documents open.
            {
                return;
            }
            else if (callResult.PSResponse == PSResponse.Busy || callResult.PSResponse == PSResponse.Failed) // Keep counting on last known file if PS not responding
            {
                if (lastActiveFile == null)
                    return;
                else
                    currentlyActiveFile = lastActiveFile;
            }
            else
            {
                currentlyActiveFile = GetOrCreateCurrentlyOpenedFile(callResult.Filename);
            }

            ChangeFileRecord(currentlyActiveFile);

            lastActiveFile = currentlyActiveFile;

            CountSummarySeconds();
        }

        private PSCallResult GetFileNameInTime(int taskTimeout)
        {
            if (!_processInfoService.PhotoshopIsRunning)
                return new PSCallResult(PSResponse.NoActiveDocument, string.Empty);

            var task = Task.Run(() => Tracker.GetFileName());
            if (task.Wait(taskTimeout))
                return task.Result;
            else
                return new PSCallResult(PSResponse.Failed, string.Empty);
        }

        private void ChangeFileRecord(PsFile currentlyOpenedFile)
        {
            currentlyOpenedFile.TrackedSeconds++;
            currentlyOpenedFile.IsCurrentlyActive = true;
            currentlyOpenedFile.LastActiveTime = DateTimeOffset.Now;
        }

        /// <summary>Finds current filename in list, if it was opened before, otherwise adds file to a list.</summary>
        private PsFile GetOrCreateCurrentlyOpenedFile(string fileName)
        {
            // Find current filename in list, if it was opened before
            // Add filename to list if it's new

            PsFile currentlyOpenedFile = _filesList.FirstOrDefault(f => f.FileName == fileName);

            if (currentlyOpenedFile == null)
            {
                currentlyOpenedFile = new PsFile() { FileName = fileName, FirstActiveTime = DateTimeOffset.Now };
                _filesList.Add(currentlyOpenedFile);
            }

            return currentlyOpenedFile;
        }
    }
}
