using ManagedShell.AppBar;
using RetroBar.Utilities;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace RetroBar.Converters
{
    [ValueConversion(typeof(double), typeof(double))]
    public class ToolTipVerticalOffsetConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.FirstOrDefault(v => v == DependencyProperty.UnsetValue) != null)
            {
                return double.NaN;
            }

            // Edge and scale of the taskbar hosting the placement target, when it is passed
            DependencyObject target = TaskbarContext.FindElement(values);

            if (TaskbarContext.IsVertical(target))
            {
                double placementTargetHeight = (double)values[0];
                double toolTipHeight = (double)values[1];
                double scale = target != null && "taskbar".Equals(parameter) ? TaskbarContext.GetScale(target) : (double)values[2];
                return (placementTargetHeight / 2.0) - (toolTipHeight / 2.0 / scale);
            }
            else
            {
                return (double)0;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
