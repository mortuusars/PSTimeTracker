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

        /// <summary><see langword="true"/> by default. Controls if tracking should stop when user is afk for more than <see cref="AFKTime"/> seconds.</summary>
        public bool CheckAFK { get; set; }
        /// <summary>Maximum allowed AFK Time in seconds. Default is 6 seconds.</summary>
        public int AFKTime { get; set; } = 6;
        /// <summary><see langword="true"/> by default. Controls if Photoshop should be active. Photoshop still needs to be running, obviously. </summary>
        public bool OnlyCheckActiveProcess { get; set; }
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
                if (_processInfoService.PhotoshopIsRunning)
                {
                    if ((CheckAFK && IdleTime.TotalSeconds < AFKTime) || !CheckAFK)
                        Track();
                }

                await Task.Delay(1000);
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
            // Getting fileName from photoshop
            string fileName;
            try
            {
                fileName = new ApplicationClass().ActiveDocument.Name;
            }
            catch (Exception)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(fileName))
                return;

            // How much time has passed since PS was active
            psTimeSinceLastActive = _processInfoService.PhotoshopWindowIsActive ? 0 : psTimeSinceLastActive + 1;

            // If should check for active and time is larger than allowed.
            if (OnlyCheckActiveProcess && psTimeSinceLastActive > MaxTimeSinceLastActive) 
                return;

            // Removing active flag from previous file
            if (lastActiveFile != null)
                lastActiveFile.IsCurrentlyActive = false;

            PsFile currentlyOpenedFile = GetOrCreateCurrentlyOpenedFile(fileName);

            // Change current file
            currentlyOpenedFile.TrackedSeconds++;
            currentlyOpenedFile.IsCurrentlyActive = true;
            currentlyOpenedFile.LastActiveTime = DateTimeOffset.Now;

            // Set current as last active
            lastActiveFile = currentlyOpenedFile;

            CountSummarySeconds();
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
