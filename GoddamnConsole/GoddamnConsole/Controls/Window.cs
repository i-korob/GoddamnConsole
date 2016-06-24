using System;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class Window : ContentControl
    {
        private WindowAlignment _horizontalAlignment;
        private WindowAlignment _verticalAlignment;
        private string _title;

        public override int MaxHeight
        {
            get
            {
                if (Content == null) return 2;
                if (Content.Height.Type == ControlSizeType.BoundingBoxSize) return 2;
                else return Content.ActualHeight + 2;
            }
        }

        public override int MaxWidth
        {
            get
            {
                if (Content == null) return 2;
                if (Content.Width.Type == ControlSizeType.BoundingBoxSize) return 2;
                else return Content.ActualWidth + 2;
            }
        }

        public override ParentControl Parent
        {
            get { return null; }
            set { throw new NotSupportedException(); }
        }

        public WindowAlignment VerticalAlignment
        {
            get { return _verticalAlignment; }
            set { _verticalAlignment = value; OnPropertyChanged(); }
        }

        public WindowAlignment HorizontalAlignment
        {
            get { return _horizontalAlignment; }
            set { _horizontalAlignment = value; OnPropertyChanged(); }
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged(); }
        }

        protected override void OnRender(DrawingContext dc)
        {
            var style = Console.FocusedWindow == this ? FrameStyle.Double : FrameStyle.Single;
            dc.DrawFrame(new Rectangle(0, 0, ActualWidth, ActualHeight), new FrameOptions
            {
                Style = style,
                Foreground = Foreground,
                Background = Background
            });
            var truncated = Title.Length + 2 > ActualWidth - 4
                                ? ActualWidth < 9
                                      ? string.Empty
                                      : $" {Title.Remove(ActualWidth - 9)}... "
                                : ActualWidth < 9
                                      ? string.Empty
                                      : $" {Title} ";
            dc.DrawText(new Point(2, 0), truncated, new TextOptions
            {
                Foreground = Foreground,
                Background = Background
            });
        }

        public override Rectangle MeasureBoundingBox(Control child)
        {
            return new Rectangle(1, 1, ActualWidth - 2, ActualHeight - 2);
        }
    }

    public enum WindowAlignment
    {
        Begin,
        End,
        Center
    }
}
