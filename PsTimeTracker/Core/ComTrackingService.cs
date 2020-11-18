using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using PSTimeTracker.Models;
using PSTimeTracker.Services;
using Photoshop;

namespace PSTimeTracker.Core
{
    public class ComTrackingService : ITrackingService
    {
        private const int CODE_NO_ACTIVE_DOCUMENT = -2147352565;
        private const int CODE_APP_IS_BUSY = -2147417846;

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

        private bool isRunning;
        private int summarySeconds;
        private int psTimeSinceLastActive;

        private PsFile lastActiveFile;

        private readonly ObservableCollection<PsFile> _psFilesList;
        private readonly ProcessInfoService _processInfoService;

        /// <summary>Every second tracks info about opened files in Photoshop. Writes to provided collection.</summary>
        /// <param name="psFilesList">Collection to write to.</param>
        public ComTrackingService(ObservableCollection<PsFile> psFilesList, ProcessInfoService processInfoService)
        {
            _psFilesList = psFilesList;
            _psFilesList.CollectionChanged += (s, e) => CountSummarySeconds();

            _processInfoService = processInfoService;
        }

        public async void StartTracking()
        {
            isRunning = true;
            while (isRunning)
            {
                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                if (_processInfoService.PhotoshopIsRunning)
                {
                    if (IgnoreAFK || (IdleTime.TotalSeconds < AFKTime))
                        Track();
                }

                //Debug.WriteLine("Tracking done in: " + stopwatch.ElapsedMilliseconds + "ms");

                int delay = CheckDelayMS - (int)stopwatch.ElapsedMilliseconds;
                if (delay < 1)
                    delay = 1;

                await Task.Delay(delay);

                //Debug.WriteLine("Total (should be 1000ms): " + stopwatch.ElapsedMilliseconds + "ms");

                stopwatch.Stop();
            }
        }

        public void StopTracking()
        {
            if (!isRunning)
                return;

            isRunning = false;
        }

        private void Track()
        {
            psTimeSinceLastActive = _processInfoService.PhotoshopWindowIsActive ? 0 : psTimeSinceLastActive + 1;
            if (IgnoreWindowState == false && psTimeSinceLastActive > MaxTimeSinceLastActive) return;

            if (lastActiveFile != null)
                lastActiveFile.IsCurrentlyActive = false;

            PsFile currentlyActiveFile;
            string fileName = GetFileNameInTime(100);

            if (fileName == null) return;
            else if (fileName.Length == 0)
            {
                if (lastActiveFile == null) return;
                else currentlyActiveFile = lastActiveFile;
            }
            else
                currentlyActiveFile = GetOrCreateCurrentlyOpenedFile(fileName);

            ChangeFileRecord(currentlyActiveFile);

            lastActiveFile = currentlyActiveFile;

            CountSummarySeconds();
        }

        private string GetFileNameInTime(int milliseconds)
        {
            var task = Task.Run(() => GetFileName());
            if (task.Wait(milliseconds))
                return task.Result;
            else
                return "";
        }

        /// <summary>Gets currently active document name.</summary>
        /// <returns>
        /// <br>Empty string if PS is busy.</br>
        /// <br><see langword="null"/> if no documents open.</br>
        /// </returns>
        private string GetFileName()
        {
            try
            {
                return new ApplicationClass().ActiveDocument.Name;
            }
            catch (Exception ex) when (ex.InnerException.HResult == CODE_APP_IS_BUSY)
            {
                return "";
            }
            catch (Exception ex) when (ex.InnerException.HResult == CODE_NO_ACTIVE_DOCUMENT)
            {
                return null;
            }
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

            PsFile currentlyOpenedFile = _psFilesList.FirstOrDefault(f => f.FileName == fileName);

            if (currentlyOpenedFile == null)
            {
                currentlyOpenedFile = new PsFile() { FileName = fileName, FirstActiveTime = DateTimeOffset.Now };
                _psFilesList.Add(currentlyOpenedFile);
            }

            return currentlyOpenedFile;
        }

        public void CountSummarySeconds()
        {
            int newCount = 0;

            foreach (var file in _psFilesList)
            {
                newCount += file.TrackedSeconds;
            }

            summarySeconds = newCount;
            SummarySecondsChanged?.Invoke(this, summarySeconds);
        }
    }
}
