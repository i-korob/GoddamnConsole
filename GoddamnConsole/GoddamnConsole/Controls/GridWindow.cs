using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class GridWindow : WindowBase, IChildrenControl
    {
        public GridWindow()
        {
            Children = new ChildrenCollection(this);
        }

        private bool _drawBorders = true;

        public bool DrawBorders
        {
            get { return _drawBorders; }
            set { _drawBorders = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Returns a collection of row definitions
        /// </summary>
        public IList<GridSize> RowDefinitions { get; } = new List<GridSize>();
        /// <summary>
        /// Returns a collection of column definitions
        /// </summary>
        public IList<GridSize> ColumnDefinitions { get; } = new List<GridSize>();

        private int[] MeasureSizes(bool measureColumns)
        {
            var boxSize = measureColumns
                              ? DrawBorders ? ActualWidth : ActualWidth - 2
                              : DrawBorders ? ActualHeight : ActualHeight - 2;
            var definitions = measureColumns ? ColumnDefinitions : RowDefinitions;
            if (definitions.Count == 0)
                definitions = new[]
                {
                    new GridSize(GridUnitType.Fixed, boxSize)
                };
            var sizes = new long[definitions.Count];
            for (var i = 0; i < definitions.Count; i++)
            {
                switch (definitions[i].UnitType)
                {
                    case GridUnitType.Auto:
                        var children = Children.Where(x =>
                        {
                            var props = x.AttachedProperty<GridProperties>() ?? new GridProperties();
                            // ReSharper disable AccessToModifiedClosure
                            return measureColumns ? props.Column == i : props.Row == i;
                            // ReSharper restore AccessToModifiedClosure
                        });
                        sizes[i] =
                            measureColumns
                                ? children.Max(x => x.Width.Type != ControlSizeType.BoundingBoxSize ? x.ActualWidth : long.MaxValue)
                                : children.Max(
                                    x => x.Height.Type != ControlSizeType.BoundingBoxSize ? x.ActualHeight : long.MaxValue);
                        break;
                    case GridUnitType.Fixed:
                        sizes[i] = definitions[i].Value;
                        break;
                    case GridUnitType.Grow:
                        break; // process later
                }
            }
            var remaining = boxSize - sizes.Sum(x => x != long.MaxValue ? x : 0);
            var alignedToBox = sizes.Count(x => x == long.MaxValue);
            if (alignedToBox > 0)
            {
                var size = remaining / alignedToBox;
                var first = true;
                for (var i = 0; i < sizes.Length; i++)
                {
                    if (sizes[i] == long.MaxValue)
                    {
                        if (first)
                        {
                            sizes[i] = size + remaining % alignedToBox;
                            first = false;
                        }
                        else sizes[i] = size;
                    }
                }
            }
            else if (remaining > 0)
            {
                var totalGrowRate = definitions.Sum(x => x.UnitType == GridUnitType.Grow ? x.Value : 0);
                if (totalGrowRate > 0)
                {
                    var growUnit = remaining / totalGrowRate;
                    var first = true;
                    for (var i = 0; i < sizes.Length; i++)
                    {
                        if (definitions[i].UnitType == GridUnitType.Grow)
                        {
                            if (first)
                            {
                                sizes[i] = definitions[i].Value * growUnit + remaining % growUnit;
                                first = false;
                            }
                            else sizes[i] = definitions[i].Value * growUnit;
                        }
                    }
                }
            }
            else
            {
                for (var i = 0; i < sizes.Length; i++) if (sizes[i] < 0) sizes[i] = 0;
            }
            return sizes.Select(x => (int)x).ToArray();
        }

        public override Rectangle MeasureBoundingBox(Control child)
        {
            if (!Children.Contains(child)) return new Rectangle(0, 0, 0, 0);
            var rows = MeasureSizes(false);
            var columns = MeasureSizes(true);
            var props = child.AttachedProperty<GridProperties>() ?? new GridProperties();
            var row = Math.Max(0, Math.Min(props.Row, rows.Length));
            var column = Math.Max(0, Math.Min(props.Column, columns.Length));
            var rowSpan = Math.Max(1, Math.Min(props.RowSpan, rows.Length - row));
            var columnSpan = Math.Max(1, Math.Min(props.ColumnSpan, columns.Length - column));
            var x = columns.Take(column).Sum();
            var y = rows.Take(row).Sum();
            var w = columns.Skip(column).Take(columnSpan).Sum();
            var h = rows.Skip(row).Take(rowSpan).Sum();
            if (DrawBorders)
            {
                var vdecr = column == 0 ? 2 : 1;
                var hdecr = row == 0 ? 2 : 1;
                if (w < vdecr || h < hdecr) return new Rectangle(0, 0, 0, 0);
                return new Rectangle(x + (vdecr - 1), y + (hdecr - 1), w - vdecr, h - hdecr);
            }
            return new Rectangle(x + 1, y + 1, w, h);
        }

        public override Point GetScrollOffset(Control child)
        {
            return new Point(0, 0);
        }

        public override bool IsChildVisible(Control child)
        {
            return true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            var style = Console.FocusedWindow == this ? FrameStyle.Double : FrameStyle.Single;
            dc.DrawFrame(new Rectangle(0, 0, ActualWidth, ActualHeight), new FrameOptions
            {
                Style = style,
                Foreground = Foreground,
                Background = Background
            });
            var truncated = Title.Length + 2 > ActualWidth - 4
                                ? ActualWidth < 9
                                      ? string.Empty
                                      : $" {Title.Remove(ActualWidth - 9)}... "
                                : ActualWidth < 9
                                      ? string.Empty
                                      : $" {Title} ";
            if (!DrawBorders)
            {
                dc.DrawText(new Point(2, 0), truncated, new TextOptions
                {
                    Foreground = Foreground,
                    Background = Background
                });
                return;
            }
            var sti = Console.FocusedWindow == this ? 1 : 0;
            var rows = MeasureSizes(false);
            var columns = MeasureSizes(true);
            var rowOfs = 0;
            for (var i = 0; i < rows.Length; i++)
            {
                var ydecr = i == 0 ? 0 : 1;
                var colOfs = 0;
                for (var j = 0; j < columns.Length; j++)
                {
                    var xdecr = j == 0 ? 0 : 1;
                    if (rows[i] >= 2 && columns[j] >= 2)
                        dc.DrawFrame(new Rectangle(colOfs - xdecr, rowOfs - ydecr, columns[j] + xdecr, rows[i] + ydecr),
                            new FrameOptions
                            {
                                Style = style
                            });
                    dc.PutChar(new Point(colOfs - xdecr, rowOfs - ydecr),
                               i == 0
                                   ? j == 0
                                         ? FrameOptions.Frames[sti][2]
                                         : FrameOptions.Frames[sti][8]
                                   : j == 0
                                         ? FrameOptions.Frames[sti][6]
                                         : FrameOptions.Frames[sti][10],
                               Foreground, Background,
                               CharAttribute.None);
                    dc.PutChar(new Point(colOfs - xdecr + columns[j], rowOfs - ydecr),
                               i == 0 ? FrameOptions.Frames[sti][3] : FrameOptions.Frames[sti][7],
                               Foreground, Background,
                               CharAttribute.None);
                    dc.PutChar(new Point(colOfs - xdecr, rowOfs - ydecr + rows[i]),
                               j == 0
                                   ? FrameOptions.Frames[sti][4]
                                   : FrameOptions.Frames[sti][9],
                               Foreground, Background,
                               CharAttribute.None);
                    colOfs += columns[j];
                }
                rowOfs += rows[i];
            }
            dc.DrawText(new Point(2, 0), truncated, new TextOptions
            {
                Foreground = Foreground,
                Background = Background
            });
        }

        public IList<Control> Children { get; }
        public IList<Control> FocusableChildren => Children;
        public event EventHandler<ChildRemovedEventArgs> ChildRemoved;

        private class ChildrenCollection : IList<Control>
        {
            public ChildrenCollection(GridWindow parent)
            {
                _parent = parent;
            }

            private readonly GridWindow _parent;
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
                if (item.Parent != _parent)
                {
                    item.Parent = _parent;
                    return;
                }
                _internal.Add(item);
                _parent.Invalidate();
            }

            public void Clear()
            {
                var copy = _internal.ToArray();
                _internal.Clear();
                foreach (var item in copy)
                {
                    _parent.ChildRemoved?.Invoke(_parent, new ChildRemovedEventArgs(item));
                }
                _parent.Invalidate();
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
                _parent.ChildRemoved?.Invoke(_parent, new ChildRemovedEventArgs(item));
                _parent.Invalidate();
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
    }
}
