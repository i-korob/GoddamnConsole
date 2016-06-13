﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GoddamnConsole.DataBinding;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public abstract class Control
    {
        private readonly Dictionary<PropertyInfo, Binding> _bindings 
            = new Dictionary<PropertyInfo, Binding>();

        public object DataContext
        {
            get { return _dataContext; }
            set
            {
                _dataContext = value;
                foreach (var binding in _bindings.Values) binding.Refresh();
            }
        }

        public void Bind(string propertyName, string bindingPath, BindingMode bindingMode)
        {
            var property = GetType().GetProperty(propertyName);
            if (property == null) throw new ArgumentException("Property not found");
            Binding existingBinding;
            _bindings.TryGetValue(property, out existingBinding);
            if (existingBinding != null)
            {
                existingBinding.Dispose();
                _bindings.Remove(property);
            }
            _bindings.Add(property, new Binding(this, property, bindingPath, bindingMode));
        }

        public void Unbind(string propertyName)
        {
            
        }

        private Control _parent;
        private object _dataContext;

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
                }
                catch
                {
                    _parent = prev;
                    throw;
                }
            }
        }

        public event EventHandler DetachedFromParent;

        internal void OnKeyPressInternal(ConsoleKeyInfo key)
        {
            OnKeyPress(key);
        }

        internal void OnRenderInternal(DrawingContext context)
        {
            Render(context);
        }

        protected virtual void OnKeyPress(ConsoleKeyInfo key) { }
        public virtual void Render(DrawingContext context) { }

        public int Width { get; set; } = int.MaxValue; // max width by default

        public int ActualWidth =>
            (_parent as IParentControl)?.MeasureChild(this)?.Width ??
            (Console.Root == this ? Math.Min(Width, Console.WindowWidth) : 0);

        public int Height { get; set; } = int.MaxValue; // max height by default

        public int ActualHeight =>
            (_parent as IParentControl)?.MeasureChild(this)?.Height ??
            (Console.Root == this ? Math.Min(Height, Console.WindowHeight) : 0);

        public void Invalidate()
        {
            if (Parent != null)
                Parent.Invalidate();
            else if (Console.Root == this) Console.Refresh();  
        } 
        
        public Point CursorPosition
        {
            get { return new Point(Console.Provider.CursorX, Console.Provider.CursorY); }
            set
            {
                if (this == Console.Focused)
                {
                    Console.Provider.CursorX = value.X;
                    Console.Provider.CursorY = value.Y;
                }
            }
        }

        public ICollection<IAttachedProperty> AttachedProperties { get; } = new List<IAttachedProperty>();

        public TAttachedProperty AttachedProperty<TAttachedProperty>() where TAttachedProperty : class, IAttachedProperty
        {
            return AttachedProperties.FirstOrDefault(x => x is TAttachedProperty) as TAttachedProperty;
        }

        public bool Focusable { get; protected set; } = false;
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

    public interface IChildrenControl : IParentControl
    {
        ICollection<Control> Children { get; }
        event EventHandler<Control> ChildRemoved;
    }

    public interface IAttachedProperty
    {

    }
}
