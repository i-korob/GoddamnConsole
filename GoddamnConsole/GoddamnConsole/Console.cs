using System;
using System.Collections.Generic;
using System.Linq;
using GoddamnConsole.Controls;
using GoddamnConsole.Drawing;
using GoddamnConsole.NativeProviders;
using GoddamnConsole.NativeProviders.Windows;

namespace GoddamnConsole
{
    public class Console
    {
        internal static INativeConsoleProvider Provider;
        private static CharColor _background = CharColor.Black;
        private static bool _isPopupVisible;
        private static Control _focused;
        private static Control _popup;

        /// <summary>
        /// Returns current console window width
        /// </summary>
        public static int WindowWidth => Provider?.WindowWidth ?? 0;
        /// <summary>
        /// Returns current console window height
        /// </summary>
        public static int WindowHeight => Provider?.WindowHeight ?? 0;
        /// <summary>
        /// Returns value that indicates whether console UI was started
        /// </summary>
        public static bool Started => Provider != null;

        /// <summary>
        /// Gets or sets current console background
        /// <para/>
        /// Default is <see cref="CharColor"/>.Black
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
        /// Returns current root element
        /// </summary>
        public static Control Root { get; private set; }

        /// <summary>
        /// Gets or sets current focused element
        /// </summary>
        public static Control Focused
        {
            get { return _focused; }
            set { _focused = value; Refresh(); }
        }

        /// <summary>
        /// Gets or sets current popup element
        /// </summary>
        public static Control Popup
        {
            get { return _popup; }
            set { _popup = value; Refresh(); }
        }

        /// <summary>
        /// Gets or sets value that indicates whether popup is shown
        /// <para/>
        /// If this value is <c>True</c>, root control will be rendered with low brightness mode, regardless of value of <see cref="Popup"/>
        /// </summary>
        public static bool IsPopupVisible
        {
            get { return _isPopupVisible; }
            set
            {
                _isPopupVisible = value;
                _focused = null;
                FocusNext();
                Refresh();
            }
        }

        /// <summary>
        /// Gets or sets value that indicates whetner console can move focus
        /// </summary>
        public static bool CanChangeFocus { get; set; } = true;

        /// <summary>
        /// Starts rendering root control and handling keyboard events
        /// </summary>
        /// <param name="provider">Native console provider.
        /// Use <see cref="WindowsNativeConsoleProvider"/> for Windows</param>
        /// <param name="root">Root control</param>
        public static void Start(INativeConsoleProvider provider, Control root)
        {
            Root = root;
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
                if (CanChangeFocus && e.Info.Key == ConsoleKey.Tab && e.Info.Modifiers == ConsoleModifiers.Shift)
                {
                    FocusPrev();
                    Refresh();
                    return;
                }
                Focused?.OnKeyPressInternal(e.Info);
            };
            provider.SizeChanged += (o, e) =>
            {
                Root?.OnSizeChangedInternal();
                Popup?.OnSizeChangedInternal();
                Refresh();
            };
            FocusNext();
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
        /// Moves focus to next focusable element
        /// </summary>
        public static void FocusNext()
        {
            if (_isPopupVisible && Popup == null) return;
            var list = new List<Control>();
            AllFocusableElements(_isPopupVisible ? Popup : Root, list);
            Focused =
                list.Contains(Focused)
                    ? list[(list.IndexOf(Focused) + 1) % list.Count]
                    : list.FirstOrDefault();
            Refresh();
        }

        /// <summary>
        /// Moves focus to previous focusable element
        /// </summary>
        public static void FocusPrev()
        {
            if (_isPopupVisible && Popup == null) return;
            var list = new List<Control>();
            AllFocusableElements(_isPopupVisible ? Popup : Root, list);
            Focused =
                list.Contains(Focused)
                    ? list[(list.Count + list.IndexOf(Focused) - 1) % list.Count]
                    : list.FirstOrDefault();
            Refresh();
        }

        /// <summary>
        /// Redrawing console
        /// </summary>
        public static void Refresh()
        {
            if (Provider == null) return;
            Provider.CursorVisible = false;
            Provider.Clear(_background);
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
                rdc.Clear(Background);
                Popup?.OnRenderInternal(rdc);
            }
            Provider?.Refresh();
        }

        /// <summary>
        /// Stops handling console events and disposes native provider
        /// </summary>
        public static void Shutdown()
        {
            if (Provider == null) throw new ArgumentException("Not started");
            Provider.Dispose();
            Provider = null;
            Root = null;
            Focused = null;
        }

        [Obsolete]
        public static int MeasureWidth(int width)
        {
            return width < 0 ? Provider.WindowWidth : Math.Min(Provider.WindowWidth, width);
        }

        [Obsolete]
        public static int MeasureHeight(int height)
        {
            return height < 0 ? Provider.WindowHeight : Math.Min(Provider.WindowHeight, height);
        }
    }
}
