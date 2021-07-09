using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace PSTimeTracker.PsTracking
{
    public class ProcessInfoService
    {
        public event EventHandler<string> ErrorOccured;

        #region Get Active Process

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        #endregion

        /// <summary>Returns <see langword="true"/> if PS window is active.</summary>
        public bool PhotoshopWindowIsActive { get { return GetActiveWindowProcess().ProcessName == PS_NAME; } }

        /// <summary>Returns <see langword="true"/> if PS is running.</summary>
        public bool PhotoshopIsRunning { get { return GetPhotoshopProcess() != null; } }

        private const string PS_NAME = "Photoshop";

        /// <summary>Returns <see langword="null"/> if Photoshop is not active.</summary>
        public string GetActivePhotoshopWindowTitle()
        {
            Process activeProcess = GetActiveWindowProcess();

            if (activeProcess == null)
                return null;
            else if (activeProcess.ProcessName != PS_NAME)
                return null;
            else
                return activeProcess.MainWindowTitle;
        }

        /// <summary>Returns <see langword="null"/> if Photoshop process is not found.</summary>
        public string GetPhotoshopWindowTitle()
        {
            Process psProcess = GetPhotoshopProcess();

            if (psProcess != null)
                return psProcess.MainWindowTitle;
            else
                return null;
        }

        /// <summary>Returns <see langword="null"/> if not running.</summary>
        private Process GetPhotoshopProcess()
        {
            var proc = Process.GetProcessesByName(PS_NAME).FirstOrDefault();
            return proc;
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
                ErrorOccured?.Invoke(this, $"Can't get active window process: {ex.Message}");
            }

            return activeProcess;
        }
    }
}
