using System;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class Border : Control, IContentControl
    {
        public override void Render(DrawingContext context)
        {
            context.DrawFrame(new Rectangle(0, 0, ActualWidth, ActualHeight));
            Content.Render(context.Shrink(new Rectangle(1, 1, ActualWidth - 2, ActualHeight - 2)));
        }

        public Size MeasureChild(Control child)
        {
            return new Size(Math.Min(child.Width, ActualWidth - 2),
                Math.Min(child.Height, ActualHeight - 2));
        }

        private Control _content;

        public Control Content
        {
            get { return _content; }
            set
            {
                if (_content != null)
                    ContentDetached?.Invoke(this, _content);
                if (value != null && value.Parent != this) value.Parent = this;
                _content = value;
            }
        }
        public event EventHandler<Control> ContentDetached;
    }
}
