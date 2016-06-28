using System;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class ContentWindow : WindowBase, IContentControl
    {
        public override Size BoundingBoxReduction => new Size(2, 2);

        public override ParentControl Parent
        {
            get { return null; }
            set { throw new NotSupportedException(); }
        }

        //public override int MaxHeight
        //{
        //    get
        //    {
        //        if (Content == null) return 2;
        //        if (Content.Height.Type == ControlSizeType.BoundingBoxSize) return 2;
        //        return Content.ActualHeight + 2;
        //    }
        //}

        //public override int MaxWidth
        //{
        //    get
        //    {
        //        if (Content == null) return 2;
        //        if (Content.Width.Type == ControlSizeType.BoundingBoxSize) return 2;
        //        return Content.ActualWidth + 2;
        //    }
        //}

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

        public override Point GetScrollOffset(Control child)
        {
            return new Point(0, 0);
        }

        public override bool IsChildVisible(Control child)
        {
            return true;
        }

        private Control _content;
        public Control Content
        {
            get { return _content; }
            set
            {
                if (_content != null)
                {
                    var pc = _content;
                    _content = null;
                    ContentDetached?.Invoke(this, new ChildRemovedEventArgs(pc));
                }
                if (value == null || value.Parent == this) _content = value;
                else value.Parent = this;
                OnPropertyChanged();
            }
        }
        public event EventHandler<ChildRemovedEventArgs> ContentDetached;
    }
}
