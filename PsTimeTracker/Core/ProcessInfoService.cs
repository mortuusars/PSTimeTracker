using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PSTimeTracker.Core
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
            Process psProcess = Process.GetProcessesByName(PS_NAME).FirstOrDefault();

            if (psProcess != null)
                return psProcess.MainWindowTitle;
            else
                return null;
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
