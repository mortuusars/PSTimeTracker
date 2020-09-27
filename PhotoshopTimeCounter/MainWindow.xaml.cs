using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace PhotoshopTimeCounter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string MAIN_WINDOW_STATE_FILENAME = "mainWindowState.";
        private string MAIN_WINDOW_STATE_FILEPATH = App.APP_FOLDER_PATH + MAIN_WINDOW_STATE_FILENAME;

        public MainWindow() {
            InitializeComponent();

            //TODO: Always on top

            WindowState windowState = LoadMainWindowState();
            this.Left = windowState.Left;
            this.Top = windowState.Top;
            this.Topmost = windowState.AlwaysOnTop;
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

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
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

        private void SaveMainWindowState() {
            try {
                string jsonString = JsonSerializer.Serialize(new WindowState() { Left = this.Left, Top = this.Top, AlwaysOnTop = this.Topmost }, new JsonSerializerOptions() { WriteIndented = true });
                File.WriteAllText(MAIN_WINDOW_STATE_FILEPATH, jsonString);
            }
            catch (Exception) {

            }
        }
    }
}
