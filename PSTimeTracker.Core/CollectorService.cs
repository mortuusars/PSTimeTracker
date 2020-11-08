using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace PSTimeTracker.Core
{
    public class CollectorService
    {
        public event EventHandler<string> ExceptionOccured;
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

        #region Get Active Process

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        #endregion


        bool isRunning;
        int summarySeconds;

        ObservableCollection<PsFile> _psFilesList;

        /// <summary>
        /// Every second collects info about opened files in Photoshop. Writes to provided collection.
        /// </summary>
        /// <param name="psFilesList">Collection to write to.</param>
        public CollectorService(ObservableCollection<PsFile> psFilesList)
        {
            _psFilesList = psFilesList;
            _psFilesList.CollectionChanged += (s, e) => CountSummarySeconds();
        }

        public async void StartCollecting()
        {
            isRunning = true;
            while (isRunning)
            {
                Collect();
                //CountSummarySeconds();
                await Task.Delay(1000);
            }
        }

        public void StopCollecting()
        {
            if (isRunning == false)
                return;

            isRunning = false;
        }

        private void Collect()
        {
            // Skip if user is afk for more that 10 seconds
            if (IdleTime.TotalSeconds > 10)
                return;

            Process activeProcess = GetActiveWindowProcess();

            if (activeProcess == null || activeProcess.ProcessName != "Photoshop")
            {
                return;
            }

            // Get filename from PS window title.
            // Photoshop changes window title depending on what file is open. 
            // DSC00000.jpg @ 100% (Layer 1, RGB/8)
            // Next we get all chars untill "@", and that is our filename.
            string fileName = Regex.Match(activeProcess.MainWindowTitle, @".*\s@").Value;
            fileName = fileName.Replace(" @", "");

            if (string.IsNullOrWhiteSpace(fileName))
                return;

            // Find current filename in list, if it was opened before
            // Add filename to list if it's new

            PsFile currentlyOpenedFile = _psFilesList.FirstOrDefault(f => f.FileName == fileName);

            if (currentlyOpenedFile == null)
            {
                currentlyOpenedFile = new PsFile() { FileName = fileName, FirstActiveTime = DateTimeOffset.Now };
                _psFilesList.Add(currentlyOpenedFile);
            }

            // Add seconds
            currentlyOpenedFile.TrackedSeconds++;

            CountSummarySeconds();

            currentlyOpenedFile.IsCurrentlyActive = true;
            currentlyOpenedFile.LastActiveTime = DateTimeOffset.Now;

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

        private Process GetActiveWindowProcess()
        {
            Process activeProcess = null;

            try
            {
                IntPtr hWnd = GetForegroundWindow();
                uint processId = 0;
                GetWindowThreadProcessId(hWnd, out processId);
                activeProcess = Process.GetProcessById((int)processId);
            }
            catch (Exception ex)
            {
                ExceptionOccured?.Invoke(this, $"Can't get active window process: {ex.Message}");
            }

            return activeProcess;
        }
    }
}
