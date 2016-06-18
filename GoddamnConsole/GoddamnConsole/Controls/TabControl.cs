using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class TabControl : Control, IChildrenControl
    {
        public class TabControlChildrenCollection : IList<Control>
        {
            public TabControlChildrenCollection(TabControl tabControl)
            {
                _tabControl = tabControl;
            }

            private readonly TabControl _tabControl;
            private readonly List<Control> _internal = new List<Control>();

            public IEnumerator<Control> GetEnumerator()
            {
                return _internal.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _internal.GetEnumerator();
            }

            public void Add(Control item)
            {
                if (!(item is Tab)) throw new ArgumentException("Only Tab controls can be added to TabControl");
                if (item.Parent != _tabControl)
                {
                    item.Parent = _tabControl;
                    return;
                }
                _internal.Add(item);
                _tabControl.Invalidate();
            }

            public void Clear()
            {
                var copy = _internal.ToArray();
                _internal.Clear();
                foreach (var item in copy)
                {
                    _tabControl.ChildRemoved?.Invoke(_tabControl, new ChildRemovedEventArgs(item));
                }
                _tabControl.Invalidate();
            }

            public bool Contains(Control item)
            {
                return _internal.Contains(item);
            }

            public void CopyTo(Control[] array, int arrayIndex)
            {
                _internal.CopyTo(array, arrayIndex);
            }

            public bool Remove(Control item)
            {
                if (!Contains(item)) return false;
                _internal.Remove(item);
                _tabControl.ChildRemoved?.Invoke(_tabControl, new ChildRemovedEventArgs(item));
                _tabControl.Invalidate();
                return true;
            }

            public int Count => _internal.Count;
            public bool IsReadOnly { get; } = false;

            public int IndexOf(Control item)
            {
                return _internal.IndexOf(item);
            }

            public void Insert(int index, Control item)
            {
                _internal.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                _internal.RemoveAt(index);
            }

            public Control this[int index]
            {
                get { return _internal[index]; }
                set { _internal[index] = value; }
            }
        }

        public TabControl()
        {
            Focusable = true;
            Children = _children = new TabControlChildrenCollection(this);
        }

        public Size MeasureChild(Control child)
        {
            return child == SelectedTab?.Content 
                ? new Size(0, 0) 
                : new Size(Math.Min(ActualWidth - 2, child.AssumedWidth), Math.Min(ActualHeight - 4, child.AssumedHeight));
        }

        public Size MeasureMaxRealSize()
        {
            return new Size(ActualWidth - 2, ActualHeight - 4);
        }

        protected override void OnKeyPress(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.LeftArrow)
            {
                var index = _children.IndexOf(SelectedTab);
                if (index < 1) index = 1;
                index--;
                if (_children.Count <= index) return;
                SelectedTab = (Tab) _children[index];
                Invalidate();
            }
            else if (key.Key == ConsoleKey.RightArrow)
            {
                var index = _children.IndexOf(SelectedTab);
                index++;
                if (_children.Count <= index) return;
                SelectedTab = (Tab) _children[index];
                Invalidate();
            }
        }

        public override void Render(DrawingContext context)
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
                    Style = Console.Focused == this ? FrameStyle.Double : FrameStyle.Single
                });
            context.DrawText(new Point(0, 1), headerLine);
            context.DrawText(new Point(1, 2), new string(style[4], ActualWidth - 2));
            context.PutChar(new Point(0, 2), style[1], CharColor.White, CharColor.Black, CharAttribute.None);
            context.PutChar(new Point(ActualWidth - 1, 2), style[2], CharColor.White, CharColor.Black, CharAttribute.None);
            for (var i = 1; i < Children.Count; i++)
            {
                context.PutChar(new Point(li + i * (wpt + 1), 0), style[5], CharColor.White, CharColor.Black, CharAttribute.None);
                context.PutChar(new Point(li + i * (wpt + 1), 2), style[3], CharColor.White, CharColor.Black, CharAttribute.None);
            }
            if (SelectedTab != null && _children.Contains(SelectedTab))
            {
                var index = _children.IndexOf(SelectedTab);
                var padded = SelectedTab.Name.PadLeft(SelectedTab.Name.Length + 2);
                if (padded.Length > wpt - 4) padded = padded.Remove(wpt - 2);
                padded = padded.PadRight(wpt + (index == 0 ? li : 0));
                context.DrawText(new Point(1 + (index > 0 ? li : 0) + index * (wpt + 1), 1), padded, new TextOptions
                {
                    Background = CharColor.White,
                    Foreground = CharColor.Black
                });
            }
            SelectedTab?.Content?.Render(context.Shrink(new Rectangle(1, 3, ActualWidth - 2, ActualHeight - 4)));
        }

        public Tab SelectedTab
        {
            get { return _selectedTab; }
            set { _selectedTab = value;
                OnPropertyChanged();
            }
        }

        private readonly TabControlChildrenCollection _children;
        private Tab _selectedTab;
        public ICollection<Control> Children { get; }
        public ICollection<Control> FocusableChildren
            => SelectedTab != null ? new List<Control> { SelectedTab } : new List<Control>();
        public event EventHandler<ChildRemovedEventArgs> ChildRemoved;
    }

    public class Tab : ContentControl
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value;
                OnPropertyChanged();
            }
        }

        public override Size MeasureChild(Control child)
        {
            return new Size(Math.Min(ActualWidth, child.AssumedWidth), Math.Min(ActualHeight, child.AssumedHeight));
        }
    }
}
