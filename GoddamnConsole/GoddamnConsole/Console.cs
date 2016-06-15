using System;
using System.Collections.Generic;
using System.Linq;
using GoddamnConsole.Controls;
using GoddamnConsole.Drawing;
using GoddamnConsole.NativeProviders;

namespace GoddamnConsole
{
    public class Console
    {
        internal static INativeConsoleProvider Provider;
        private static CharColor _background = CharColor.Black;
        private static bool _isPopupVisible;
        private static Control _focused;
        private static Control _popup;

        public static int WindowWidth => Provider?.WindowWidth ?? 0;
        public static int WindowHeight => Provider?.WindowHeight ?? 0;
        public static bool Started => Provider != null;

        public static CharColor Background
        {
            get { return _background; }
            set
            {
                _background = value;
                Refresh();
            }
        }

        public static Control Root { get; private set; }

        public static Control Focused
        {
            get { return _focused; }
            set { _focused = value; Refresh(); }
        }

        public static Control Popup
        {
            get { return _popup; }
            set { _popup = value; Refresh(); }
        }

        public static bool IsPopupVisible
        {
            get { return _isPopupVisible; }
            set { _isPopupVisible = value; Refresh(); }
        }

        public static bool CanChangeFocus { get; set; } = true;

        public static void Start(INativeConsoleProvider provider, Control root)
        {
            Root = root;
            if (Provider != null) throw new ArgumentException("Already started");
            Provider = provider;
            provider.Clear(_background);
            provider.KeyPressed += (o, e) =>
            {
                if (CanChangeFocus && e.Key == ConsoleKey.Tab && e.Modifiers == 0)
                {
                    FocusNext();
                    Refresh();
                    return;
                }
                if (CanChangeFocus && e.Key == ConsoleKey.Tab && e.Modifiers == ConsoleModifiers.Shift)
                {
                    FocusPrev();
                    Refresh();
                    return;
                }
                Focused?.OnKeyPressInternal(e);
            };
            provider.SizeChanged += (o, e) => Refresh();
            FocusNext();
        }

        private static void AllFocusableElements(Control current, List<Control> elements)
        {
            if (current.Focusable) elements.Add(current);
            var contentControl = current as IContentControl;
            if (contentControl != null && contentControl.Content != null)
                AllFocusableElements(contentControl.Content, elements);
            else
            {
                var childrenControl = current as IChildrenControl;
                if (childrenControl == null) return;
                foreach (var element in childrenControl.Children)
                    AllFocusableElements(element, elements);
            }
        }

        public static void FocusNext()
        {
            var list = new List<Control>();
            AllFocusableElements(Root, list);
            Focused =
                list.Contains(Focused)
                    ? list[(list.IndexOf(Focused) + 1) % list.Count]
                    : list.FirstOrDefault();
            Refresh();
        }

        public static void FocusPrev() // TODO
        {
            var list = new List<Control>();
            AllFocusableElements(Root, list);
            Focused =
                list.Contains(Focused)
                    ? list[(list.Count + list.IndexOf(Focused) - 1) % list.Count]
                    : list.FirstOrDefault();
            Refresh();
        }

        public static void Refresh()
        {
            Provider?.Clear(_background);
            Root?.OnRenderInternal(new RealDrawingContext(_isPopupVisible));
            if (_isPopupVisible && Popup != null)
            {
                var rdc =
                    Popup.AssumedWidth > WindowWidth || Popup.AssumedHeight > WindowHeight
                        ? new RealDrawingContext()
                        : new RealDrawingContext()
                              .Shrink(
                                  new Rectangle(
                                      (WindowWidth - Popup.AssumedWidth) / 2,
                                      (WindowHeight - Popup.AssumedHeight) / 2,
                                      Popup.AssumedWidth,
                                      Popup.AssumedHeight
                                      ));
                rdc.Clear();
                Popup?.OnRenderInternal(rdc);
            }
            Provider?.Refresh();
        }

        public static void Shutdown()
        {
            if (Provider == null) throw new ArgumentException("Not started");
            Provider.Dispose();
            Provider = null;
            Root = null;
            Focused = null;
        }

        public static int MeasureWidth(int width)
        {
            return width == -1 ? Provider.WindowWidth : Math.Min(Provider.WindowWidth, width);
        }

        public static int MeasureHeight(int height)
        {
            return height == -1 ? Provider.WindowHeight : Math.Min(Provider.WindowHeight, height);
        }
    }
}
