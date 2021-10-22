using System;
using System.IO;
using System.Linq;
using System.Windows.Threading;

namespace PSTimeTracker.Services
{
    public static class CrashManager
    {
        private const string CRASHES_FOLDER_NAME = "crash-reports";
        private static readonly string APP_CRASHES_FOLDER_PATH = $"{App.APP_FOLDER_PATH}{CRASHES_FOLDER_NAME}/";

        public static void HandleCrash(DispatcherUnhandledExceptionEventArgs exceptionEventArgs)
        {
            exceptionEventArgs.Handled = true;

            string crashReportFileName = $"{APP_CRASHES_FOLDER_PATH}crash-{DateTimeOffset.Now:yyyy-mm-dd_HH-mm-ss}.txt";
            string crashMessage = $"HResult: {exceptionEventArgs.Exception.HResult}\n\n" +
                                  $"Error: {exceptionEventArgs.Exception}\n\n";
            if (exceptionEventArgs.Exception.InnerException is not null)
                crashMessage += "Inner Exception:" + exceptionEventArgs.Exception.InnerException;

            try
            {
                Directory.CreateDirectory(APP_CRASHES_FOLDER_PATH);
                File.WriteAllText(crashReportFileName, crashMessage);

                ViewManager.DisplayErrorMessage(App.APP_NAME + " has crashed\n\n" + crashMessage);
            }
            catch (Exception)
            {
                ViewManager.DisplayErrorMessage(App.APP_NAME + " has crashed\n\nCrash report has not been saved due to an error\n\n" + crashMessage);
            }
            
            CleanUpFolder();
            App.Current.Shutdown();
        }

        private static void CleanUpFolder()
        {
            try
            {
                FileInfo[] reportFiles = new DirectoryInfo(APP_CRASHES_FOLDER_PATH).GetFiles().OrderBy(f => f.LastWriteTime).ToArray();

                if (reportFiles.Length > 10)
                    reportFiles.Take(reportFiles.Length - 8).ToList().ForEach(f => File.Delete(f.FullName));
            }
            catch (Exception)
            {
            }
        }
    }
}
