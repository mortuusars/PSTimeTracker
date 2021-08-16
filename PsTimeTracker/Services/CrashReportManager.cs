using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PSTimeTracker.Services
{
    public class CrashReportManager
    {
        private const string CRASHES_FOLDER_NAME = "crash-reports";
        private static readonly string APP_CRASHES_FOLDER_PATH = $"{App.APP_FOLDER_PATH}{CRASHES_FOLDER_NAME}/";

        FileInfo[] reportFiles;

        /// <summary>
        /// Write report to a txt file in a crash-reports folder.
        /// </summary>
        /// <param name="crashMessage">Crash message that will be written.</param>
        public void ReportCrash(string crashMessage)
        {
            string crashReportFileName = $"{APP_CRASHES_FOLDER_PATH}crash-{DateTimeOffset.Now:yyyy-mm-dd_HH-mm-ss}.txt";

            try
            {
                Directory.CreateDirectory(APP_CRASHES_FOLDER_PATH);
                File.WriteAllText(crashReportFileName, crashMessage);
            }
            catch (Exception ex)
            {
                App.DisplayErrorMessage($"{App.APP_NAME} has crashed.\n" +
                    $"Creating crash-report failed: {ex.Message}\n" +
                    $"Error that caused crash: {crashMessage}");
            }
        }

        public void CleanUpFolder()
        {
            GetSortedDirectoryFiles();
            
            try
            {
                while (reportFiles.Length > 50)
                {
                    File.Delete(reportFiles.First().FullName);
                    GetSortedDirectoryFiles();
                }
            }
            catch (Exception ex)
            {
                App.DisplayErrorMessage($"Failed to cleanup crash-reports folder: {ex.Message}");
            }
        }

        private void GetSortedDirectoryFiles()
        {
            reportFiles = new DirectoryInfo(APP_CRASHES_FOLDER_PATH).GetFiles().OrderBy(f => f.LastWriteTime).ToArray();
        }
    }
}
