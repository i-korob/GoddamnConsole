using System;
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
            provider.CursorVisible = false;
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
            Refresh();
        }

        public static void FocusNext() // TODO
        {

        }

        public static void FocusPrev() // TODO
        {

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
