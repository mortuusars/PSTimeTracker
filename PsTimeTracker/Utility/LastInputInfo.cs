using System;
using System.Runtime.InteropServices;

namespace PSTimeTracker
{
    public static class LastInputInfo
    {
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
            get
            {
                var lastInputInfo = new Lastinputinfo { cbSize = (uint)Marshal.SizeOf(typeof(Lastinputinfo)) };
                GetLastInputInfo(ref lastInputInfo);
                return lastInputInfo.dwTime;
            }
        }
    }
}
