using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PSTimeTracker.Models;
using PSTimeTracker.Services;

namespace PSTimeTracker.Core
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

        /// <summary><see langword="true"/> by default. Controls if tracking should stop when user is afk for more than <see cref="AFKTime"/> seconds.</summary>
        public bool CheckAFK { get; set; }
        /// <summary>Maximum allowed AFK Time in seconds. Default is 6 seconds.</summary>
        public int AFKTime { get; set; } = 6;
        /// <summary><see langword="true"/> by default. Controls if Photoshop should be active. Photoshop still needs to be running, obviously. </summary>
        public bool CheckActiveProcess { get; set; }
        /// <summary>How much time can pass after PS is not active that will still count. Default is 2 seconds</summary>
        public int MaxTimeSinceLastActive { get; set; } = 2;

        private bool isRunning;
        private int summarySeconds;
        private int psTimeSinceLastActive;

        readonly ObservableCollection<PsFile> _psFilesList;
        readonly ProcessInfoService _processInfoService;
        readonly Config _config;

        /// <summary>Every second tracks info about opened files in Photoshop. Writes to provided collection.</summary>
        /// <param name="psFilesList">Collection to write to.</param>
        public TrackingService(ObservableCollection<PsFile> psFilesList, ProcessInfoService processInfoService, Config config)
        {
            _psFilesList = psFilesList;
            _psFilesList.CollectionChanged += (s, e) => CountSummarySeconds();

            _processInfoService = processInfoService;
            _config = config;
            _config.PropertyChanged += (s, e) => LoadConfigSettings();

            LoadConfigSettings();
        }

        private void LoadConfigSettings()
        {
            CheckAFK = _config.CheckAFK;
            CheckActiveProcess = _config.OnlyActiveWindow;

            Debug.WriteLine("CheckActiveProcess is: " + CheckActiveProcess);
        }

        public async void StartTracking()
        {
            isRunning = true;
            while (isRunning)
            {
                if ( (CheckAFK && IdleTime.TotalSeconds < AFKTime) || !CheckAFK)
                    Track();

                await Task.Delay(1000);
            }
        }

        public void StopTracking()
        {
            if (isRunning == false)
                return;

            isRunning = false;
        }

        private void Track()
        {
            string title = _processInfoService.GetActivePhotoshopWindowTitle();

            // If filename is null - ps is not active.
            if (title == null)
            {
                psTimeSinceLastActive++;
                // If should check for active and time is larger than allowed.
                if (CheckActiveProcess && psTimeSinceLastActive > MaxTimeSinceLastActive)
                    return;
                else
                {
                    title = _processInfoService.GetPhotoshopWindowTitle();
                    // If null - PS is not running.
                    if (title == null)
                        return;
                }
            }
            else
                psTimeSinceLastActive = 0;

            string fileName = GetFileNameFromTitle(title);

            if (string.IsNullOrWhiteSpace(fileName))
                return;

            PsFile currentlyOpenedFile = GetOrCreateCurrentlyOpenedFile(fileName);

            // Add seconds
            currentlyOpenedFile.TrackedSeconds++;

            CountSummarySeconds();

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

        private string GetFileNameFromTitle(string title)
        {
            // Get filename from PS window title.
            // Photoshop changes window title depending on what file is open. 
            // DSC00000.jpg @ 100% (Layer 1, RGB/8)
            // Next we get all chars untill "@", and that is our filename.
            string fileName = Regex.Match(title, @".*\s@").Value;
            return fileName.Replace(" @", "");
        }

        public void CountSummarySeconds()
        {
            int newCount = 0;

            foreach (var file in _psFilesList)
            {
                newCount += file.TrackedSeconds;
                file.IsCurrentlyActive = false;
            }

            summarySeconds = newCount;
            SummarySecondsChanged?.Invoke(this, summarySeconds);
        }

        
    }
}
