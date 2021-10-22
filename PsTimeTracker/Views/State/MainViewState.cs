using PSTimeTracker.PsTracking;
using System.IO;
using FileIO;
using System.Windows;

namespace PSTimeTracker
{
    public class MainViewState
    {
        private const string STATE_FILENAME = "mainWindowState.";
        private const string STATE_DIR_NAME = "state\\";

        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool AlwaysOnTop { get; set; }
        public SizeToContent SizeToContent { get; set; }
        public Sorting SortingOrder { get; set; }

        public void Save()
        {
            _ = Directory.CreateDirectory(App.APP_FOLDER_PATH + STATE_DIR_NAME);
            _ = JsonManager.SerializeAndWrite(this, App.APP_FOLDER_PATH + STATE_DIR_NAME + STATE_FILENAME);
        }

        public static MainViewState Load()
        {
            return JsonManager.ReadAndDeserialize<MainViewState>(App.APP_FOLDER_PATH + STATE_DIR_NAME + STATE_FILENAME)
                ?? new MainViewState();
        }
    }
}
