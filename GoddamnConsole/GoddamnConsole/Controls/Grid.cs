using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                _grid.Invalidate();
            }

            public void Clear()
            {
                var copy = _internal.ToArray();
                _internal.Clear();
                foreach (var item in copy)
                {
                    _grid.ChildRemoved?.Invoke(_grid, item);
                }
                _grid.Invalidate();
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
                _grid.Invalidate();
                return true;
            }

            public int Count => _internal.Count;
            public bool IsReadOnly { get; } = false;
        }

        public Grid()
        {
            var rd = new ObservableCollection<GridSize>();
            rd.CollectionChanged += (o, e) => Invalidate();
            RowDefinitions = rd;
            var cd = new ObservableCollection<GridSize>();
            cd.CollectionChanged += (o, e) => Invalidate();
            ColumnDefinitions = cd;
            Children = new GridChildrenCollection(this);
        }

        private struct CellSize
        {
            public int Width;
            public int Height;
            public int RowSpan;
            public int ColSpan;
        }

        private CellSize[][] MeasureControls()
        {
            var sizes =
                Enumerable
                    .Repeat(0, RowDefinitions.Count)
                    .Select(x => new CellSize[ColumnDefinitions.Count])
                    .ToArray();
            for (var row = 0; row < RowDefinitions.Count; row++)
                for (var column = 0; column < ColumnDefinitions.Count; column++)
                {
                    GridRowProperty grp = null;
                    GridColumnProperty gcp = null;
                    var child = Children.FirstOrDefault(x =>
                        ((grp = x.AttachedProperty<GridRowProperty>())?.Row ?? 0) == row &&
                        ((gcp = x.AttachedProperty<GridColumnProperty>())?.Column ?? 0) == column);
                    sizes[row][column] = child == null
                        ? new CellSize
                        {
                            ColSpan = 1,
                            RowSpan = 1
                        }
                        : new CellSize
                        {
                            Width = child.AssumedWidth,
                            Height = child.AssumedHeight,
                            ColSpan = Math.Min(gcp.ColumnSpan, 1),
                            RowSpan = Math.Min(grp.RowSpan, 1)
                        };

                }
            return sizes;
        }

        private Tuple<int[], int[]> MeasureGrid(Size maxSize)
        {
            var fixedWidth = ColumnDefinitions.Where(x => x.UnitType == GridUnitType.Fixed).Select(x => x.Value).Sum();
            var fixedHeight = RowDefinitions.Where(x => x.UnitType == GridUnitType.Fixed).Select(x => x.Value).Sum();
            var sizes = (CellSize[][]) MeasureControls().Clone();
            for (var row = 0; row < RowDefinitions.Count; row++)
            {
                var rd = RowDefinitions[row];
                for (var column = 0; column < ColumnDefinitions.Count; column++)
                {
                    var cd = ColumnDefinitions[column];
                    if (rd.UnitType == GridUnitType.Fixed)
                        sizes[row][column].Height = rd.Value;
                    else if (rd.UnitType == GridUnitType.Grow) sizes[row][column].Height = 0;
                    if (cd.UnitType == GridUnitType.Fixed)
                        sizes[row][column].Width = cd.Value;
                    else if (cd.UnitType == GridUnitType.Grow) sizes[row][column].Width = 0;
                }
            }
            var widest =
                Enumerable.Range(0, sizes[0].Length)
                          .Select(x => sizes.Select(y => y[x]).OrderByDescending(y => y.Width).First()).ToArray();
            var widestWidth = widest.Sum(x => (long) x.Width);
            if (widestWidth > maxSize.Width)
            {
                var remainingAutoWidth = maxSize.Width - fixedWidth;
                var decreaseBy = ((double) widestWidth - fixedWidth)/remainingAutoWidth;
                for (var column = 0; column < ColumnDefinitions.Count; column++)
                {
                    var cd = ColumnDefinitions[column];
                    if (cd.UnitType == GridUnitType.Auto)
                        widest[column].Width = (int) (widest[column].Width/decreaseBy);
                }
            }
            else
            {
                var remainingGrowWidth = maxSize.Width - widestWidth;
                var totalWidthGrowValue = ColumnDefinitions.Where(x => x.UnitType == GridUnitType.Grow)
                    .Sum(x => x.Value);
                for (var column = 0; column < ColumnDefinitions.Count; column++)
                {
                    var cd = ColumnDefinitions[column];
                    if (cd.UnitType == GridUnitType.Grow)
                        widest[column].Width =
                            (int) ((double) remainingGrowWidth/totalWidthGrowValue*cd.Value);
                }
            }
            var highest =
                sizes
                    .Select(x =>
                            x.OrderByDescending(y => (long) y.Height).First()).ToArray();
            var highestHeight = highest.Sum(x => (long) x.Height);
            if (highestHeight > maxSize.Height)
            {
                var remainingAutoHeight = maxSize.Height - fixedHeight;
                var decreaseBy = ((double) highestHeight - fixedHeight)/remainingAutoHeight;
                for (var row = 0; row < RowDefinitions.Count; row++)
                {
                    var rd = RowDefinitions[row];
                    if (rd.UnitType == GridUnitType.Auto)
                        highest[row].Height = (int) (highest[row].Height /decreaseBy);
                }
            }
            else
            {
                var remainingGrowHeight = maxSize.Height - highestHeight;
                var totalHeightGrowValue = RowDefinitions.Where(x => x.UnitType == GridUnitType.Grow).Sum(x => x.Value);
                for (var row = 0; row < RowDefinitions.Count; row++)
                {
                    var rd = RowDefinitions[row];
                    if (highest[row].RowSpan > 1 || highest[row].ColSpan > 1) continue;
                    if (rd.UnitType == GridUnitType.Grow)
                        highest[row].Width =
                            (int) ((double) remainingGrowHeight/totalHeightGrowValue*rd.Value);
                }
            }
            return new Tuple<int[], int[]>(widest.Select(x => x.Width).ToArray(),
                highest.Select(x => x.Height).ToArray());
        }

        public override void Render(DrawingContext context)
        {
            var sizes = MeasureGrid(new Size(ActualWidth, ActualHeight));
            foreach (var child in Children)
            {
                var grp = child.AttachedProperties.FirstOrDefault(x => x is GridRowProperty) as GridRowProperty;
                var gcp = child.AttachedProperties.FirstOrDefault(x => x is GridColumnProperty) as GridColumnProperty;
                var row = grp?.Row ?? 0;
                var column = gcp?.Column ?? 0;
                var rowSpan = Math.Max(grp?.RowSpan ?? 1, 1);
                var columnSpan = Math.Max(gcp?.ColumnSpan ?? 1, 1);
                child.Render(context.Shrink(
                    new Rectangle(
                        sizes.Item1.Take(column).Sum(),
                        sizes.Item2.Take(row).Sum(),
                        Math.Min(child.AssumedWidth, sizes.Item1.Skip(column).Take(columnSpan).Sum()),
                        Math.Min(child.AssumedHeight, sizes.Item2.Skip(row).Take(rowSpan).Sum()))));
            }
        }

        public Size MeasureChild(Control child)
        {
            var sizes = MeasureGrid(new Size(ActualWidth, ActualHeight));
            var grp = child.AttachedProperties.FirstOrDefault(x => x is GridRowProperty) as GridRowProperty;
            var gcp = child.AttachedProperties.FirstOrDefault(x => x is GridColumnProperty) as GridColumnProperty;
            var row = grp?.Row ?? 0;
            var column = gcp?.Column ?? 0;
            var rowSpan = Math.Max(grp?.RowSpan ?? 1, 1);
            var columnSpan = Math.Max(gcp?.ColumnSpan ?? 1, 1);
            return new Size(
                Math.Min(child.AssumedWidth, sizes.Item1.Skip(column).Take(columnSpan).Sum()),
                Math.Min(child.AssumedHeight, sizes.Item2.Skip(row).Take(rowSpan).Sum()));
        }

        public IList<GridSize> RowDefinitions { get; }
        public IList<GridSize> ColumnDefinitions { get; }

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

    public enum GridUnitType
    {
        Grow,
        Auto,
        Fixed
    }

    public class GridSize
    {
        public GridSize(GridUnitType unit, int val)
        {
            UnitType = unit;
            Value = val;
        }

        public int Value { get; set; }

        public GridUnitType UnitType { get; set; }
    }
}