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
        private bool _drawBorders;

        public bool DrawBorders
        {
            get { return _drawBorders; }
            set { _drawBorders = value; OnPropertyChanged(); }
        }

        public override Size BoundingBoxReduction
            => DrawBorders ? new Size(Math.Max(1, ColumnDefinitions.Count) + 1, Math.Max(1, RowDefinitions.Count) + 1) : new Size(0, 0);

        public override int MaxHeight =>
            Children.GroupBy(x => x.AttachedProperty<GridProperties>()?.Column ?? 0)
                    .Max(x => x.Sum(y => y.ActualHeight)) + BoundingBoxReduction.Height;

        public override int MaxWidth =>
            Children.GroupBy(x => x.AttachedProperty<GridProperties>()?.Row ?? 0)
                    .Max(x => x.Sum(y => y.ActualWidth)) + BoundingBoxReduction.Width;

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
                    new GridSize(GridUnitType.Auto, 0)
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
                                ? children.Max(
                                    x =>
                                    x.Width.Type != ControlSizeType.BoundingBoxSize
                                        ? x.ActualWidth + (DrawBorders ? i == 0 ? 2 : 1 : 0)
                                        : long.MaxValue)
                                : children.Max(
                                    x =>
                                    x.Height.Type != ControlSizeType.BoundingBoxSize
                                        ? x.ActualHeight + (DrawBorders ? i == 0 ? 2 : 1 : 0)
                                        : long.MaxValue);
                        break;
                    case GridUnitType.Fixed:
                        sizes[i] = definitions[i].Value + (DrawBorders ? i == 0 ? 2 : 1 : 0);
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
            var x = columns.Take(column).Sum();
            var y = rows.Take(row).Sum();
            var w = columns.Skip(column).Take(columnSpan).Sum();
            var h = rows.Skip(row).Take(rowSpan).Sum();
            if (DrawBorders)
            {
                var nfc = column > 0 ? 1 : 0;
                var nfr = row > 0 ? 1 : 0;
                return new Rectangle(x + 1 - nfc, y + 1 - nfr, w - 2 + nfc, h - 2 + nfr);
                //var hdecr = column == 0 ? 2 : 1;
                //var vdecr = row == 0 ? 2 : 1;
                //if (w < vdecr || h < hdecr) return new Rectangle(0, 0, 0, 0);
                //return new Rectangle(x + (hdecr - 1), y + (vdecr - 1), w /*- hdecr*/ - 1 - columnSpan, h /*- vdecr*/ - 1 - rowSpan);
            }
            return new Rectangle(x, y, w, h);
        }

        private bool HasSpanningChildren(int row, int column, bool vertical)
        {
            return
                vertical
                    ? Children.Any(x =>
                    {
                        var atp = x.AttachedProperty<GridProperties>();
                        return ((atp?.Row ?? 0) <= row) && ((atp?.Column ?? 0) == column) &&
                               ((atp?.Row ?? 0) + (atp?.RowSpan ?? 1) - 1 > row);
                    })
                    : Children.Any(x =>
                    {
                        var atp = x.AttachedProperty<GridProperties>();
                        return ((atp?.Row ?? 0) == row) && ((atp?.Column ?? 0) <= column) &&
                               ((atp?.Column ?? 0) + (atp?.ColumnSpan ?? 1) - 1 > row);
                    });
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (!DrawBorders) return;
            for (var column = 0; column < Math.Max(1, ColumnDefinitions.Count); column++)
                for (var row = 0; row < Math.Max(1, RowDefinitions.Count); row++)
                {
                    var child = Children.FirstOrDefault(x =>
                    {
                        var atp = x.AttachedProperty<GridProperties>();
                        return ((atp?.Row ?? 0) == row) && ((atp?.Column ?? 0) == column);
                    });
                    if (child == null) continue;
                    var boundingBox = MeasureBoundingBox(child).Offset(-1, -1).Expand(2, 2);
                    dc.DrawFrame(boundingBox);
                    if (column > 0 || row > 0)
                        dc.PutChar(new Point(boundingBox.X, boundingBox.Y),
                                   column > 0
                                       ? row > 0 && !HasSpanningChildren(row - 1, column - 1, false)
                                             ? !HasSpanningChildren(row - 1, column - 1, true)
                                                   ? FrameOptions.Frames[0][10]
                                                   : FrameOptions.Frames[0][6]
                                             : FrameOptions.Frames[0][8]
                                       : !HasSpanningChildren(row - 1, column - 1, true)
                                             ? FrameOptions.Frames[0][6]
                                             : FrameOptions.Frames[0][6],
                                   Foreground, Background, CharAttribute.None);
                    if (column == Math.Max(1, ColumnDefinitions.Count) - 1 && row > 0)
                        dc.PutChar(new Point(boundingBox.X + boundingBox.Width - 1, boundingBox.Y),
                                   FrameOptions.Frames[0][7],
                                   Foreground, Background, CharAttribute.None);
                    if (row == Math.Max(1, RowDefinitions.Count) - 1 && column > 0)
                        dc.PutChar(new Point(boundingBox.X, boundingBox.Y + boundingBox.Height - 1),
                                   FrameOptions.Frames[0][9],
                                   Foreground, Background, CharAttribute.None);
                }
            //var rows = MeasureSizes(false);
            //var columns = MeasureSizes(true);
            //var rowOfs = 0;
            //for (var i = 0; i < rows.Length; i++)
            //{
            //    var ydecr = i == 0 ? 0 : 1;
            //    var colOfs = 0;
            //    for (var j = 0; j < columns.Length; j++)
            //    {
            //        var xdecr = j == 0 ? 0 : 1;
            //        if (rows[i] >= 2 && columns[j] >= 2)
            //            dc.DrawFrame(new Rectangle(colOfs - xdecr, rowOfs - ydecr, columns[j] + xdecr, rows[i] + ydecr));
            //        dc.PutChar(new Point(colOfs - xdecr, rowOfs - ydecr),
            //                   i == 0
            //                       ? j == 0
            //                             ? FrameOptions.Frames[0][2]
            //                             : FrameOptions.Frames[0][8]
            //                       : j == 0
            //                             ? FrameOptions.Frames[0][6]
            //                             : FrameOptions.Frames[0][10],
            //                   Foreground, Background,
            //                   CharAttribute.None);
            //        dc.PutChar(new Point(colOfs - xdecr + columns[j], rowOfs - ydecr),
            //                   i == 0 ? FrameOptions.Frames[0][3] : FrameOptions.Frames[0][7],
            //                   Foreground, Background,
            //                   CharAttribute.None);
            //        dc.PutChar(new Point(colOfs - xdecr, rowOfs - ydecr + rows[i]),
            //                   j == 0
            //                       ? FrameOptions.Frames[0][4]
            //                       : FrameOptions.Frames[0][9],
            //                   Foreground, Background,
            //                   CharAttribute.None);
            //        colOfs += columns[j];
            //    }
            //    rowOfs += rows[i];
            //}
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
