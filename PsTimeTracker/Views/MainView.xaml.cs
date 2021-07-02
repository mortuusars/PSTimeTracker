using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace PSTimeTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {

        public double MaxListHeight { get; set; }

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

            this.MouseLeftButtonDown += (s, e) =>
            {
                if (MenuContrainer.IsMouseOver == false)
                    MenuContrainer.Visibility = Visibility.Hidden;
            };

            //MainListView.LostFocus += (s, e) => MainListView.SelectedItems.Clear();

            MaxListHeight = MainListView.MaxHeight;
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
            //SetAutoHeight();
        }
        private void RightResizeBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            SendMessage(hwndSource.Handle, 0x112, (IntPtr)ResizeDirection.Right, IntPtr.Zero);
            //SetAutoHeight();
        }

        private void SideResizeBorder_MouseEnter(object sender, MouseEventArgs e) => Mouse.OverrideCursor = Cursors.SizeWE;

        // Bottom
        private void BottomResizeBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainListView.MaxHeight = this.Height;
            IsDragging = true;
            previousMouseY = e.GetPosition(MainListView).Y;
            DragResizeBottom(e);
        }

        private void BottomResizeBorder_MouseMove(object sender, MouseEventArgs e)
        {
            DragResizeBottom(e);
        }

        private void DragResizeBottom(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                return;

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

        private void BottomResizeBorder_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetAutoHeight();
        }

        private void BottomResizeBorder_MouseEnter(object sender, MouseEventArgs e) => Mouse.OverrideCursor = Cursors.SizeNS;

        private void ResizeBorder_MouseLeave(object sender, MouseEventArgs e) => Mouse.OverrideCursor = Cursors.Arrow;

        private void SetAutoHeight()
        {
            this.MainListView.MaxHeight = MaxListHeight;
            this.MainListView.Height = double.NaN;
        }



        #endregion

        private void MenuSortingButton_Click_1(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            ContextMenu contextMenu = btn.ContextMenu;
            contextMenu.PlacementTarget = btn;
            contextMenu.IsOpen = true;
            e.Handled = true;
        }
    }
}
