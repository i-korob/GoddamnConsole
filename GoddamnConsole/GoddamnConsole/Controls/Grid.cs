using System;
using System.Collections.Generic;
using System.Linq;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class Grid : ChildrenControl
    {
        public IList<GridSize> RowDefinitions { get; } = new List<GridSize>();
        public IList<GridSize> ColumnDefinitions { get; } = new List<GridSize>();

        public int[] MeasureSizes(bool measureColumns)
        {
            var boxSize = measureColumns ? ActualWidth : ActualHeight;
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
            return sizes.Select(x => (int) x).ToArray();
        }

        public override void Render(DrawingContext context)
        {
            foreach (var child in Children)
            {
                var rows = MeasureSizes(false);
                var columns = MeasureSizes(true);
                var props = child.AttachedProperty<GridProperties>() ?? new GridProperties();
                var row = Math.Max(0, Math.Min(props.Row, rows.Length));
                var column = Math.Max(0, Math.Min(props.Column, columns.Length));
                var rowSpan = Math.Max(1, Math.Min(props.RowSpan, rows.Length - row));
                var columnSpan = Math.Max(1, Math.Min(props.ColumnSpan, columns.Length - column));
                child.Render(context.Shrink(
                    new Rectangle(
                        columns.Take(column).Sum(),
                        rows.Take(row).Sum(),
                        columns.Skip(column).Take(columnSpan).Sum(),
                        rows.Skip(row).Take(rowSpan).Sum())));
            }
        }

        public override Size MeasureBoundingBox(Control child)
        {
            if (!Children.Contains(child)) return new Size(0, 0);
            var rows = MeasureSizes(false);
            var columns = MeasureSizes(true);
            var props = child.AttachedProperty<GridProperties>() ?? new GridProperties();
            var row = Math.Max(0, Math.Min(props.Row, rows.Length));
            var column = Math.Max(0, Math.Min(props.Column, columns.Length));
            var rowSpan = Math.Max(1, Math.Min(props.RowSpan, rows.Length - row));
            var columnSpan = Math.Max(1, Math.Min(props.ColumnSpan, columns.Length - column));
            return new Size(
                columns.Skip(column).Take(columnSpan).Sum(),
                rows.Skip(row).Take(rowSpan).Sum());
        }
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
            Value = Math.Max(0, val);
        }

        public int Value { get; set; }

        public GridUnitType UnitType { get; set; }
    }

    public class GridProperties : IAttachedProperty
    {
        public int Row { get; set; }
        public int RowSpan { get; set; }
        public int Column { get; set; }
        public int ColumnSpan { get; set; }
    }
}
