using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using PropertyChanged;

namespace PSTimeTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class MainView : Window
    {
        public bool IsMenuOpen { get; set; }

        public MainView()
        {
            InitializeComponent();

            this.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    TrackedFilesListControl.MainListView.SelectedItem = null;
                    e.Handled = true;
                }
            };

            this.MouseLeftButtonDown += (s, e) =>
            {
                if (IsMenuOpen && !MenuContrainer.IsMouseOver)
                    IsMenuOpen = false;
            };

            this.Deactivated += MainView_Deactivated;

            LoadState();
        }

        public void SaveState()
        {
            new MainViewState()
            {
                Top = this.Top,
                Left = this.Left,
                Width = this.Width,
                Height = this.Height,
                AlwaysOnTop = this.Topmost,
                SizeToContent = this.SizeToContent
            }.Save();
        }

        private void LoadState()
        {
            var state = MainViewState.Load();

            this.Top = state.Top;
            this.Left = state.Left;
            this.Width = state.Width;
            this.Topmost = state.AlwaysOnTop;
            this.SizeToContent = state.SizeToContent;

            if (SizeToContent != SizeToContent.Height)
                this.Height = state.Height;
            else
                this.Height = double.NaN;
        }

        private void HeaderContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            //this.MainListView.SelectedItem = null;
        }

        private void MainView_Deactivated(object sender, EventArgs e)
        {
            IsMenuOpen = false;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveState();
        }


        #region Resising Window

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private enum ResizeDirection { Left = 61441, Right = 61442, Top = 61443, Bottom = 61446, BottomRight = 61448, }

        private void StartResize(Visual visual, ResizeDirection direction)
        {
            var hwndSource = (HwndSource)PresentationSource.FromVisual(visual);
            SendMessage(hwndSource.Handle, 0x112, (IntPtr)direction, IntPtr.Zero);
        }

        private void LeftResizeBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var sizing = this.SizeToContent;
            StartResize((Visual)sender, ResizeDirection.Left);
            this.SizeToContent = sizing;
            e.Handled = true;
        }

        private void RightResizeBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var sizing = this.SizeToContent;
            StartResize((Visual)sender, ResizeDirection.Right);
            this.SizeToContent = sizing;
            e.Handled = true;
        }

        private void BottomResizeBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartResize((Visual)sender, ResizeDirection.Bottom);
            e.Handled = true;
        }

        private void BottomResizeBorder_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetAutoHeight();
            e.Handled = true;
        }

        private void SetAutoHeight()
        {
            SizeToContent = SizeToContent.Height;
        }

        #endregion

        private void MenuSortingButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                ContextMenu contextMenu = btn.ContextMenu;
                contextMenu.PlacementTarget = btn;
                contextMenu.IsOpen = true;
                e.Handled = true;
            }
        }

        private void Pin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Topmost = !this.Topmost;
            SaveState();
        }

        private void MinimizeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
            e.Handled = true;
        }

        private void CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
            e.Handled = true;
        }

        private void MenuButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsMenuOpen = !IsMenuOpen;
            e.Handled = true;
        }

        //private void mainView_PreviewDragEnter(object sender, DragEventArgs e)
        //{

        //}
    }
}
