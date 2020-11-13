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
using PSTimeTracker.Core;
using PSTimeTracker.Models;

namespace PSTimeTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private const string MAIN_WINDOW_STATE_FILENAME = "mainWindowState.";
        private string MAIN_WINDOW_STATE_FILEPATH = App.APP_FOLDER_PATH + MAIN_WINDOW_STATE_FILENAME;

        public bool AlwaysOnTop
        {
            get { return alwaysOnTop; }
            set {
                alwaysOnTop = value;
                this.Topmost = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AlwaysOnTop)));
            }
        }

        public string CurrentSortingString { get => $"Sorted by: {currentSorting}"; }

        private bool alwaysOnTop;
        private SortingTypes currentSorting;


        public MainView()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            SetWindowFromState();

            MainItemsControl.Items.IsLiveSorting = true;

            SortList(currentSorting);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveMainWindowState();
            base.OnClosing(e);
        }

        #region Resising Window

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private enum ResizeDirection { Left = 61441, Right = 61442, Top = 61443, Bottom = 61446, BottomRight = 61448, }

        // Sides
        private void LeftResizeBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("LeftResizeBorderMouseDown");
            var hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            SendMessage(hwndSource.Handle, 0x112, (IntPtr)ResizeDirection.Left, IntPtr.Zero);
            this.SizeToContent = SizeToContent.Height;
        }
        private void RightResizeBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("RightResizeBorderMouseDown");
            var hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            SendMessage(hwndSource.Handle, 0x112, (IntPtr)ResizeDirection.Right, IntPtr.Zero);
            this.SizeToContent = SizeToContent.Height;
        }

        private void SideResizeBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.SizeWE;
        }

        // Bottom
        private void BottomResizeBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("BottomResizeBorderMouseDown");
            var hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            SendMessage(hwndSource.Handle, 0x112, (IntPtr)ResizeDirection.Bottom, IntPtr.Zero);
        }

        private void BottomResizeBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.SizeNS;
        }

        private void ResizeBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }
        #endregion


        #region Buttons

        private void SortList(SortingTypes sortBy)
        {
            MainItemsControl.Items.SortDescriptions.Clear();

            // TODO: Sorting in viewmodel
            switch (sortBy)
            {
                case SortingTypes.NameABC:
                    MainItemsControl.Items.SortDescriptions.Add(new SortDescription(nameof(PsFile.FileName), ListSortDirection.Ascending));
                    break;
                case SortingTypes.NameZYX:
                    MainItemsControl.Items.SortDescriptions.Add(new SortDescription(nameof(PsFile.FileName), ListSortDirection.Descending));
                    break;
                case SortingTypes.TimeShorter:
                    MainItemsControl.Items.SortDescriptions.Add(new SortDescription(nameof(PsFile.TrackedSeconds), ListSortDirection.Ascending));
                    break;
                case SortingTypes.TimeLonger:
                    MainItemsControl.Items.SortDescriptions.Add(new SortDescription(nameof(PsFile.TrackedSeconds), ListSortDirection.Descending));
                    break;
                case SortingTypes.FirstOpened:
                    MainItemsControl.Items.SortDescriptions.Add(new SortDescription(nameof(PsFile.FirstActiveTime), ListSortDirection.Ascending));
                    break;
                case SortingTypes.LastOpened:
                    MainItemsControl.Items.SortDescriptions.Add(new SortDescription(nameof(PsFile.FirstActiveTime), ListSortDirection.Descending));
                    break;
            }

            SortingPopup.Text = sortBy.ToString();
            currentSorting = sortBy;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentSortingString)));
        }

        private void AlwaysOnTop_Click(object sender, RoutedEventArgs e)
        {
            AlwaysOnTop = !AlwaysOnTop;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Sort_Click(object sender, RoutedEventArgs e)
        {

            SortingTypes sortBy;

            if (currentSorting >= Enum.GetValues(typeof(SortingTypes)).Cast<SortingTypes>().Max())
                sortBy = (SortingTypes)0;
            else
            {
                sortBy = currentSorting + 1;
            }

            currentSorting = sortBy;

            SortList(sortBy);
        }

        private void Sort_RightClick(object sender, MouseButtonEventArgs e)
        {
            SortingTypes sortBy;

            if (currentSorting <= Enum.GetValues(typeof(SortingTypes)).Cast<SortingTypes>().Min())
                sortBy = Enum.GetValues(typeof(SortingTypes)).Cast<SortingTypes>().Max();
            else
            {
                sortBy = currentSorting - 1;
            }

            SortList(sortBy);
        }


        #endregion


        #region State

        private MainViewState LoadMainWindowState()
        {
            try
            {
                string jsonString = File.ReadAllText(MAIN_WINDOW_STATE_FILEPATH);
                return JsonSerializer.Deserialize<MainViewState>(jsonString);
            }
            catch (Exception)
            {
                return new MainViewState();
            }
        }

        private void SetWindowFromState()
        {
            MainViewState windowState = LoadMainWindowState();
            this.Left = windowState.Left;
            this.Top = windowState.Top;
            this.Width = windowState.Width;
            this.Height = windowState.Height;
            AlwaysOnTop = windowState.AlwaysOnTop;
            currentSorting = windowState.SortingOrder;
        }

        public void SaveMainWindowState()
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(new MainViewState()
                {
                    Left = this.Left,
                    Top = this.Top,
                    Width = this.ActualWidth,
                    Height = this.ActualHeight,
                    AlwaysOnTop = this.Topmost,
                    SortingOrder = this.currentSorting
                }, new JsonSerializerOptions() { WriteIndented = true });
                File.WriteAllText(MAIN_WINDOW_STATE_FILEPATH, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot save window state: " + ex.Message, "PSTimerTracker", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        #endregion

        
    }
}
