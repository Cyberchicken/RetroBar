using RetroBar.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RetroBar.Controls
{
    /// <summary>
    /// Interaction logic for ToolbarButton.xaml
    /// </summary>
    public partial class ToolbarButton : UserControl
    {
        public const int DefaultIconSize = 16;

        // Icon size in pixels; the default size keeps the theme's own icon style.
        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(nameof(IconSize), typeof(int), typeof(ToolbarButton),
            new PropertyMetadata(DefaultIconSize, OnIconSizeChanged));

        public int IconSize
        {
            get => (int)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        // Scale of the hosting taskbar, used to pick a sharp enough icon
        public static readonly DependencyProperty IconScaleProperty = DependencyProperty.Register(nameof(IconScale), typeof(double), typeof(ToolbarButton),
            new PropertyMetadata(1.0, OnIconSizeChanged));

        public double IconScale
        {
            get => (double)GetValue(IconScaleProperty);
            set => SetValue(IconScaleProperty, value);
        }

        public ToolbarButton()
        {
            InitializeComponent();

            setIconBinding();
        }

        private static void OnIconSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is ToolbarButton button)
            {
                button.setIconBinding();
            }
        }

        private void setIconBinding()
        {
            int size = IconSize;

            if (size == DefaultIconSize)
            {
                ToolbarIcon.ClearValue(WidthProperty);
                ToolbarIcon.ClearValue(HeightProperty);
            }
            else
            {
                ToolbarIcon.Width = size;
                ToolbarIcon.Height = size;
            }

            // Use the smallest system icon that is at least as large as the shown size, so it stays sharp
            double pixels = size * IconScale;
            string bindingPath = "SmallIcon";
            bool useLargeIcons = IconScale > 1 || (Application.Current.FindResource("UseLargeIcons") as bool? ?? false);

            if (pixels > 48)
            {
                bindingPath = "JumboIcon";
            }
            else if (pixels > 32)
            {
                bindingPath = "ExtraLargeIcon";
            }
            else if (useLargeIcons || pixels > 16)
            {
                bindingPath = "LargeIcon";
            }

            Binding iconBinding = new Binding(bindingPath);
            iconBinding.Mode = BindingMode.OneWay;
            ToolbarIcon.SetBinding(Image.SourceProperty, iconBinding);
        }
    }
}
