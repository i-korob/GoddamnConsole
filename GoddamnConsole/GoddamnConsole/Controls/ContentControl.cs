using System;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public abstract class ContentControl : Control, IContentControl
    {
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

        public virtual Rectangle MeasureBoundingBox(Control child) 
            => new Rectangle(0, 0, ActualWidth, ActualHeight);

        public virtual Point GetScrollOffset(Control child) => new Point(0, 0);
                
        public virtual bool IsChildVisible(Control child) => true;
    }
}
