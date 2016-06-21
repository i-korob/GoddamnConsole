using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using GoddamnConsole.Drawing;
using GoddamnConsole.NativeProviders;

namespace GoddamnConsole.Controls
{
    public interface IPreventableEvent
    {
        bool Handled { get; set; }
    }

    public class KeyPressedEventArgs : EventArgs, IPreventableEvent
    {
        public KeyPressedEventArgs(ConsoleKeyInfo info)
        {
            Info = info;
        }

        public ConsoleKeyInfo Info { get; }
        public bool Handled { get; set; }
    }

    public abstract partial class Control : INotifyPropertyChanged
    {
        public bool SuppressUnhandledExceptions { get; set; } = false;

        public event EventHandler<KeyPressedEventArgs> PreviewKeyPressed;
        public event EventHandler<KeyPressedEventArgs> KeyPressed;
        public event EventHandler GotFocus;
        public event EventHandler LostFocus;
        //public event EventHandler LayoutUpdated; // todo maybe?
        public event EventHandler Rendered;
        public event EventHandler<SizeChangedEventArgs> SizeChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnRender(DrawingContext dc) { }
        protected virtual void OnKeyPressed(ConsoleKeyInfo info) { }
        protected virtual void OnGotFocus() { }
        protected virtual void OnLostFocus() { }
        protected virtual void OnSizeChanged(Size prevSize, Size newSize) { }

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
            if (preventable?.Handled ?? false) return;
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

        internal void OnSizeChangedInternal(Size prevSize, Size newSize)
        {
            OnSizeChanged(prevSize, newSize);
            SafeInvoke(SizeChanged, new SizeChangedEventArgs(prevSize, newSize));
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

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class AlsoNotifyForAttribute : Attribute
    {
        public AlsoNotifyForAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class NoInvalidateOnChangeAttribute : Attribute {}
}
