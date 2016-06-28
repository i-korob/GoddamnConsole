using System;
using System.Collections.Generic;
using System.Linq;
using GoddamnConsole.Drawing;

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
            => SelectedTab == null ? new Control[0] : new Control[] {SelectedTab};
        
        public override Rectangle MeasureBoundingBox(Control child)
        {
            return child == SelectedTab
                ? new Rectangle(1, 3, ActualWidth - 2, ActualHeight - 4)
                : new Rectangle(0, 0, 0, 0) ;
        }
        
        protected override void OnKeyPressed(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.LeftArrow)
            {
                var index = Children.IndexOf(SelectedTab);
                if (index < 1) index = 1;
                index--;
                if (Children.Count <= index) return;
                SelectedTab = (Tab) Children[index];
                Invalidate();
            }
            else if (key.Key == ConsoleKey.RightArrow)
            {
                var index = Children.IndexOf(SelectedTab);
                index++;
                if (Children.Count <= index) return;
                SelectedTab = (Tab) Children[index];
                Invalidate();
            }
        }

        protected override void OnRender(DrawingContext context)
        {
            var style = Console.Focused == this
                            ? "║╠╣╩═╦"
                            : "│├┤┴─┬";
            var wpt = (ActualWidth - 1) / Children.Count - 1;
            if (wpt < 2) return;
            var li = Math.Max(ActualWidth - (wpt + 1) * Children.Count - 1, 0);
            var headerLine = style[0] + string.Concat(Children.Cast<Tab>().Select((x, i) =>
            {
                var padded = x.Name.PadLeft(x.Name.Length + 2);
                if (padded.Length > wpt - 4) padded = padded.Remove(wpt - 2);
                return padded.PadRight(wpt + (i == 0 ? li : 0)) + style[0];
            }));
            context.DrawFrame(new Rectangle(0, 0, ActualWidth, ActualHeight), 
                new FrameOptions
                {
                    Style = Console.Focused == this ? FrameStyle.Double : FrameStyle.Single,
                    Foreground = Foreground,
                    Background = Background
                });
            var txo = new TextOptions()
            {
                Background = Background,
                Foreground = Foreground
            };
            context.DrawText(new Point(0, 1), headerLine, txo);
            context.DrawText(new Point(1, 2), new string(style[4], ActualWidth - 2), txo);
            context.PutChar(new Point(0, 2), style[1], Foreground, Background, CharAttribute.None);
            context.PutChar(new Point(ActualWidth - 1, 2), style[2], Foreground, Background, CharAttribute.None);
            for (var i = 1; i < Children.Count; i++)
            {
                context.PutChar(new Point(li + i * (wpt + 1), 0), style[5], Foreground, Background, CharAttribute.None);
                context.PutChar(new Point(li + i * (wpt + 1), 2), style[3], Foreground, Background, CharAttribute.None);
            }
            if (SelectedTab != null && Children.Contains(SelectedTab))
            {
                var index = Children.IndexOf(SelectedTab);
                var padded = SelectedTab.Name.PadLeft(SelectedTab.Name.Length + 2);
                if (padded.Length > wpt - 4) padded = padded.Remove(wpt - 2);
                padded = padded.PadRight(wpt + (index == 0 ? li : 0));
                context.DrawText(new Point(1 + (index > 0 ? li : 0) + index * (wpt + 1), 1), padded, new TextOptions
                {
                    Background = Foreground,
                    Foreground = Background
                });
            }
        }

        /// <summary>
        /// Gets or sets the current tab
        /// </summary>
        public Tab SelectedTab
        {
            get { return _selectedTab; }
            set { _selectedTab = value;
                OnPropertyChanged();
            }
        }
        
        private Tab _selectedTab;

        public override bool IsChildVisible(Control child) => child == SelectedTab;
    }

    /// <summary>
    /// Represents a tab content container
    /// </summary>
    public class Tab : ContentControl
    {
        private string _name;

        /// <summary>
        /// Gets or sets the name of tab
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value;
                OnPropertyChanged();
            }
        }

        public override Rectangle MeasureBoundingBox(Control child)
        {
            return new Rectangle(0, 0, ActualWidth, ActualHeight);
        }
    }
}
