using RetroBar.Utilities;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RetroBar.Converters
{
    // The screen edge of the taskbar hosting an element (e.g. a popup's PlacementTarget)
    public class TaskbarEdgeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TaskbarContext.GetEdge(value as DependencyObject);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
