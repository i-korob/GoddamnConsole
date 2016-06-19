using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using GoddamnConsole.DataBinding;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public abstract class Control : INotifyPropertyChanged
    {
        private readonly Dictionary<PropertyInfo, Binding> _bindings
            = new Dictionary<PropertyInfo, Binding>();

        private CharColor _background = CharColor.Black;
        private object _dataContext;


        private CharColor _foreground = CharColor.White;


        private Control _parent;
        private ControlSize _width = ControlSizeType.BoundingBoxSize;
        private ControlSize _height = ControlSizeType.BoundingBoxSize;

        /// <summary>
        ///     Gets or sets data context that used data binding
        /// </summary>
        public object DataContext
        {
            get { return _dataContext; }
            set
            {
                _dataContext = value;
                foreach (var binding in _bindings.Values) binding.Refresh();
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets control parent
        ///     <para />
        ///     Child will be added into parent automatically
        /// </summary>
        public Control Parent
        {
            get { return _parent; }
            set
            {
                var prev = _parent;
                _parent = value;
                try
                {
                    if (value == null)
                    {
                        RemoveFromParent();
                        _parent = null;
                        return;
                    }
                    var cctl = value as IContentControl;
                    if (cctl != null && cctl.Content != this)
                    {
                        cctl.ContentDetached += OnDetach;
                        cctl.Content = this;
                    }
                    else
                    {
                        var pctl = value as IChildrenControl;
                        if (pctl != null)
                        {
                            pctl.ChildRemoved += OnDetach;
                            pctl.Children.Add(this);
                        }
                        else throw new NotSupportedException($"{value.GetType().Name} can not have child");
                    }
                    OnPropertyChanged();
                }
                catch
                {
                    _parent = prev;
                    throw;
                }
            }
        }

        /// <summary>
        ///     Gets or sets current cursor position
        ///     <para />
        ///     Setting working only if control is focused
        /// </summary>
        public Point CursorPosition
        {
            get { return new Point(Console.Provider.CursorX, Console.Provider.CursorY); }
            set
            {
                if (this == Console.Focused || this == Console.Popup)
                {
                    Console.Provider.CursorX = value.X;
                    Console.Provider.CursorY = value.Y;
                    OnPropertyChanged(noInvalidate: true);
                }
            }
        }


        /// <summary>
        ///     Collection of attached properties
        /// </summary>
        public ICollection<IAttachedProperty> AttachedProperties { get; } = new List<IAttachedProperty>();


        /// <summary>
        ///     Gets or (in derived class) sets a value that indicates whether element can receive focus
        /// </summary>
        public bool Focusable { get; protected set; } = false;

        public CharColor Foreground
        {
            get { return _foreground; }
            set
            {
                _foreground = value;
                OnPropertyChanged();
            }
        }

        public CharColor Background
        {
            get { return _background; }
            set
            {
                _background = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Binds control property to data context
        /// </summary>
        /// <param name="propertyName">Name of control property</param>
        /// <param name="bindingPath">Path in data context</param>
        public void Bind(string propertyName, string bindingPath)
        {
            var property = GetType().GetProperty(propertyName);
            if (property == null) throw new ArgumentException("Property not found");
            Unbind(propertyName);
            _bindings.Add(property, new Binding(this, property, bindingPath));
        }

        /// <summary>
        ///     Unbinds control property from data context
        /// </summary>
        /// <param name="propertyName">Name of control property</param>
        public void Unbind(string propertyName)
        {
            var property = GetType().GetProperty(propertyName);
            Binding existingBinding;
            _bindings.TryGetValue(property, out existingBinding);
            if (existingBinding == null) return;
            existingBinding.Cleanup();
            _bindings.Remove(property);
        }

        private void OnDetach(object sender, ChildRemovedEventArgs args)
        {
            var ctrl = args?.Child;
            if (sender != _parent || ctrl != this) return;
            var cctl = sender as IContentControl;
            if (cctl != null && cctl.Content != this)
            {
                cctl.ContentDetached -= OnDetach;
            }
            else
            {
                var pctl = sender as IChildrenControl;
                if (pctl != null)
                {
                    pctl.ChildRemoved -= OnDetach;
                }
                else throw new Exception("Parent can not have children (wat)"); // how this shit could happen?
            }
            try
            {
                DetachedFromParent?.Invoke(this, EventArgs.Empty);
            }
            catch
            {
                /* */
            }
        }

        private void RemoveFromParent()
        {
            var cctl = _parent as IContentControl;
            if (cctl != null && cctl.Content != this)
            {
                cctl.Content = null;
            }
            else
            {
                var pctl = _parent as IChildrenControl;
                if (pctl != null)
                {
                    pctl.Children.Remove(this);
                }
                else throw new Exception("Parent can not have children (wat)");
            }
            try
            {
                DetachedFromParent?.Invoke(this, EventArgs.Empty);
            }
            catch
            {
                /* */
            }
        }

        /// <summary>
        ///     Fires when child removed from parent
        /// </summary>
        public event EventHandler DetachedFromParent;


        internal void OnKeyPressInternal(ConsoleKeyInfo key)
        {
            OnKeyPress(key);
        }

        internal void OnRenderInternal(DrawingContext context)
        {
            Render(context);
        }

        internal void OnSizeChangedInternal()
        {
            OnSizeChanged();
            var cctl = this as IContentControl;
            if (cctl != null)
            {
                cctl.Content?.OnSizeChangedInternal();
            }
            else
            {
                var pctl = this as IChildrenControl;
                if (pctl != null)
                {
                    foreach (var child in pctl.Children) child.OnSizeChangedInternal();
                }
            }
        }

        /// <summary>
        ///     Invoked when keyboard button pressed. Override this to add handling
        /// </summary>
        /// <param name="key">Keyboard key info</param>
        protected virtual void OnKeyPress(ConsoleKeyInfo key)
        {
        }

        /// <summary>
        ///     Invoked when Console needs to refresh. Override this to implement control rendering
        /// </summary>
        /// <param name="context">Rendering context, used to drawing primitives</param>
        public virtual void Render(DrawingContext context)
        {
        }

        /// <summary>
        ///     Invoked when control actual size was changed. Override this to add handling
        /// </summary>
        protected virtual void OnSizeChanged()
        {
        }

        /// <summary>
        ///     Used to force redraw console
        ///     <para />
        ///     Works only if control or its parent attached to console as root or popup element
        /// </summary>
        public void Invalidate()
        {
            if (Parent != null)
                Parent.Invalidate();
            else if (Console.Root == this || Console.Popup == this) Console.Refresh();
        }

        /// <summary>
        ///     Returns attached property of required type, or null if it ain't exist
        /// </summary>
        /// <typeparam name="TAttachedProperty">Required type of attached property</typeparam>
        /// <returns>Attached property of required type, or null if it ain't exist</returns>
        public TAttachedProperty AttachedProperty<TAttachedProperty>()
            where TAttachedProperty : class, IAttachedProperty
        {
            return AttachedProperties.FirstOrDefault(x => x is TAttachedProperty) as TAttachedProperty;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null,
            bool noInvalidate = false)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (noInvalidate) return;
            Invalidate();
        }

        public virtual int MinWidth { get; } = 0;
        public virtual int MaxWidth { get; } = int.MaxValue;
        public virtual int MinHeight { get; } = 0;
        public virtual int MaxHeight { get; } = int.MaxValue;

        public ControlSize Width
        {
            get { return _width; }
            set { _width = value; OnPropertyChanged(); OnSizeChanged(); }
        }

        public ControlSize Height
        {
            get { return _height; }
            set { _height = value; OnPropertyChanged(); OnSizeChanged(); }
        }

        public int ActualWidth
        {
            get
            {
                switch (Width.Type)
                {
                    case ControlSizeType.Fixed:
                        return Width.Value;
                    case ControlSizeType.Infinite:
                        return int.MaxValue;
                    case ControlSizeType.BoundingBoxSize:
                        return (Parent as IParentControl)?.MeasureBoundingBox(this)?.Width
                            ?? (this == Console.Root ? Console.WindowWidth : 0);
                    case ControlSizeType.MinByContent:
                        return MinWidth;
                    case ControlSizeType.MaxByContent:
                        return MaxWidth;
                    default:
                        return 0;
                }
            }
        }

        public int ActualHeight
        {
            get
            {
                switch (Height.Type)
                {
                    case ControlSizeType.Fixed:
                        return Height.Value;
                    case ControlSizeType.Infinite:
                        return int.MaxValue;
                    case ControlSizeType.BoundingBoxSize:
                        return (Parent as IParentControl)?.MeasureBoundingBox(this)?.Height 
                            ?? (this == Console.Root ? Console.WindowHeight : 0);
                    case ControlSizeType.MinByContent:
                        return MinHeight;
                    case ControlSizeType.MaxByContent:
                        return MaxHeight;
                    default:
                        return 0;
                }
            }
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
    public enum ControlSizeType
    {
        Fixed,
        Infinite,
        BoundingBoxSize,
        MaxByContent,
        MinByContent
    }

    public struct ControlSize
    {
        public ControlSize(ControlSizeType type, int value)
        {
            Type = type;
            Value = value;
        }

        public ControlSizeType Type { get; set; }

        public int Value { get; set; }

        public static implicit operator ControlSize(uint size)
            => new ControlSize(ControlSizeType.Fixed, (int) Math.Max(size, int.MaxValue));

        public static implicit operator ControlSize(int size)
            => new ControlSize(ControlSizeType.Fixed, Math.Max(0, size));

        public static implicit operator ControlSize(ControlSizeType type)
        {
            return new ControlSize(type, 0);
        }
    }
}