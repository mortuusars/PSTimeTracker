using System;
using System.Diagnostics;

namespace PSTimeTracker.Tracking;

internal class PhotoshopCOM : IPhotoshop
{
    private const int CODE_NO_ACTIVE_DOCUMENT = -2147352565;
    private const int CODE_APP_IS_BUSY = -2147417846;
    private const int CODE_CALL_FAILED = -2146233088;
    private const int CODE_NOT_RUNNING = -2147023174;
    private const int CODE_OBJECT_DISCONNECTED = -2147417848;

    private dynamic? _photoshop;

    private int _failedCalls = 0;

    public TrackFileResult GetActiveDocumentName()
    {
        if (_photoshop is null)
            CreatePhotoshopCOMInstance();

        try
        {
            string filename = _photoshop!.ActiveDocument.Name;
            return new TrackFileResult(Status.Success, filename);
        }
        catch (Exception ex) when (ex.HResult == CODE_APP_IS_BUSY)
        {
            return new TrackFileResult(Status.Busy, string.Empty);
        }
        catch (Exception ex) when (ex.HResult == CODE_NO_ACTIVE_DOCUMENT)
        {
            return new TrackFileResult(Status.NoActiveDocument, string.Empty);
        }
        catch (Exception ex) when (ex.HResult == CODE_CALL_FAILED)
        {
            return new TrackFileResult(Status.Failed, string.Empty);
        }
        catch (Exception ex) when (ex.HResult == CODE_NOT_RUNNING)
        {
            _failedCalls++;

            if (_failedCalls > 10)
            {
                _failedCalls = 0;
                CreatePhotoshopCOMInstance();
            }

            return new TrackFileResult(Status.PSNotRunning, string.Empty);
        }
        catch (Exception ex) when (ex.HResult == CODE_OBJECT_DISCONNECTED)
        {
            return new TrackFileResult(Status.PSNotRunning, string.Empty);
        }
        catch (Exception ex)
        {
            _failedCalls++;

            Debug.WriteLine("PS FAILED: " + ex.Message);

            if (_failedCalls >= 10)
            {
                _failedCalls = 0;
                return new TrackFileResult(Status.Failed, string.Empty);
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