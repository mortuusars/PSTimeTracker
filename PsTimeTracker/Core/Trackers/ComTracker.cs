using System;
using System.IO;

namespace PSTimeTracker.Core
{
    public class ComTracker : ITracker
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
        public PSCallResult GetFileName()
        {
            if (_psCOMInterface == null)
                CreatePhotoshopCOMInstance();

            //Log("Trying to get filename");

            try
            {
                _failedCalls = 0;

                //Log($"doc name: {_psCOMInterface.ActiveDocument.Name}");
                return new PSCallResult(PSResponse.Success, _psCOMInterface.ActiveDocument.Name);
            }
            catch (Exception ex) when (ex.HResult == CODE_APP_IS_BUSY)
            {
                //Log("busy");
                return new PSCallResult(PSResponse.Busy, string.Empty);
            }
            catch (Exception ex) when (ex.HResult == CODE_NO_ACTIVE_DOCUMENT)
            {
                //Log("no active");
                return new PSCallResult(PSResponse.NoActiveDocument, string.Empty);
            }
            catch (Exception ex) when (ex.HResult == CODE_CALL_FAILED)
            {
                //Log("call failed");
                return new PSCallResult(PSResponse.Failed, string.Empty);
            }
            catch (Exception ex )
            {
                //Log("Something else: ");
                //Log(ex.Message);

                _failedCalls++;
                if (_failedCalls > 10)
                    return null;

                CreatePhotoshopCOMInstance();

                return GetFileName();
            }
        }

        private void CreatePhotoshopCOMInstance()
        {
            //Log("Creating instance");
            try
            {
                _psCOMInterface = Activator.CreateInstance(Type.GetTypeFromProgID("Photoshop.Application"));
            }
            catch (Exception ex)
            {
                //Log(ex.Message);
            }
            //Log("Created PS COM Instance");
        }

        private void Log(string message)
        {
            File.AppendAllText("log.txt", "\n" + message);
        }
    }
}
