using ManagedShell.AppBar;
using RetroBar.Utilities;
using System;
using System.Windows;
using System.Windows.Controls;

namespace RetroBar.Controls
{
    /// <summary>
    /// A horizontal WrapPanel that, on a multi-row horizontal taskbar, wraps after just enough columns
    /// to fit its items into the given number of rows. Items then fill row by row, left to right.
    /// It stays a WrapPanel so that drag and drop keeps treating it as a horizontal list.
    /// </summary>
    public class RowWrapPanel : WrapPanel
    {
        public static readonly DependencyProperty RowsProperty = DependencyProperty.Register(nameof(Rows), typeof(int), typeof(RowWrapPanel),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public int Rows
        {
            get => (int)GetValue(RowsProperty);
            set => SetValue(RowsProperty, value);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            bool horizontalTaskbar = !TaskbarContext.IsVertical(this);

            // On a vertical taskbar a vertical panel fills column by column: wrap after enough rows for the columns that fit
            if (Orientation == Orientation.Vertical && !horizontalTaskbar)
            {
                return MeasureColumnsFirst(constraint);
            }

            if (Orientation != Orientation.Horizontal || !horizontalTaskbar || Rows < 2)
            {
                return base.MeasureOverride(constraint);
            }

            double maxItemWidth = 0;
            int visibleItems = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child == null || child.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                maxItemWidth = Math.Max(maxItemWidth, child.DesiredSize.Width);
                visibleItems++;
            }

            if (visibleItems == 0)
            {
                return base.MeasureOverride(constraint);
            }

            int columns = (int)Math.Ceiling((double)visibleItems / Rows);
            // Small tolerance so that rounding does not push the last item of a row onto the next one
            double width = Math.Min(constraint.Width, columns * maxItemWidth + 0.5);

            return base.MeasureOverride(new Size(width, constraint.Height));
        }

        private Size MeasureColumnsFirst(Size constraint)
        {
            double maxItemWidth = 0;
            double maxItemHeight = 0;
            int visibleItems = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child == null || child.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                maxItemWidth = Math.Max(maxItemWidth, child.DesiredSize.Width);
                maxItemHeight = Math.Max(maxItemHeight, child.DesiredSize.Height);
                visibleItems++;
            }

            if (visibleItems == 0 || maxItemWidth <= 0 || double.IsInfinity(constraint.Width))
            {
                return base.MeasureOverride(constraint);
            }

            int columns = Math.Max(1, (int)Math.Floor(constraint.Width / maxItemWidth));
            int rows = (int)Math.Ceiling((double)visibleItems / columns);
            // Small tolerance so that rounding does not push the last item of a column onto the next one
            double height = Math.Min(constraint.Height, rows * maxItemHeight + 0.5);

            return base.MeasureOverride(new Size(constraint.Width, height));
        }
    }
}
