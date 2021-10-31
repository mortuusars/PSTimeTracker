using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using PropertyChanged;

namespace PSTimeTracker
{
    [AddINotifyPropertyChangedInterface]
    public partial class MainView : Window
    {
        public bool IsMenuOpen { get; set; }

        private static double _defaultMaxHeight = 1050;
        private static readonly DispatcherTimer _clampAutoSizeTimer = new();

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

            this.Deactivated += (s, e) => IsMenuOpen = false;

            LoadState();
        }

        public void SaveWindowState()
        {
            new MainViewState()
            {
                Top = this.Top,
                Left = this.Left,
                Width = this.Width,
                Height = this.Height,
                MaxHeight = this.MaxHeight,
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
            this.MaxHeight = state.MaxHeight;

            if (SizeToContent != SizeToContent.Height)
                this.Height = state.Height;
            else
                this.Height = double.NaN;
        }

        private void HeaderContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveWindowState();
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
            e.Handled = true;
            var sizing = this.SizeToContent;
            StartResize((Visual)sender, ResizeDirection.Left);
            this.SizeToContent = sizing;
            SaveWindowState();
        }

        private void RightResizeBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var sizing = this.SizeToContent;
            StartResize((Visual)sender, ResizeDirection.Right);
            this.SizeToContent = sizing;
            SaveWindowState();
        }

        private void BottomResizeBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            this.MaxHeight = _defaultMaxHeight;
            StartResize((Visual)sender, ResizeDirection.Bottom);
            SaveWindowState();

            clampAutoSizeButton.Visibility = Visibility.Visible;
            clampAutoSizeButton.IsVisible = true;
            HideClampButtonAfter(2);
        }

        private void HideClampButtonAfter(int seconds)
        {
            _clampAutoSizeTimer.Stop();
            _clampAutoSizeTimer.Interval = TimeSpan.FromSeconds(seconds);
            _clampAutoSizeTimer.Start();
            _clampAutoSizeTimer.Tick += (s, e) =>
            {
                _clampAutoSizeTimer.Stop();
                clampAutoSizeButton.IsVisible = false;
            };
        }

        private void SetAutoHeight()
        {
            clampAutoSizeButton.IsVisible = false;
            SizeToContent = SizeToContent.Height;
            SaveWindowState();
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
            SaveWindowState();
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

        private void clampAutoSizeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            clampAutoSizeButton.Visibility = Visibility.Collapsed;

            this.MaxHeight = this.Height;
            SetAutoHeight();
        }

        private void clampAutoSizeButton_MouseEnter(object sender, MouseEventArgs e)
        {
            _clampAutoSizeTimer.Stop();
        }

        private void clampAutoSizeButton_MouseLeave(object sender, MouseEventArgs e)
        {
            HideClampButtonAfter(1);
        }
    }
}
