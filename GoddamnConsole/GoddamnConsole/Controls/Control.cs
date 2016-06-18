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
        private class Lazy<T>
        {
            private readonly Control _owner;
            private readonly Func<T> _generator;
            private readonly string _propertyName;

            public Lazy(Control owner, Func<T> generator, string propertyName)
            {
                _owner = owner;
                _generator = generator;
                _propertyName = propertyName;
            }

            private bool _generated;
            private T _value;

            public void Reset()
            {
                _generated = false;
                // ReSharper disable once ExplicitCallerInfoArgument
                _owner.OnPropertyChanged(_propertyName);
            }

            public T Value
            {
                get
                {
                    if (_generated) return _value;
                    _value = _generator();
                    _generated = true;
                    return _value;
                }
            }
        }

        protected Control()
        {
            _actualWidth = new Lazy<int>(
                this,
                () =>
                (_parent as IParentControl)?.MeasureChild(this)?.Width ??
                (Console.Root == this || Console.Popup == this
                     ? Math.Min(AssumedWidth, Console.WindowWidth)
                     : 0),
                nameof(ActualWidth));
            _actualHeight = new Lazy<int>(
                this,
                () =>
                (_parent as IParentControl)?.MeasureChild(this)?.Height ??
                (Console.Root == this || Console.Popup == this
                     ? Math.Min(AssumedHeight, Console.WindowHeight)
                     : 0),
                nameof(ActualHeight));
        }

        #region Data Binding

        private object _dataContext;

        /// <summary>
        /// Gets or sets data context that used data binding
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

        private readonly Dictionary<PropertyInfo, Binding> _bindings
            = new Dictionary<PropertyInfo, Binding>();

        /// <summary>
        /// Binds control property to data context
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
        /// Unbinds control property from data context
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

        #endregion

        #region Parent

        private Control _parent;
        private int _width = int.MaxValue;
        private int _height = int.MaxValue;

        private void OnDetach(object sender, Control ctrl)
        {
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
            catch { /* */ }
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
            catch { /* */ }
        }

        /// <summary>
        /// Gets or sets control parent
        /// <para/>
        /// Child will be added into parent automatically
        /// </summary>
        public Control Parent
        {
            get
            {
                return _parent;
            }
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
        /// Fires when child removed from parent
        /// </summary>
        public event EventHandler DetachedFromParent;

        #endregion

        #region Virtual methods

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
            _actualWidth.Reset();
            _actualHeight.Reset();
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
        /// Invoked when keyboard button pressed. Override this to add handling
        /// </summary>
        /// <param name="key">Keyboard key info</param>
        protected virtual void OnKeyPress(ConsoleKeyInfo key) { }
        /// <summary>
        /// Invoked when Console needs to refresh. Override this to implement control rendering
        /// </summary>
        /// <param name="context">Rendering context, used to drawing primitives</param>
        public virtual void Render(DrawingContext context) { }

        /// <summary>
        /// Invoked when control actual size was changed. Override this to add handling
        /// </summary>
        protected virtual void OnSizeChanged() { }

        #endregion

        #region Control size 

        /// <summary>
        /// Gets or sets width of control
        /// <para/>
        /// If width less than zero, it is assumed as maximum value
        /// </summary>
        public int Width
        {
            get { return _width; }
            set { _width = value; OnSizeChanged(); OnPropertyChanged(); }
        }

        /// <summary>
        /// Assumed width of control
        /// </summary>
        public int AssumedWidth => Width < 0 ? int.MaxValue : Width;

        private readonly Lazy<int> _actualWidth;

        /// <summary>
        /// Measured width of control
        /// </summary>
        public int ActualWidth => _actualWidth.Value;

        /// <summary>
        /// Gets or sets height of control
        /// <para/>
        /// If height less than zero, it is assumed as maximum value
        /// </summary>
        public int Height
        {
            get { return _height; }
            set { _height = value; OnSizeChanged(); OnPropertyChanged(); }
        }

        /// <summary>
        /// Assumed height of control
        /// </summary>
        public int AssumedHeight => Height < 0 ? int.MaxValue : Height;

        private readonly Lazy<int> _actualHeight;
        private CharColor _foreground = CharColor.White;
        private CharColor _background = CharColor.Black;

        /// <summary>
        /// Measured height of control
        /// </summary>
        public int ActualHeight => _actualHeight.Value;

        #endregion

        /// <summary>
        /// Used to force redraw console
        /// <para/>
        /// Works only if control or its parent attached to console as root or popup element
        /// </summary>
        public void Invalidate()
        {
            if (Parent != null)
                Parent.Invalidate();
            else if (Console.Root == this || Console.Popup == this) Console.Refresh();
        }

        /// <summary>
        /// Gets or sets current cursor position
        /// <para/>
        /// Setting working only if control is focused
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

        #region Attached Properties

        /// <summary>
        /// Collection of attached properties
        /// </summary>
        public ICollection<IAttachedProperty> AttachedProperties { get; } = new List<IAttachedProperty>();

        /// <summary>
        /// Returns attached property of required type, or null if it ain't exist
        /// </summary>
        /// <typeparam name="TAttachedProperty">Required type of attached property</typeparam>
        /// <returns>Attached property of required type, or null if it ain't exist</returns>
        public TAttachedProperty AttachedProperty<TAttachedProperty>() where TAttachedProperty : class, IAttachedProperty
        {
            return AttachedProperties.FirstOrDefault(x => x is TAttachedProperty) as TAttachedProperty;
        }

        #endregion

        /// <summary>
        /// Gets or (in derived class) sets a value that indicates whether element can receive focus
        /// </summary>
        public bool Focusable { get; protected set; } = false;

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null, bool noInvalidate = false)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (noInvalidate) return;
            Invalidate();
        }
    }

    public interface IParentControl
    {
        /// <summary>
        /// Returns size of child control
        /// </summary>
        /// <param name="child">Child control</param>
        /// <returns>Size of child control</returns>
        Size MeasureChild(Control child);
    }

    public interface IContentControl : IParentControl
    {
        /// <summary>
        /// Gets or sets child control
        /// </summary>
        Control Content { get; set; }
        /// <summary>
        /// Fires after child control is changed
        /// </summary>
        event EventHandler<Control> ContentDetached;
    }

    public interface IChildrenControl : IParentControl
    {
        /// <summary>
        /// Returns a collection that contains children
        /// </summary>
        ICollection<Control> Children { get; }
        /// <summary>
        /// Returns a collection that contains children
        /// </summary>
        ICollection<Control> FocusableChildren { get; }
        /// <summary>
        /// Fires after child control removed from children collection
        /// </summary>
        event EventHandler<Control> ChildRemoved;
    }

    public interface IAttachedProperty
    {

    }
}
