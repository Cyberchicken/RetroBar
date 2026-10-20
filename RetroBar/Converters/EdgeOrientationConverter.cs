using System;
using System.Windows.Controls;
using System.Windows.Data;
using ManagedShell.AppBar;
using RetroBar.Utilities;

namespace RetroBar.Converters
{
    [ValueConversion(typeof(bool), typeof(Orientation))]
    public class EdgeOrientationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Bound to the hosting window's orientation, which can differ per monitor
            if (value is Orientation windowOrientation)
            {
                return windowOrientation;
            }

            if (Settings.Instance.Edge == AppBarEdge.Left || Settings.Instance.Edge == AppBarEdge.Right)
            {
                return Orientation.Vertical;
            }
            else
            {
                return Orientation.Horizontal;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
