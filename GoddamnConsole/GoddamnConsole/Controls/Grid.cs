using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class Grid : Control, IChildrenControl
    {
        public class GridChildrenCollection : ICollection<Control>
        {
            public GridChildrenCollection(Grid grid)
            {
                _grid = grid;
            }

            private readonly Grid _grid;
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
                if (item.Parent != _grid)
                {
                    item.Parent = _grid;
                    return;
                }
                _internal.Add(item);
            }

            public void Clear()
            {
                var copy = _internal.ToArray();
                _internal.Clear();
                foreach (var item in copy)
                {
                    _grid.ChildRemoved?.Invoke(_grid, item);
                }
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
                _grid.ChildRemoved?.Invoke(_grid, item);
                return false;
            }

            public int Count => _internal.Count;
            public bool IsReadOnly { get; } = false;
        }

        public Grid()
        {
            Children = new GridChildrenCollection(this);
        }

        public override void Render(DrawingContext context)
        {
            var rowCount =
                Children.Max(
                    xx =>
                    {
                        var prop =
                            (xx.AttachedProperties.FirstOrDefault(x => x is GridRowProperty) as GridRowProperty);
                        return prop != null ? prop.Row + Math.Min(prop.RowSpan, 1) : -1;
                    }) + 1;
            var columnCount =
                Children.Max(
                    xx =>
                    {
                        var prop =
                            (xx.AttachedProperties.FirstOrDefault(x => x is GridColumnProperty) as GridColumnProperty);
                        return prop != null ? prop.Column + Math.Min(prop.ColumnSpan, 1) : -1;
                    }) + 1;
            if (rowCount == 0 || columnCount == 0) return;
            var cellWidth = ActualWidth/columnCount;
            var cellHeight = ActualHeight/rowCount;
            foreach (var child in Children)
            {
                var grp = (child.AttachedProperties.FirstOrDefault(x => x is GridRowProperty) as GridRowProperty);
                var gcp = (child.AttachedProperties.FirstOrDefault(x => x is GridColumnProperty) as GridColumnProperty);
                var row = grp?.Row ?? 0;
                var rowspan = Math.Max(1, grp?.RowSpan ?? 1);
                var column = gcp?.Column ?? 0;
                var colspan = Math.Max(1, gcp?.ColumnSpan ?? 1);
                child.Render(context.Shrink(new Rectangle(column*cellWidth, row*cellHeight, cellWidth * colspan, cellHeight * rowspan)));
            }
        }

        public Size MeasureChild(Control child)
        {
            var rowCount =
                Children.Max(
                    xx =>
                    {
                        var prop =
                            (xx.AttachedProperties.FirstOrDefault(x => x is GridRowProperty) as GridRowProperty);
                        return prop != null ? prop.Row + Math.Min(prop.RowSpan, 1) : -1;
                    }) + 1;
            var columnCount =
                Children.Max(
                    xx =>
                    {
                        var prop =
                            (xx.AttachedProperties.FirstOrDefault(x => x is GridColumnProperty) as GridColumnProperty);
                        return prop != null ? prop.Column + Math.Min(prop.ColumnSpan, 1) : -1;
                    }) + 1;
            var grp = (child.AttachedProperties.FirstOrDefault(x => x is GridRowProperty) as GridRowProperty);
            var gcp = (child.AttachedProperties.FirstOrDefault(x => x is GridColumnProperty) as GridColumnProperty);
            var rowspan = Math.Max(1, grp?.RowSpan ?? 1);
            var colspan = Math.Max(1, gcp?.ColumnSpan ?? 1);
            if (rowCount == 0 || columnCount == 0) return new Size(0, 0);
            return new Size(ActualWidth/columnCount*colspan, ActualHeight/rowCount*rowspan);
        }

        public ICollection<Control> Children { get; }

        public event EventHandler<Control> ChildRemoved;
    }

    public class GridRowProperty : IAttachedProperty
    {
        public int Row { get; set; }
        public int RowSpan { get; set; }
    }

    public class GridColumnProperty : IAttachedProperty
    {
        public int Column { get; set; }
        public int ColumnSpan { get; set; }
    }
}
