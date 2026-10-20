using ManagedShell.AppBar;
using RetroBar.Utilities;
using System.Windows.Controls;
using System;
using System.Windows.Data;
using System.Globalization;

namespace RetroBar.Converters
{
    public class IconListOrientation : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Orientation of the hosting window (can differ per monitor)
            bool horizontal = values.Length > 0 && values[0] is Orientation windowOrientation ? windowOrientation == Orientation.Horizontal : Settings.Instance.Edge is AppBarEdge.Top or AppBarEdge.Bottom;
            // Rows of the hosting window (can differ per monitor)
            int rows = values.Length > 1 && values[1] is int windowRows ? windowRows : Settings.Instance.RowCount;

            if (horizontal && rows > 1)
            {
                return Orientation.Vertical;
            }

            return Orientation.Horizontal;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
