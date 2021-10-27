using System.Windows;
using System.Windows.Controls;

namespace PSTimeTracker.Views.UserControls
{
    /// <summary>
    /// Interaction logic for TrackedFilesList.xaml
    /// </summary>
    public partial class TrackedFilesList : UserControl
    {
        public object SelectedFiles
        {
            get { return (object)GetValue(SelectedFilesProperty); }
            set { SetValue(SelectedFilesProperty, value); }
        }

        public static readonly DependencyProperty SelectedFilesProperty =
            DependencyProperty.Register("SelectedFiles", typeof(object), typeof(TrackedFilesList), new PropertyMetadata(null));


        public TrackedFilesList()
        {
            InitializeComponent();

            MainListView.SelectionChanged += MainListView_SelectionChanged;
        }

        private void MainListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedFiles = MainListView.SelectedItems;
        }
    }
}
