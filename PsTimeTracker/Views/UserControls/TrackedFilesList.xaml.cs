using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace PSTimeTracker.Views.UserControls
{
    public partial class TrackedFilesList : UserControl
    {
        public IList SelectedItemsList
        {
            get { return (IList)GetValue(SelectedItemsListProperty); }
            set { SetValue(SelectedItemsListProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsListProperty =
            DependencyProperty.Register("SelectedItemsList", typeof(object), typeof(TrackedFilesList), new PropertyMetadata(null));

        public TrackedFilesList()
        {
            InitializeComponent();
            MainListView.SelectionChanged += (s, e) => SelectedItemsList = MainListView.SelectedItems;
        }

        public void ClearSelection()
        {
            MainListView.UnselectAll();
        }
    }
}
