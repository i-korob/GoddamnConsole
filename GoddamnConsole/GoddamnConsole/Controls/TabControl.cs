using System;
using System.Collections.Generic;
using System.Linq;
using GoddamnConsole.Drawing;
using static GoddamnConsole.Drawing.FrameOptions;

namespace GoddamnConsole.Controls
{
    /// <summary>
    /// Represents a control that contains multiple items that share same space
    /// </summary>
    public class TabControl : ChildrenControl
    {
        public TabControl()
        {
            Focusable = true;
        }

        public override Size BoundingBoxReduction => new Size(2, 4);

        public override IList<Control> FocusableChildren
            => SelectedTab == null ? new Control[0] : new Control[] { SelectedTab };

        public override Rectangle MeasureBoundingBox(Control child)
        {
            return child == SelectedTab
                ? new Rectangle(1, 3, ActualWidth - 2, ActualHeight - 4)
                : new Rectangle(0, 0, 0, 0);
        }

        protected override void OnKeyPressed(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.LeftArrow)
            {
                var index = Children.IndexOf(SelectedTab);
                if (index < 1) index = 1;
                index--;
                if (Children.Count <= index) return;
                SelectedTab = (Tab)Children[index];
                Invalidate();
            }
            else if (key.Key == ConsoleKey.RightArrow)
            {
                var index = Children.IndexOf(SelectedTab);
                index++;
                if (Children.Count <= index) return;
                SelectedTab = (Tab)Children[index];
                Invalidate();
            }
        }

        private int _scroll;

        protected override void OnRender(DrawingContext context)
        {
            var style = Console.Focused == this
                            ? FrameStyle.Double
                            : FrameStyle.Single;
            var wpt = (ActualWidth - 1) / Children.Count - 1;
            if (wpt < 2) return;
            context.DrawFrame(new Rectangle(0, 0, ActualWidth, ActualHeight),
                new FrameOptions
                {
                    Style = Console.Focused == this ? FrameStyle.Double : FrameStyle.Single,
                    Foreground = Foreground,
                    Background = Background
                });
            var txo = new TextOptions
            {
                Background = Background,
                Foreground = Foreground
            };
            context.PutChar(new Point(0, 2),
                            Piece(FramePiece.Vertical | FramePiece.Right, style), Foreground, Background,
                            CharAttribute.None);
            context.PutChar(new Point(ActualWidth - 1, 2),
                            Piece(FramePiece.Vertical | FramePiece.Left, style), Foreground, Background,
                            CharAttribute.None);
            var ro = new RectangleOptions
            {
                Foreground = Foreground,
                Background = Background
            };
            context.DrawRectangle(new Rectangle(1, 0, ActualWidth - 2, 1),
                                  Piece(FramePiece.Horizontal, style), ro);
            context.DrawRectangle(new Rectangle(1, 2, ActualWidth - 2, 1),
                                  Piece(FramePiece.Horizontal, style), ro);
            const int padding = 2;
            var strPadding = new string(' ', padding);
            var lengths = Children.Select(x => ((Tab)x).Title.Length + 2 * padding);
            var selectedOffset = lengths.Take(SelectedIndex).Sum(x => x + 1);
            var selectedLen = (SelectedTab?.Title.Length ?? 0) + 2 * padding;
            if (selectedOffset + _scroll < 0) _scroll = -selectedOffset;
            if (selectedOffset + selectedLen + _scroll > ActualWidth - 2)
                _scroll = ActualWidth - 2 - selectedOffset - selectedLen;
            var headerContext = context.Shrink(new Rectangle(1, 0, ActualWidth - 2, 3)).Scroll(new Point(-1 + _scroll, 0));
            for (int i = 0, ofs = 0; i < Children.Count; i++)
            {
                var tab = (Tab)Children[i];
                var len = Math.Max(tab.Title.Length, 1);
                headerContext.DrawText(
                    new Point(ofs + 1, 1),
                    $"{strPadding}{tab.Title}{strPadding}",
                    i == SelectedIndex
                        ? new TextOptions
                        {
                            Foreground = Background,
                            Background = Foreground
                        }
                        : txo);
                ofs += len + 1 + 2 * padding;
                headerContext.PutChar(new Point(ofs, 1),
                                  Piece(FramePiece.Vertical, style), Foreground, Background, CharAttribute.None);
                headerContext.PutChar(new Point(ofs, 0),
                                      Piece(FramePiece.Horizontal | FramePiece.Bottom, style), Foreground, Background,
                                      CharAttribute.None);
                headerContext.PutChar(new Point(ofs, 2),
                                      Piece(FramePiece.Horizontal | FramePiece.Top, style), Foreground, Background,
                                      CharAttribute.None);
            }
        }

        /// <summary>
        /// Gets or sets the current tab
        /// </summary>
        public Tab SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                _selectedTab = value;
                OnPropertyChanged();
            }
        }

        public int SelectedIndex
        {
            get { return Children.IndexOf(SelectedTab); }
            set { SelectedTab = (Tab)Children[value]; }
        }

        private Tab _selectedTab;

        public override bool IsChildVisible(Control child) => child == SelectedTab;
    }

    /// <summary>
    /// Represents a tab content container
    /// </summary>
    public class Tab : ContentControl
    {
        private string _title;

        /// <summary>
        /// Gets or sets the name of tab
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value;
                OnPropertyChanged();
            }
        }

        public override Rectangle MeasureBoundingBox(Control child)
        {
            return new Rectangle(0, 0, ActualWidth, ActualHeight);
        }
    }
}
