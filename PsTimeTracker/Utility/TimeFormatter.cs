using System;

namespace PSTimeTracker
{
    public static class TimeFormatter
    {
        /// <summary>
        /// Formats seconds to full time. 12:34:56 or if hours < 0: 12:34.
        /// </summary>
        /// <param name="seconds"></param>
        public static string GetTimeStringFromSecods(int seconds)
        {
            DateTimeOffset time = new DateTimeOffset().AddSeconds(seconds);

            if (time.Hour > 0)
                return time.ToString("HH:mm:ss");
            else
                return time.ToString("mm:ss");
        }
    }
}
