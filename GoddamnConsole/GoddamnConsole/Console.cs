using System;
using System.Collections.Generic;
using System.Linq;
using GoddamnConsole.Controls;
using GoddamnConsole.Drawing;
using GoddamnConsole.NativeProviders;
using GoddamnConsole.NativeProviders.Unix;
using GoddamnConsole.NativeProviders.Windows;

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
        private static WindowBase _focusedWindow;

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
        public static IList<WindowBase> Windows { get; } = new List<WindowBase>();

        /// <summary>
        /// Gets or sets the current focused window
        /// </summary>
        public static WindowBase FocusedWindow
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
        public static void Start()
        {
            var pf = Environment.OSVersion.Platform;
            var isUnix = pf == PlatformID.Unix || pf == PlatformID.MacOSX || (int) pf == 128; // unix/macosx/mono linux
            var provider =
                isUnix
                    ? (INativeConsoleProvider) new UnixNativeConsoleProvider()
                    : new WindowsNativeConsoleProvider();
            if (Provider != null) throw new ArgumentException("Already started");
            Provider = provider;
            provider.Clear(_background);
            provider.KeyPressed += (o, e) =>
            {
                if (CanChangeFocus && e.Info.Key == ConsoleKey.Tab && e.Info.Modifiers == 0)
                {
                    _prevent = true;
                    FocusNext();
                    _prevent = false;
                    Refresh();
                    return;
                }
                if (CanChangeFocus && e.Info.Key == ConsoleKey.Tab && e.Info.Modifiers == ConsoleModifiers.Shift) // Can not intercept CTRL in Linux
                {
                    _prevent = true;
                    FocusNextWindow();
                    _prevent = false;
                    Refresh();
                    return;
                }
                _prevent = true;
                Focused?.OnKeyPressedInternal(e.Info);
                _prevent = false;
                Refresh();
            };
            provider.SizeChanged += (o, e) =>
            {
                _prevent = true;
                foreach (var window in Windows)
                    window.OnSizeChangedInternal();
                _prevent = false;
                Refresh();
            };
            _prevent = true;
            FocusNextWindow();
            _prevent = false;
            Refresh();
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
        }

        private static volatile bool _fault;
        private static bool _refreshing;
        private static int _discardCount;
        private static bool _prevent;

        /// <summary>
        /// Performs console redraw
        /// </summary>
        public static void Refresh()
        {
            if (_prevent) return;
            if (_refreshing)
            {
                _fault = true;
                return;
            }
            _refreshing = true;
            _discardCount = 0;
            _fault = false;
            try
            {
                if (Provider == null) return;
                do
                {
                    Provider.CursorVisible = false;
                    Provider.Clear(_background);
                    foreach (var window in Windows.OrderBy(x => FocusedWindow == x ? int.MaxValue : Windows.IndexOf(x)))
                    {
                        DrawingContext dc = new RealDrawingContext(window != FocusedWindow);
                        var wid = window.ActualWidth;
                        var hei = window.ActualHeight;
                        dc = dc.Shrink(
                            new Rectangle(
                                window.HorizontalAlignment == Alignment.Center
                                    ? (WindowWidth - wid) / 2
                                    : window.HorizontalAlignment == Alignment.End
                                          ? WindowWidth - wid
                                          : 0,
                                window.VerticalAlignment == Alignment.Center
                                    ? (WindowHeight - hei) / 2
                                    : window.VerticalAlignment == Alignment.End
                                          ? WindowHeight - hei
                                          : 0,
                                wid,
                                hei));
                        dc.Clear(Background);
                        window.OnRenderInternal(dc);
                    }
                    _discardCount++;
                } while (_fault && _discardCount < 3);
                Provider.Refresh();
            }
            finally
            {
                _refreshing = false; 
            }
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
