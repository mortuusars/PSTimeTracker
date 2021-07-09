using System;
using System.IO;

namespace PSTimeTracker.PsTracking
{
    public class ComTracker : ITracker
    {
        private const int CODE_NO_ACTIVE_DOCUMENT = -2147352565;
        private const int CODE_APP_IS_BUSY = -2147417846;
        private const int CODE_CALL_FAILED = -2146233088;

        private dynamic _psCOMInterface;
        private ProcessInfoService _processInfoService;
        private int _failedCalls = 0;


        public ComTracker(ProcessInfoService processInfoService)
        {
            _processInfoService = processInfoService;
        }

        /// <summary>Gets currently active document name by calling PS COM Interface.</summary>
        /// <returns>
        /// <br>Empty string if PS is busy.</br>
        /// <br><see langword="null"/> if no documents open.</br>
        /// </returns>
        public PSCallResult GetFileName()
        {
            if (_psCOMInterface == null)
                CreatePhotoshopCOMInstance();

            try
            {
                string filename = _psCOMInterface.ActiveDocument.Name;
                _failedCalls = 0;
                return new PSCallResult(PSResponse.Success, filename);
            }
            catch (Exception ex) when (ex.HResult == CODE_APP_IS_BUSY)
            {
                return new PSCallResult(PSResponse.Busy, string.Empty);
            }
            catch (Exception ex) when (ex.HResult == CODE_NO_ACTIVE_DOCUMENT)
            {
                return new PSCallResult(PSResponse.NoActiveDocument, string.Empty);
            }
            catch (Exception ex) when (ex.HResult == CODE_CALL_FAILED)
            {
                return new PSCallResult(PSResponse.Failed, string.Empty);
            }
            catch (Exception)
            {
                _failedCalls++;
                if (_failedCalls > 5)
                    return new PSCallResult(PSResponse.NoActiveDocument, string.Empty);

                CreatePhotoshopCOMInstance();

                return GetFileName();
            }
        }

        private void CreatePhotoshopCOMInstance()
        {
            try
            {
                if (_processInfoService.PhotoshopIsRunning == false)
                    return;

                _psCOMInterface = Activator.CreateInstance(Type.GetTypeFromProgID("Photoshop.Application"));
            }
            catch
            {

            }
        }
    }
}
