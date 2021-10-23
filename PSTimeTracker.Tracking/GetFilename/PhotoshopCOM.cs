using System;
using System.Diagnostics;

namespace PSTimeTracker.Tracking
{
    public interface IPhotoshop
    {
        PSGetNameResult GetActiveDocumentName();
    }

    public class PhotoshopCOM : IPhotoshop
    {
        private const int CODE_NO_ACTIVE_DOCUMENT = -2147352565;
        private const int CODE_APP_IS_BUSY = -2147417846;
        private const int CODE_CALL_FAILED = -2146233088;
        private const int CODE_NOT_RUNNING = -2147023174;
        private const int CODE_OBJECT_DISCONNECTED = -2147417848;

        private dynamic? _photoshop;

        private int _getNameFailedCalls = 0;

        public PSGetNameResult GetActiveDocumentName()
        {
            if (_photoshop is null)
                CreatePhotoshopCOMInstance();

            try
            {
                string filename = _photoshop.ActiveDocument.Name;
                return new PSGetNameResult(PSResponse.Success, filename);
            }
            catch (Exception ex) when (ex.HResult == CODE_APP_IS_BUSY)
            {
                return new PSGetNameResult(PSResponse.Busy, string.Empty);
            }
            catch (Exception ex) when (ex.HResult == CODE_NO_ACTIVE_DOCUMENT)
            {
                return new PSGetNameResult(PSResponse.NoActiveDocument, string.Empty);
            }
            catch (Exception ex) when (ex.HResult == CODE_CALL_FAILED)
            {
                return new PSGetNameResult(PSResponse.Failed, string.Empty);
            }
            catch (Exception ex) when (ex.HResult == CODE_NOT_RUNNING)
            {
                _getNameFailedCalls++;

                if (_getNameFailedCalls > 10)
                {
                    _getNameFailedCalls = 0;
                    CreatePhotoshopCOMInstance();
                }

                return new PSGetNameResult(PSResponse.PSNotRunning, string.Empty);
            }
            catch (Exception ex) when (ex.HResult == CODE_OBJECT_DISCONNECTED)
            {
                return new PSGetNameResult(PSResponse.PSNotRunning, string.Empty);
            }
            catch (Exception ex)
            {
                _getNameFailedCalls++;

                Debug.WriteLine("PS FAILED: " + ex.Message);

                if (_getNameFailedCalls >= 5)
                {
                    _getNameFailedCalls = 0;
                    return new PSGetNameResult(PSResponse.Failed, string.Empty);
                }

                CreatePhotoshopCOMInstance();
                return GetActiveDocumentName();
            }
        }

        private void CreatePhotoshopCOMInstance()
        {
            if (Process.GetProcessesByName("photoshop").Length > 0)
            {
                try { _photoshop = Activator.CreateInstance(Type.GetTypeFromProgID("Photoshop.Application")); }
                catch { }
            }
        }
    }
}
