using System;
using Photoshop;

namespace PSTimeTracker.Core
{
    public class ComTracker : ITracker
    {
        private const int CODE_NO_ACTIVE_DOCUMENT = -2147352565;
        private const int CODE_APP_IS_BUSY = -2147417846;
        private const int CODE_CALL_FAILED = -2146233088;

        private ApplicationClass PS;

        private int failedCalls = 0;

        /// <summary>Gets currently active document name by calling PS COM Object Library.</summary>
        /// <returns>
        /// <br>Empty string if PS is busy.</br>
        /// <br><see langword="null"/> if no documents open.</br>
        /// </returns>
        public string GetFileName()
        {
            if (PS == null)
                PS = new ApplicationClass();

            try
            {
                return PS.ActiveDocument.Name;
            }
            catch (Exception ex) when (ex.HResult == CODE_APP_IS_BUSY)
            {
                return "";
            }
            catch (Exception ex) when (ex.HResult == CODE_NO_ACTIVE_DOCUMENT)
            {
                return null;
            }
            catch (Exception ex) when (ex.HResult == CODE_CALL_FAILED)
            {
                return null;
            }
            catch (Exception)
            {
                if (failedCalls > 10)
                    return null;

                failedCalls++;

                PS = new ApplicationClass();
                return GetFileName();
            }
        }
    }
}
