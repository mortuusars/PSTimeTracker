using System;

namespace PSTimeTracker.Core
{
    public class MultipleComTracker : ITracker
    {
        private const int CODE_NO_ACTIVE_DOCUMENT = -2147352565;
        private const int CODE_APP_IS_BUSY = -2147417846;
        private const int CODE_CALL_FAILED = -2146233088;

        private dynamic _psCOMInterface;

        private int _failedCalls = 0;

        /// <summary>Gets currently active document name by calling PS COM Interface.</summary>
        /// <returns>
        /// <br>Empty string if PS is busy.</br>
        /// <br><see langword="null"/> if no documents open.</br>
        /// </returns>
        public string GetFileName()
        {
            if (_psCOMInterface == null)
                CreatePhotoshopCOMInstance();

            try
            {
                _failedCalls = 0;
                return _psCOMInterface.ActiveDocument.Name;
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
                if (_failedCalls > 10)
                    return null;

                _failedCalls++;

                CreatePhotoshopCOMInstance();

                return GetFileName();
            }
        }

        private void CreatePhotoshopCOMInstance()
        {
            _psCOMInterface = Activator.CreateInstance(Type.GetTypeFromProgID("Photoshop.Application"));
        }
    }
}
