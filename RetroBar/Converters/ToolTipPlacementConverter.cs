using System;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using ManagedShell.AppBar;
using RetroBar.Utilities;

namespace RetroBar.Converters
{
    [ValueConversion(typeof(bool), typeof(PlacementMode))]
    public class ToolTipPlacementConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Edge of the taskbar hosting the placement target, when it is passed
            AppBarEdge edge = TaskbarContext.GetEdge(TaskbarContext.FindElement(value));

            if (edge == AppBarEdge.Left)
            {
                return PlacementMode.Right;
            }
            else if (edge == AppBarEdge.Right)
            {
                return PlacementMode.Left;
            }
            else if (edge == AppBarEdge.Top)
            {
                return PlacementMode.Bottom;
            }
            else
            {
                return PlacementMode.Top;
            }
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
