using System;
using System.Collections.Generic;
using System.Linq;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    /// <summary>
    /// Represents a base class for user interface elements
    /// </summary>
    public abstract partial class Control
    {
        private bool _focusable;
        private CharColor _foreground = CharColor.White;
        private CharColor _background = CharColor.Black;

        /// <summary>
        /// Gets or sets a current cursor position. If control is not focused, attempts to set cursor position will be ignored
        /// </summary>
        [NoInvalidateOnChange]
        public Point CursorPosition
        {
            get { return new Point(Console.Provider.CursorX, Console.Provider.CursorY); }
            set
            {
                if (this == Console.Focused)
                {
                    Console.Provider.CursorX = value.X;
                    Console.Provider.CursorY = value.Y;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Returns a value that indicates whether the control can have focus
        /// </summary>
        public bool Focusable
        {
            get { return _focusable; }
            protected set { _focusable = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets a control foreground
        /// </summary>
        public CharColor Foreground
        {
            get { return _foreground; }
            set { _foreground = value; OnPropertyChanged(); }
        }
        
        /// <summary>
        /// Gets or sets a control background
        /// </summary>
        public CharColor Background
        {
            get { return _background; }
            set { _background = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Forces a full console UI redrawing
        /// </summary>
        public void Invalidate()
        {
            if (Parent != null)
                Parent.Invalidate();
            else if (Console.Windows.Contains(this)) Console.Refresh();
        }

        /// <summary>
        /// Returns a collection of attached properties
        /// </summary>
        public ICollection<IAttachedProperty> AttachedProperties { get; } = new List<IAttachedProperty>();

        /// <summary>
        /// Returns an attached property with specified type
        /// </summary>
        /// <typeparam name="TAttachedProperty">Type of attached property</typeparam>
        public TAttachedProperty AttachedProperty<TAttachedProperty>()
            where TAttachedProperty : class, IAttachedProperty
        {
            return AttachedProperties.FirstOrDefault(x => x is TAttachedProperty) as TAttachedProperty;
        }
    }

    /// <summary>
    /// Provides data for the ChildRemoved event
    /// </summary>
    public class ChildRemovedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the ChildRemovedEventArgs class
        /// </summary>
        /// <param name="child">Removed child</param>
        public ChildRemovedEventArgs(Control child)
        {
            Child = child;
        }

        /// <summary>
        /// Returns a removed children
        /// </summary>
        public Control Child { get; }
    }
}