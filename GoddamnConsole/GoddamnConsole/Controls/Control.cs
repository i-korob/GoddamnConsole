using System;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public abstract class Control
    {
        internal void OnKeyPressInternal(ConsoleKeyInfo key)
        {
            OnKeyPress(key);
        }

        internal void OnRenderInternal(DrawingContext context)
        {
            OnRender(context);
        }

        protected virtual void OnKeyPress(ConsoleKeyInfo key) { }
        protected virtual void OnRender(DrawingContext context) { }
    }
}
