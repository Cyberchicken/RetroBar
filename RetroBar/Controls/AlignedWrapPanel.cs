using RetroBar.Utilities;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RetroBar.Controls
{
    /// <summary>
    /// A WrapPanel that aligns each line (row, or column when vertical) at the start, center or end of the free space.
    /// It stays a WrapPanel so that drag and drop keeps treating it as one.
    /// </summary>
    public class AlignedWrapPanel : WrapPanel
    {
        public static readonly DependencyProperty LineAlignmentProperty = DependencyProperty.Register(nameof(LineAlignment), typeof(TaskButtonsAlignmentOption), typeof(AlignedWrapPanel),
            new FrameworkPropertyMetadata(TaskButtonsAlignmentOption.Start, FrameworkPropertyMetadataOptions.AffectsArrange));

        public TaskButtonsAlignmentOption LineAlignment
        {
            get => (TaskButtonsAlignmentOption)GetValue(LineAlignmentProperty);
            set => SetValue(LineAlignmentProperty, value);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (LineAlignment == TaskButtonsAlignmentOption.Start)
            {
                return base.ArrangeOverride(finalSize);
            }

            bool horizontal = Orientation == Orientation.Horizontal;
            double lineLimit = horizontal ? finalSize.Width : finalSize.Height;
            double factor = LineAlignment == TaskButtonsAlignmentOption.Center ? 0.5 : 1.0;

            List<UIElement> line = new List<UIElement>();
            double lineLength = 0;
            double lineThickness = 0;
            double lineOffset = 0;

            void ArrangeLine()
            {
                double position = Math.Max(0, (lineLimit - lineLength) * factor);

                foreach (UIElement child in line)
                {
                    Size size = child.DesiredSize;
                    child.Arrange(horizontal
                        ? new Rect(position, lineOffset, size.Width, lineThickness)
                        : new Rect(lineOffset, position, lineThickness, size.Height));
                    position += horizontal ? size.Width : size.Height;
                }

                lineOffset += lineThickness;
                line.Clear();
                lineLength = 0;
                lineThickness = 0;
            }

            foreach (UIElement child in InternalChildren)
            {
                if (child == null)
                {
                    continue;
                }

                Size size = child.DesiredSize;
                double length = horizontal ? size.Width : size.Height;
                double thickness = horizontal ? size.Height : size.Width;

                // Same wrapping rule as WrapPanel: start a new line when this item does not fit
                if (line.Count > 0 && lineLength + length > lineLimit + 0.5)
                {
                    ArrangeLine();
                }

                line.Add(child);
                lineLength += length;
                lineThickness = Math.Max(lineThickness, thickness);
            }

            if (line.Count > 0)
            {
                ArrangeLine();
            }

            return finalSize;
        }
    }
}
