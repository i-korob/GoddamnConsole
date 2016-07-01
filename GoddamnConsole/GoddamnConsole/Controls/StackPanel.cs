using System;
using System.Linq;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class StackPanel : ChildrenControl
    {
        private StackPanelOrientation _orientation = StackPanelOrientation.Vertical;

        public override Rectangle MeasureBoundingBox(Control child)
        {
            if (Orientation == StackPanelOrientation.Vertical)
            {
                var yofs =
                    Children.TakeWhile(x => x != child)
                            .Sum(x => x.Height.Type == ControlSizeType.BoundingBoxSize ? 0 : x.ActualHeight);
                if (yofs > ActualHeight) return new Rectangle(0, 0, 0, 0);
                return
                    new Rectangle(
                        0,
                        yofs,
                        Math.Min(
                            ActualWidth,
                            child.Width.Type == ControlSizeType.BoundingBoxSize
                                ? ActualWidth
                                : child.ActualWidth),
                        Math.Min(ActualHeight - yofs,
                                 child.Height.Type == ControlSizeType.BoundingBoxSize
                                     ? 0
                                     : child.ActualHeight));
            }
            var xofs =
                Children.TakeWhile(x => x != child)
                        .Sum(x => x.Width.Type == ControlSizeType.BoundingBoxSize ? 0 : x.ActualWidth);
            if (xofs > ActualWidth) return new Rectangle(0, 0, 0, 0);
            return
                new Rectangle(
                    xofs,
                    0,
                    Math.Min(
                        ActualWidth - xofs,
                        child.Width.Type == ControlSizeType.BoundingBoxSize
                            ? 0
                            : child.ActualWidth),
                    Math.Min(ActualHeight,
                             child.Height.Type == ControlSizeType.BoundingBoxSize
                                 ? ActualHeight
                                 : child.ActualHeight));
        }

        public StackPanelOrientation Orientation
        {
            get { return _orientation; }
            set { _orientation = value; OnPropertyChanged(); }
        }
    }

    public enum StackPanelOrientation
    {
        Horizontal,
        Vertical
    }
}
