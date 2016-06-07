using System;
using System.Collections.Generic;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public abstract class Control
    {
        private Control _parent;

        private void OmfgIamThrownOut(object sender, Control ctrl)
        {
            if (sender != _parent || ctrl != this) return;
            var cctl = sender as IContentControl;
            if (cctl != null && cctl.Content != this)
            {
                cctl.ContentDetached -= OmfgIamThrownOut;
            }
            else
            {
                var pctl = sender as IPedophileControl;
                if (pctl != null)
                {
                    pctl.ChildRemoved -= OmfgIamThrownOut;
                }
                else throw new Exception("Parent can not have children (wat)"); // how this shit could happen?
            }
            try
            {
                DetachedFromParent?.Invoke(this, EventArgs.Empty);
            }
            catch { /* */ }
        }

        public Control Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                var cctl = value as IContentControl;
                if (cctl != null && cctl.Content != this)
                {
                    cctl.ContentDetached += OmfgIamThrownOut;
                    cctl.Content = this;
                }
                else
                {
                    var pctl = value as IPedophileControl;
                    if (pctl != null)
                    {
                        pctl.ChildRemoved += OmfgIamThrownOut;
                        pctl.Children.Add(this);
                    }
                    else throw new NotSupportedException($"{value.GetType().Name} can not have child");
                }
                _parent = value;
            }
        }

        public event EventHandler DetachedFromParent;

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

        public int Width { get; set; }

        public int ActualWidth =>
            (_parent as IParentControl)?.MeasureChild(this)?.Width ??
            (Console.Root == this ? Console.WindowWidth : 0);
        public int Height { get; set; }
        public int ActualHeight =>
            (_parent as IParentControl)?.MeasureChild(this)?.Height ??
            (Console.Root == this ? Console.WindowHeight : 0);
    }

    public interface IParentControl
    {
        Size MeasureChild(Control child);
    }

    public interface IContentControl : IParentControl
    {
        Control Content { get; set; }
        event EventHandler<Control> ContentDetached;
    }

    public interface IPedophileControl : IParentControl
    {
        ICollection<Control> Children { get; }
        event EventHandler<Control> ChildRemoved;
    }
}
