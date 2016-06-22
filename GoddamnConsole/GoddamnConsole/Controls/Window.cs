using System;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class Window : ContentControl
    {
        private WindowAlignment _horizontalAlignment;
        private WindowAlignment _verticalAlignment;
        private string _title;

        public override int MaxHeight => (Content?.ActualHeight ?? 0) + 2;
        public override int MaxWidth => (Content?.ActualWidth ?? 0) + 2;

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
            dc.DrawFrame(new Rectangle(0, 0, ActualWidth, ActualHeight), new FrameOptions {Style = style});
            var truncated = Title.Length + 2 > ActualWidth - 4
                                ? ActualWidth < 9
                                      ? string.Empty
                                      : $" {Title.Remove(ActualWidth - 9)}... "
                                : ActualWidth < 9
                                      ? string.Empty
                                      : $" {Title} ";
            dc.DrawText(new Point(2, 0), truncated);
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
