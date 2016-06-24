using System;
using System.Collections.Generic;
using System.Linq;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    /// <summary>
    /// Represents a flexible grid area that consists of rows and columns. Child elements of the Grid are measured and arranged according to their row/column assignments
    /// </summary>
    public class Grid : ChildrenControl
    {
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
            return new Rectangle(
                columns.Take(column).Sum(),
                rows.Take(row).Sum(),
                columns.Skip(column).Take(columnSpan).Sum(),
                rows.Skip(row).Take(rowSpan).Sum());
        }
    }

    /// <summary>
    /// Describes the kind of value that a GridUnitType object is holding
    /// </summary>
    public enum GridUnitType
    {
        /// <summary>
        /// The value is expressed as a weighted proportion of available space
        /// </summary>
        Grow,
        /// <summary>
        /// The size is determined by the size of content object
        /// </summary>
        Auto,
        /// <summary>
        /// The value is expressed in pixels
        /// </summary>
        Fixed
    }

    /// <summary>
    /// Represents a row/column size value
    /// </summary>
    public class GridSize
    {
        public GridSize(GridUnitType unit, int val)
        {
            UnitType = unit;
            Value = Math.Max(0, val);
        }

        /// <summary>
        /// Gets or sets the fixed value (used only in Fixed sizing)
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Gets or sets the type of sizing
        /// </summary>
        public GridUnitType UnitType { get; set; }
    }

    /// <summary>
    /// Represents an attached property for Grid control
    /// </summary>
    public class GridProperties : IAttachedProperty
    {
        /// <summary>
        /// Gets or sets the row alignment
        /// </summary>
        public int Row { get; set; }
        /// <summary>
        /// Gets or sets a value that indicates the total numbers of occupied rows
        /// </summary>
        public int RowSpan { get; set; }
        /// <summary>
        /// Gets or sets the column alignment
        /// </summary>
        public int Column { get; set; }
        /// <summary>
        /// Gets or sets a value that indicates the total numbers of occupied columns
        /// </summary>
        public int ColumnSpan { get; set; }
    }
}
