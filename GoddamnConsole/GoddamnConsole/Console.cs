using System;
using System.Collections.Generic;
using System.Linq;
using GoddamnConsole.Controls;
using GoddamnConsole.Drawing;
using GoddamnConsole.NativeProviders;

namespace GoddamnConsole
{
    /// <summary>
    /// Represents a control host
    /// </summary>
    public class Console
    {
        static Console()
        {
            System.Console.CancelKeyPress += (o, e) => Shutdown();
        }

        internal static INativeConsoleProvider Provider;
        private static CharColor _background = CharColor.Black;
        private static Control _focused;
        private static Window _focusedWindow;

        /// <summary>
        /// Returns current window width
        /// </summary>
        public static int WindowWidth => Provider?.WindowWidth ?? 0;

        /// <summary>
        /// Returns current window height
        /// </summary>
        public static int WindowHeight => Provider?.WindowHeight ?? 0;

        /// <summary>
        /// Returns a value that indicates whether rendering cycle has been started
        /// </summary>
        public static bool Started => Provider != null;

        /// <summary>
        /// Gets or sets the console background
        /// </summary>
        public static CharColor Background
        {
            get { return _background; }
            set
            {
                _background = value;
                Refresh();
            }
        }

        /// <summary>
        /// Returns a list of windows
        /// </summary>
        public static IList<Window> Windows { get; } = new List<Window>();

        /// <summary>
        /// Gets or sets the current focused window
        /// </summary>
        public static Window FocusedWindow
        {
            get { return _focusedWindow; }
            set
            {
                _focusedWindow?.OnLostFocusInternal();
                _focusedWindow = value;
                _focusedWindow?.OnGotFocusInternal();
                Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the current focused element
        /// </summary>
        public static Control Focused
        {
            get { return _focused; }
            set
            {
                _focused?.OnLostFocusInternal();
                _focused = value;
                _focused?.OnGotFocusInternal();
                Refresh();
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether console can move focus
        /// </summary>
        public static bool CanChangeFocus { get; set; } = true;

        /// <summary>
        /// Starts rendering cycle and waits for shutdown
        /// </summary>
        public static void Start(INativeConsoleProvider provider)
        {
            if (Provider != null) throw new ArgumentException("Already started");
            Provider = provider;
            provider.Clear(_background);
            provider.KeyPressed += (o, e) =>
            {
                if (CanChangeFocus && e.Info.Key == ConsoleKey.Tab && e.Info.Modifiers == 0)
                {
                    FocusNext();
                    Refresh();
                    return;
                }
                if (CanChangeFocus && e.Info.Key == ConsoleKey.Tab && e.Info.Modifiers == ConsoleModifiers.Shift) // Can not intercept CTRL in Linux
                {
                    FocusNextWindow();
                    Refresh();
                    return;
                }
                //if (CanChangeFocus && e.Info.Key == ConsoleKey.Tab && e.Info.Modifiers == ConsoleModifiers.Control)
                //{
                //    FocusNextWindow();
                //    Refresh();
                //    return;
                //}
                //if (CanChangeFocus && e.Info.Key == ConsoleKey.Tab && e.Info.Modifiers == (ConsoleModifiers.Control | ConsoleModifiers.Shift))
                //{
                //    FocusPrevWindow();
                //    Refresh();
                //    return;
                //}
                Focused?.OnKeyPressedInternal(e.Info);
            };
            provider.SizeChanged += (o, e) =>
            {
                foreach (var window in Windows)
                    window.OnSizeChangedInternal(e.Before, e.After);
                Refresh();
            };
            FocusNextWindow();
        }

        private static void AllFocusableElements(Control current, ICollection<Control> elements)
        {
            if (current.Focusable) elements.Add(current);
            var contentControl = current as IContentControl;
            if (contentControl?.Content != null)
                AllFocusableElements(contentControl.Content, elements);
            else
            {
                var childrenControl = current as IChildrenControl;
                if (childrenControl == null) return;
                foreach (var element in childrenControl.FocusableChildren)
                    AllFocusableElements(element, elements);
            }
        }

        /// <summary>
        /// Moves focus to next window
        /// </summary>
        public static void FocusNextWindow()
        {
            Focused = null;
            if (Windows.Count == 0) return;
            var index = (Windows.IndexOf(FocusedWindow) + 1) % Windows.Count;
            FocusedWindow = Windows[index];
            FocusNext();
        }


        /// <summary>
        /// Moves focus to previous window
        /// </summary>
        public static void FocusPrevWindow()
        {
            Focused = null;
            if (Windows.Count == 0) return;
            var index = (Windows.Count + Windows.IndexOf(FocusedWindow) - 1) % Windows.Count;
            FocusedWindow = Windows[index];
            FocusNext();
        }
        
        /// <summary>
        /// Moves focus to next control
        /// </summary>
        public static void FocusNext()
        {
            if (Windows.Count == 0) return;
            var list = new List<Control>();
            AllFocusableElements(FocusedWindow, list);
            Focused =
                list.Contains(Focused)
                    ? list[(list.IndexOf(Focused) + 1) % list.Count]
                    : list.FirstOrDefault();
            Refresh();
        }
        
        /// <summary>
        /// Moves focus to previous control
        /// </summary>
        public static void FocusPrev()
        {
            if (Windows.Count == 0) return;
            var list = new List<Control>();
            AllFocusableElements(FocusedWindow, list);
            Focused =
                list.Contains(Focused)
                    ? list[(list.Count + list.IndexOf(Focused) - 1) % list.Count]
                    : list.FirstOrDefault();
            Refresh();
        }
        
        /// <summary>
        /// Performs console redraw
        /// </summary>
        public static void Refresh()
        {
            if (Provider == null) return;
            Provider.CursorVisible = false;
            Provider.Clear(_background);
            foreach (var window in Windows)
            {
                DrawingContext dc = new RealDrawingContext(window != FocusedWindow);
                var wid = window.ActualWidth;
                var hei = window.ActualHeight;
                dc = dc.Shrink(
                    new Rectangle(
                        window.HorizontalAlignment == WindowAlignment.Center
                            ? (WindowWidth - wid) / 2
                            : window.HorizontalAlignment == WindowAlignment.End
                                  ? WindowWidth - wid
                                  : 0,
                        window.VerticalAlignment == WindowAlignment.Center
                            ? (WindowHeight - hei) / 2
                            : window.VerticalAlignment == WindowAlignment.End
                                  ? WindowHeight - hei
                                  : 0,
                        wid,
                        hei));
                dc.Clear(Background);
                window.OnRenderInternal(dc);
            }
            Provider?.Refresh();
        }
        
        /// <summary>
        /// Stops console rendering cycle and disposes native provider
        /// </summary>
        public static void Shutdown()
        {
            if (Provider == null) throw new ArgumentException("Not started");
            Provider.Dispose();
            Provider = null;
            Focused = null;
        }
    }
}
