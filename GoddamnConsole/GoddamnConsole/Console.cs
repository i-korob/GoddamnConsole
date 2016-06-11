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

        public static int WindowWidth => Provider.WindowWidth;
        public static int WindowHeight => Provider.WindowHeight;
        public static bool Started => Provider != null;

        public static Control Root { get; private set; }
        public static Control Focused { get; set; }
        public static bool CanChangeFocus { get; set; } = true;

        public static void Start(INativeConsoleProvider provider, Control root)
        {
            Root = root;
            if (Provider != null) throw new ArgumentException("Already started");
            Provider = provider;
            provider.Clear();
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

        public static void FocusNext() // TODO
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
            Provider?.Clear();
            Root?.OnRenderInternal(new RealDrawingContext());
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
