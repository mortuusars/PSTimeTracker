using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
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

        public bool AlwaysOnTop
        {
            get { return alwaysOnTop; }
            set {
                alwaysOnTop = value;
                this.Topmost = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AlwaysOnTop)));
            }
        }

        public bool IsHeightResized { get; set; }

        public string CurrentSortingString { get => $"Sorted by: {currentSorting}"; }

        private bool alwaysOnTop;
        private SortingTypes currentSorting;


        #region Dragging

        private double previousMouseY = double.NaN;

        private bool _isDragging;

        public bool IsDragging
        {
            get { return _isDragging; }
            set {
                if (value == false)
                    Mouse.Capture(null);

                _isDragging = value;
            }
        }


        #endregion

        public MainView()
        {
            InitializeComponent();

            this.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    MainListView.SelectedItem = null;
                    e.Handled = true;
                }
            };
        }

        #region Events

        private void HeaderContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            this.MainListView.SelectedItem = null;
        }

        #endregion


        #region Resising Window

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private enum ResizeDirection { Left = 61441, Right = 61442, Top = 61443, Bottom = 61446, BottomRight = 61448, }

        // Sides
        private void LeftResizeBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            SendMessage(hwndSource.Handle, 0x112, (IntPtr)ResizeDirection.Left, IntPtr.Zero);
            SetAutoHeight();
        }
        private void RightResizeBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            SendMessage(hwndSource.Handle, 0x112, (IntPtr)ResizeDirection.Right, IntPtr.Zero);
            SetAutoHeight();
        }

        private void SideResizeBorder_MouseEnter(object sender, MouseEventArgs e) => Mouse.OverrideCursor = Cursors.SizeWE;

        // Bottom
        private void BottomResizeBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainListView.MaxHeight = this.Height;
            IsDragging = true;
            previousMouseY = e.GetPosition(MainListView).Y;
            DragResizeBottom();
        }

        private void BottomResizeBorder_MouseMove(object sender, MouseEventArgs e)
        {
            DragResizeBottom();
        }

        private void DragResizeBottom()
        {
            if (IsDragging)
            {
                Mouse.Capture(BottomResizeBorder);

                var currentMouseY = Mouse.GetPosition(MainListView).Y;

                var oldHeight = MainListView.ActualHeight;

                var offset = currentMouseY - previousMouseY;

                var newHeight = oldHeight += offset;

                if (newHeight > 0)
                    MainListView.Height = newHeight;

                previousMouseY = currentMouseY;

                if (Mouse.LeftButton == MouseButtonState.Released)
                    IsDragging = false;

            }
        }

        private void BottomResizeBorder_MouseRightButtonDown(object sender, MouseButtonEventArgs e) => SetAutoHeight();

        private void BottomResizeBorder_MouseEnter(object sender, MouseEventArgs e) => Mouse.OverrideCursor = Cursors.SizeNS;

        private void ResizeBorder_MouseLeave(object sender, MouseEventArgs e) => Mouse.OverrideCursor = Cursors.Arrow;

        private void SetAutoHeight()
        {
            this.MainListView.MaxHeight = 600;
            this.MainListView.Height = double.NaN;
        }

        #endregion


        #region Sorting


        private void SortList(SortingTypes sortBy)
        {
            MainListView.Items.SortDescriptions.Clear();

            // TODO: Sorting in viewmodel
            switch (sortBy)
            {
                case SortingTypes.NameABC:
                    MainListView.Items.SortDescriptions.Add(new SortDescription(nameof(PsFile.FileName), ListSortDirection.Ascending));
                    break;
                case SortingTypes.NameZYX:
                    MainListView.Items.SortDescriptions.Add(new SortDescription(nameof(PsFile.FileName), ListSortDirection.Descending));
                    break;
                case SortingTypes.TimeShorter:
                    MainListView.Items.SortDescriptions.Add(new SortDescription(nameof(PsFile.TrackedSeconds), ListSortDirection.Ascending));
                    break;
                case SortingTypes.TimeLonger:
                    MainListView.Items.SortDescriptions.Add(new SortDescription(nameof(PsFile.TrackedSeconds), ListSortDirection.Descending));
                    break;
                case SortingTypes.FirstOpened:
                    MainListView.Items.SortDescriptions.Add(new SortDescription(nameof(PsFile.FirstActiveTime), ListSortDirection.Ascending));
                    break;
                case SortingTypes.LastOpened:
                    MainListView.Items.SortDescriptions.Add(new SortDescription(nameof(PsFile.FirstActiveTime), ListSortDirection.Descending));
                    break;
            }

            //SortingPopup.Text = sortBy.ToString();
            currentSorting = sortBy;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentSortingString)));
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

        







        #endregion

        
    }
}
