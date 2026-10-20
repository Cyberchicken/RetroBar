using System;
using System.Windows.Data;

namespace RetroBar.Converters
{
    // Computes an individual task button width so a full row fills the taskbar exactly.
    // Every button gets the base (floored) width; the pixels lost to flooring are spread
    // evenly across the row's columns, each affected column getting one extra pixel.
    //
    // Distribution is keyed on the column (index % buttonsPerRow) so widths line up in
    // columns across rows, and a partial last row keeps the same per-column widths.
    //
    // values[0] = item index (ItemsControl.AlternationIndex)
    // values[1] = base button width (double)
    // values[2] = number of columns that get an extra pixel (int)
    // values[3] = buttons per row (int)
    //
    // If the index isn't available the base width is returned, so the layout simply falls
    // back to uniform widths rather than breaking.
    public class TaskButtonWidthConverter : IMultiValueConverter
    {
        // Shift of each row, as a fraction of a button, like the letter rows of a keyboard:
        // none, then 1/6 back, then 1/6 forward, so that every button stays at least 5/6 wide.
        private static readonly double[] StaggerPattern = { 0, 1.0 / 6, -1.0 / 6 };

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 4 || !(values[1] is double baseWidth))
            {
                return Binding.DoNothing;
            }

            double width = baseWidth;

            if (values[0] is int index
                && values[2] is int extraCount
                && values[3] is int buttonsPerRow
                && extraCount > 0
                && buttonsPerRow > 0)
            {
                int column = index % buttonsPerRow;

                // Evenly spaced "+1"s: 1 when this column falls on a distribution boundary.
                int extraForColumn = (column + 1) * extraCount / buttonsPerRow - column * extraCount / buttonsPerRow;

                width = baseWidth + extraForColumn;
            }

            // values[4] = stagger rows (bool), values[5] = number of buttons (int):
            // like keyboard rows, shift the button boundaries of each row without moving the row's edges,
            // by making its first button narrower and its last one wider by the same amount.
            if (values.Length >= 6
                && values[4] is bool stagger && stagger
                && values[0] is int i
                && values[3] is int perRow && perRow > 1
                && values[5] is int count && count > perRow)
            {
                int row = i / perRow;
                int column = i % perRow;
                double shift = StaggerPattern[row % StaggerPattern.Length];
                bool rowIsFull = (row + 1) * perRow <= count;

                if (rowIsFull)
                {
                    // Full row: first button narrower (or wider), last one wider (or narrower) by the same amount
                    double offset = Math.Round(baseWidth * shift);

                    if (column == 0)
                    {
                        width -= offset;
                    }
                    else if (column == perRow - 1)
                    {
                        width += offset;
                    }
                }
                else if (column == 0 && shift != 0)
                {
                    // Partial (last) row: it has at least one button of free space, so only widen the first button.
                    // Widening by (1 - shift) gives the same boundaries as narrowing by shift.
                    width += Math.Round(baseWidth * (shift > 0 ? 1 - shift : -shift));
                }
            }

            return width;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
