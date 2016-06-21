using System;
using System.Collections.Generic;
using System.Linq;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public abstract partial class Control : IControl
    {
        private bool _focusable;
        private CharColor _foreground = CharColor.White;
        private CharColor _background = CharColor.Black;

        public Point CursorPosition
        {
            get { return new Point(Console.Provider.CursorX, Console.Provider.CursorY); }
            set
            {
                if (this == Console.Focused || this == Console.Popup)
                {
                    Console.Provider.CursorX = value.X;
                    Console.Provider.CursorY = value.Y;
                    OnPropertyChanged();
                }
            }
        }

        public bool Focusable
        {
            get { return _focusable; }
            protected set { _focusable = value; OnPropertyChanged(); }
        }

        public CharColor Foreground
        {
            get { return _foreground; }
            set { _foreground = value; OnPropertyChanged(); }
        }

        public CharColor Background
        {
            get { return _background; }
            set { _background = value; OnPropertyChanged(); }
        }

        public void Invalidate()
        {
            if (Parent != null)
                Parent.Invalidate();
            else if (Console.Root == this || Console.Popup == this) Console.Refresh();
        }

        public ICollection<IAttachedProperty> AttachedProperties { get; } = new List<IAttachedProperty>();

        public TAttachedProperty AttachedProperty<TAttachedProperty>()
            where TAttachedProperty : class, IAttachedProperty
        {
            return AttachedProperties.FirstOrDefault(x => x is TAttachedProperty) as TAttachedProperty;
        }
    }

    public class ChildRemovedEventArgs : EventArgs
    {
        public ChildRemovedEventArgs(Control child)
        {
            Child = child;
        }

        public Control Child { get; }
    }
}