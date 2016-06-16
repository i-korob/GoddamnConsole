using System;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public abstract class ContentControl : Control, IContentControl
    {
        private Control _content;
        public abstract Size MeasureChild(Control child);

        public Control Content
        {
            get { return _content; }
            set
            {
                if (_content != null)
                {
                    var pc = _content;
                    _content = null;
                    ContentDetached?.Invoke(this, pc);
                }
                if (value == null || value.Parent == this) _content = value;
                else value.Parent = this;
            }
        }

        public event EventHandler<Control> ContentDetached;
    }
}
