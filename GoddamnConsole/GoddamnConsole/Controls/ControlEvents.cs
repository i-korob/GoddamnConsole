using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using GoddamnConsole.Drawing;
using GoddamnConsole.NativeProviders;

namespace GoddamnConsole.Controls
{
    /// <summary>
    /// Represents an interface of event which can be prevented to further handling
    /// </summary>
    public interface IPreventableEvent
    {
        /// <summary>
        /// Gets or sets a value that indicates whether event is handled and should be prevented to further handling
        /// </summary>
        bool Handled { get; set; }
    }

    /// <summary>
    /// Provides data for the KeyPressed event
    /// </summary>
    public class KeyPressedEventArgs : EventArgs, IPreventableEvent
    {
        /// <summary>
        /// Initializes a new instance of the KeyPressedEventArgs class
        /// <param name="info">Keyboard button info</param>
        /// </summary>
        public KeyPressedEventArgs(ConsoleKeyInfo info)
        {
            Info = info;
        }

        /// <summary>
        /// Returns the keyboard button info
        /// </summary>
        public ConsoleKeyInfo Info { get; }
        public bool Handled { get; set; }
    }

    public abstract partial class Control : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets a value that indicates whether unhandled exceptions in event handlers is suppressed
        /// </summary>
        public bool SuppressUnhandledExceptions { get; set; } = false;

        /// <summary>
        /// Occurs when keyboard button pressed and this control is focused before the event has been processed
        /// </summary>
        public event EventHandler<KeyPressedEventArgs> PreviewKeyPressed;
        /// <summary>
        /// Occurs when keyboard button pressed and this control is focused after the event has been processed
        /// </summary>
        public event EventHandler<KeyPressedEventArgs> KeyPressed;
        /// <summary>
        /// Occurs when control becomes focused
        /// </summary>
        public event EventHandler GotFocus;
        /// <summary>
        /// Occurs when control ceases to be focused
        /// </summary>
        public event EventHandler LostFocus;
        //public event EventHandler LayoutUpdated; // todo maybe?
        /// <summary>
        /// Occurs after control was rendered
        /// </summary>
        public event EventHandler Rendered;
        /// <summary>
        /// Occurs when control size was changed
        /// </summary>
        public event EventHandler SizeChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when control needs to be redrawn
        /// </summary>
        protected virtual void OnRender(DrawingContext dc) { }
        /// <summary>
        /// Called when keyboard button pressed and this control is focused
        /// </summary>
        protected virtual void OnKeyPressed(ConsoleKeyInfo info) { }
        /// <summary>
        /// Called when control becomes focused
        /// </summary>
        protected virtual void OnGotFocus() { }
        /// <summary>
        /// Called when control ceases to be focused
        /// </summary>
        protected virtual void OnLostFocus() { }
        /// <summary>
        /// Called when control size was changed
        /// </summary>
        protected virtual void OnSizeChanged() { }

        private void SafeInvoke(dynamic @event, EventArgs args)
        {
            try
            {
                @event?.Invoke(this, args);
            }
            catch
            {
                /* todo log */
                if (!SuppressUnhandledExceptions) throw;
            }
        }

        private void Bubble(dynamic @event, EventArgs args)
        {
            var preventable = args as IPreventableEvent;
            SafeInvoke(@event, args);
            if (preventable?.Handled ?? false) return; //-V3021
            (this as IContentControl)?.Content?.Bubble(@event, args);
            if (preventable?.Handled ?? false) return;
            foreach (var control in (this as IChildrenControl)?.Children ?? new Control[0])
            {
                control?.Bubble(@event, args);
                if (preventable?.Handled ?? false) return;
            }
        }
        
        internal void OnKeyPressedInternal(ConsoleKeyInfo cki)
        {
            Bubble(PreviewKeyPressed, new KeyPressedEventArgs(cki));
            OnKeyPressed(cki);
            SafeInvoke(KeyPressed, new KeyPressedEventArgs(cki));
        }

        internal void OnGotFocusInternal()
        {
            OnGotFocus();
            SafeInvoke(GotFocus, EventArgs.Empty);
        }

        internal void OnLostFocusInternal()
        {
            OnLostFocus();
            SafeInvoke(LostFocus, EventArgs.Empty);
        }

        internal void OnRenderInternal(DrawingContext dc)
        {
            if (!ActualVisibility) return;
            OnRender(dc);
            var pc = this as IParentControl;
            if (pc == null) return;
            var content = (this as IContentControl)?.Content;
            content?.OnRenderInternal(dc.Shrink(pc.GetScrollOffset(content), pc.MeasureBoundingBox(content)));
            foreach (
                var control in (this as IChildrenControl)?.Children ?? new Control[0])
            {
                control.OnRenderInternal(dc.Shrink(pc.GetScrollOffset(content), pc.MeasureBoundingBox(control)));
            }
            SafeInvoke(Rendered, EventArgs.Empty);
        }

        internal void OnSizeChangedInternal()
        {
            OnSizeChanged();
            SafeInvoke(SizeChanged, EventArgs.Empty);
            var content = (this as IContentControl)?.Content;
            content?.OnSizeChangedInternal();
            foreach (
                var control in (this as IChildrenControl)?.Children ?? new Control[0])
            {
                control.OnSizeChangedInternal();
            }
        }
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var cancelInvalidation = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            var prop = GetType().GetProperty(propertyName ?? "");
            cancelInvalidation |=
                prop?.CustomAttributes.Any(x => x.AttributeType == typeof(NoInvalidateOnChangeAttribute)) ?? false;
            foreach (var alsoNotifyFor in
                (prop?.GetCustomAttributes(typeof (AlsoNotifyForAttribute), true) ??
                 new object[0]).Cast<AlsoNotifyForAttribute>())
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(alsoNotifyFor.PropertyName));
                prop = GetType().GetProperty(alsoNotifyFor.PropertyName ?? "");
                cancelInvalidation |=
                    prop?.CustomAttributes.Any(x => x.AttributeType == typeof(NoInvalidateOnChangeAttribute)) ?? false;
            }
            if (!cancelInvalidation) Invalidate();
        }
    }

    /// <summary>
    /// Used when property should notify that another property has changed, when it changes 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class AlsoNotifyForAttribute : Attribute
    {
        public AlsoNotifyForAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        /// <summary>
        /// Returns a property name
        /// </summary>
        public string PropertyName { get; }
    }

    /// <summary>
    /// Used when control should not invalidate, when property changes
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NoInvalidateOnChangeAttribute : Attribute {}
}
