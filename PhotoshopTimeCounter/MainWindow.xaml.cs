using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace PhotoshopTimeCounter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string MAIN_WINDOW_STATE_FILENAME = "mainWindowState.";
        private string MAIN_WINDOW_STATE_FILEPATH = App.APP_FOLDER_PATH + MAIN_WINDOW_STATE_FILENAME;

        private SortingTypes currentSorting;

        public MainWindow()
        {
            InitializeComponent();

            SetWindowFromState();

            MainItemsControl.Items.IsLiveSorting = true;

            SortList(currentSorting);
        }

        #region Resising Window

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private enum ResizeDirection { Left = 61441, Right = 61442, Top = 61443, Bottom = 61446, BottomRight = 61448, }

        private void ResizeButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("entered event");
            var hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            SendMessage(hwndSource.Handle, 0x112, (IntPtr)ResizeDirection.BottomRight, IntPtr.Zero);
        }

        #endregion

        private void SortList(SortingTypes sortBy) {
            MainItemsControl.Items.SortDescriptions.Clear();

            SortingPopup.Text = sortBy.ToString();

            switch (sortBy) {
                case SortingTypes.Name:
                    MainItemsControl.Items.SortDescriptions.Add(new SortDescription(nameof(PsFileInfo.FileName), ListSortDirection.Ascending));
                    break;
                case SortingTypes.NameReversed:
                    MainItemsControl.Items.SortDescriptions.Add(new SortDescription(nameof(PsFileInfo.FileName), ListSortDirection.Descending));
                    break;
                case SortingTypes.Time:
                    MainItemsControl.Items.SortDescriptions.Add(new SortDescription(nameof(PsFileInfo.SecondsActive), ListSortDirection.Ascending));
                    break;
                case SortingTypes.TimeReversed:
                    MainItemsControl.Items.SortDescriptions.Add(new SortDescription(nameof(PsFileInfo.SecondsActive), ListSortDirection.Descending));
                    break;
                case SortingTypes.TimeAdded:
                    MainItemsControl.Items.SortDescriptions.Add(new SortDescription(nameof(PsFileInfo.FirstOpenTime), ListSortDirection.Ascending));
                    break;
                case SortingTypes.TimeAddedReversed:
                    MainItemsControl.Items.SortDescriptions.Add(new SortDescription(nameof(PsFileInfo.FirstOpenTime), ListSortDirection.Descending));
                    break;
                default:
                    break;
            }
        }

        protected override void OnClosing(CancelEventArgs e) {
            SaveMainWindowState();
            base.OnClosing(e);
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            this.DragMove();
        }

        

        private WindowState LoadMainWindowState() {
            try {
                string jsonString = File.ReadAllText(MAIN_WINDOW_STATE_FILEPATH);
                return JsonSerializer.Deserialize<WindowState>(jsonString);
            }
            catch (Exception) {
                return new WindowState();
            }
        }

        private void SetWindowFromState()
        {
            WindowState windowState = LoadMainWindowState();
            this.Left = windowState.Left;
            this.Top = windowState.Top;
            this.Width = windowState.Width;
            this.Height = windowState.Height;
            this.Topmost = windowState.AlwaysOnTop;
            this.currentSorting = windowState.SortingOrder;
        }

        private void SaveMainWindowState() {
            try {
                string jsonString = JsonSerializer.Serialize(new WindowState() { 
                    Left = this.Left,
                    Top = this.Top,
                    Width = this.ActualWidth,
                    Height = this.ActualHeight,
                    AlwaysOnTop = this.Topmost,
                    SortingOrder = this.currentSorting }, new JsonSerializerOptions() { WriteIndented = true });
                File.WriteAllText(MAIN_WINDOW_STATE_FILEPATH, jsonString);
            }
            catch (Exception) {

            }
        }

        private void Sort_Click(object sender, RoutedEventArgs e) {

            SortingTypes sortBy;

            if (currentSorting >= Enum.GetValues(typeof(SortingTypes)).Cast<SortingTypes>().Max())
                sortBy = (SortingTypes)0;
            else {
                sortBy = currentSorting + 1;
            }

            currentSorting = sortBy;

            SortList(sortBy);
        }

        
    }
}
