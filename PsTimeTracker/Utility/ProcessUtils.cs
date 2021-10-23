using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace PSTimeTracker
{
    public static class ProcessUtils
    {
        public static bool IsProcessRunning(string processName) => GetProcess(processName) != null;
        public static bool IsWindowActive(string processName)
        {
            string? procName = GetActiveWindowProcess()?.ProcessName;
            return procName is not null && procName.Equals(processName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Returns empty string if process is not found.</summary>
        public static string GetWindowTitle(string processName) => GetProcess(processName)?.MainWindowTitle ?? string.Empty;

        /// <summary>Returns Process by name or <see langword="null"/> if not running.</summary>
        private static Process? GetProcess(string processName)
        {
            return Process.GetProcessesByName(processName).FirstOrDefault();
        }

        private static Process? GetActiveWindowProcess()
        {
            IntPtr hWnd = GetForegroundWindow();
            GetWindowThreadProcessId(hWnd, out uint processId);
            return Process.GetProcessById((int)processId);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    }
}
