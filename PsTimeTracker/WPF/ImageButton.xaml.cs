using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PSTimeTracker.WPF
{
    public partial class ImageButton : UserControl
    {
        public static readonly DependencyProperty ActivateAlternativeStyleProperty =
            DependencyProperty.Register(nameof(ActivateAlternativeStyle), typeof(bool), typeof(ImageButton), new PropertyMetadata(false));

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(null));

        public static readonly DependencyProperty AlternativeSourceProperty =
            DependencyProperty.Register(nameof(AlternativeSource), typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(null));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ImageButton), new PropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(ImageButton), new PropertyMetadata(null));

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public bool ActivateAlternativeStyle
        {
            get { return (bool)GetValue(ActivateAlternativeStyleProperty); }
            set { SetValue(ActivateAlternativeStyleProperty, value); }
        }

        public ImageSource AlternativeSource
        {
            get { return (ImageSource)GetValue(AlternativeSourceProperty); }
            set { SetValue(AlternativeSourceProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public ImageButton()
        {
            InitializeComponent();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                Command?.Execute(CommandParameter);
        }
    }
}
