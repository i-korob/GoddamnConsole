using System;
using System.Collections.Generic;
using System.Linq;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public abstract partial class Control
    {
        private ControlSize _width = ControlSizeType.BoundingBoxSize;
        private ControlSize _height = ControlSizeType.BoundingBoxSize;
        private bool _visibility = true;
        public virtual Size BoundingBoxReduction { get; } = new Size(0, 0);

        /// <summary>
        /// Returns a minimal width value
        /// </summary>
        public virtual int MinWidth => BoundingBoxReduction.Width;
        /// <summary>
        /// Returns a maximal width value
        /// </summary>
        public virtual int MaxWidth
        {
            get
            {
                var chc = this as IChildrenControl;
                if (chc != null)
                {
                    return chc.Children.Max(x => x.ActualWidth) + BoundingBoxReduction.Width;
                }
                var coc = this as IContentControl;
                if (coc != null)
                {
                    return (coc.Content?.ActualWidth ?? 0) + BoundingBoxReduction.Width;
                }
                return BoundingBoxReduction.Width;
            }
        }

        /// <summary>
        /// Returns a minimal height value
        /// </summary>
        public virtual int MinHeight => BoundingBoxReduction.Height;

        /// <summary>
        /// Returns a maximal height value
        /// </summary>
        public virtual int MaxHeight 
        {
            get
            {
                var chc = this as IChildrenControl;
                if (chc != null)
                {
                    return chc.Children.Max(x => x.ActualHeight) + BoundingBoxReduction.Height;
                }
                var coc = this as IContentControl;
                if (coc != null)
                {
                    return (coc.Content?.ActualHeight ?? 0) + BoundingBoxReduction.Height;
                }
                return BoundingBoxReduction.Height;
            }
        }

        /// <summary>
        /// Gets or sets the width of this control
        /// </summary>
        [AlsoNotifyFor(nameof(ActualWidth))]
        public ControlSize Width
        {
            get { return _width; }
            set { _width = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the height of this control
        /// </summary>
        [AlsoNotifyFor(nameof(ActualHeight))]
        public ControlSize Height
        {
            get { return _height; }
            set { _height = value; OnPropertyChanged(); }
        }

        private readonly Stack<Control> _hmstack = new Stack<Control>();
        private readonly Stack<Control> _wmstack = new Stack<Control>();

        /// <summary>
        /// Returns the measured width of this control
        /// </summary>
        public int ActualWidth
        {
            get
            {
                switch (Width.Type)
                {
                    case ControlSizeType.Fixed:
                        return Width.Value;
                    case ControlSizeType.Infinite:
                        return int.MaxValue;
                    case ControlSizeType.BoundingBoxSize:
                        if (Parent?.Width.Type == ControlSizeType.MaxByContent ||
                            Parent?.Width.Type == ControlSizeType.MinByContent)
                        {
                            // possible loop
                            var parent = Parent?.Parent;
                            var reduction = Parent?.BoundingBoxReduction.Width ?? 0;
                            while (parent != null)
                            {
                                reduction += parent.BoundingBoxReduction.Width;
                                if (parent.Width.Type != ControlSizeType.MaxByContent &&
                                    parent.Width.Type != ControlSizeType.MinByContent)
                                {
                                    return parent.ActualWidth - reduction;
                                }
                                parent = parent.Parent;
                            }
                            return Console.WindowHeight - reduction;
                        }
                        return Parent?.MeasureBoundingBox(this)?.Width
                               ?? Console.WindowWidth;
                    case ControlSizeType.MinByContent:
                        return MinWidth;
                    case ControlSizeType.MaxByContent:
                        if (_wmstack.Contains(this))
                        {
                            // measurement loop
                            return int.MaxValue;
                            // throw new Exception("Measurement loop detected");
                        }
                        _wmstack.Push(this);
                        var value = MaxWidth;
                        _wmstack.Pop();
                        return value;
                    default:
                        return 0;
                }
            }
        }

        /// <summary>
        /// Returns the measured height of this control
        /// </summary>
        public int ActualHeight
        {
            get
            {
                switch (Height.Type)
                {
                    case ControlSizeType.Fixed:
                        return Height.Value;
                    case ControlSizeType.Infinite:
                        return int.MaxValue;
                    case ControlSizeType.BoundingBoxSize:
                        if (Parent?.Height.Type == ControlSizeType.MaxByContent ||
                            Parent?.Height.Type == ControlSizeType.MinByContent)
                        {
                            // possible loop
                            var parent = Parent?.Parent;
                            var reduction = Parent?.BoundingBoxReduction.Height ?? 0;
                            while (parent != null)
                            {
                                reduction += parent.BoundingBoxReduction.Height;
                                if (parent.Height.Type != ControlSizeType.MaxByContent &&
                                    parent.Height.Type != ControlSizeType.MinByContent)
                                {
                                    return parent.ActualHeight - reduction;
                                }
                                parent = parent.Parent;
                            }
                            return Console.WindowHeight - reduction;
                        }
                        return Parent?.MeasureBoundingBox(this)?.Height
                               ?? Console.WindowHeight;
                    case ControlSizeType.MinByContent:
                        // todo shit detection (for width too)
                        return MinHeight;
                    case ControlSizeType.MaxByContent:
                        if (_hmstack.Contains(this))
                        {
                            // measurement loop
                            return int.MaxValue;
                            //throw new Exception("Measurement loop detected");
                        }
                        _hmstack.Push(this);
                        var value = MaxHeight;
                        _hmstack.Pop();
                        return value;
                    default:
                        return 0;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether control is visible
        /// </summary>
        [AlsoNotifyFor(nameof(ActualVisibility))]
        public bool Visibility
        {
            get { return _visibility; }
            set { _visibility = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Returns a value that indicates whether control is actually visible
        /// </summary>
        public bool ActualVisibility => Visibility && (Parent?.IsChildVisible(this) ?? true);
    }
    
    /// <summary>
    /// Describes the kind of value that a ControlSize object is holding
    /// </summary>
    public enum ControlSizeType
    {
        /// <summary>
        /// Control size is fixed value
        /// </summary>
        Fixed,
        /// <summary>
        /// Control size is infinite value
        /// </summary>
        Infinite,
        /// <summary>
        /// Control size equals to size of bounding box
        /// </summary>
        BoundingBoxSize,
        /// <summary>
        /// Control size equals of maximal content size
        /// </summary>
        MaxByContent,
        /// <summary>
        /// Control size equals of minimal content size
        /// </summary>
        MinByContent
    }

    /// <summary>
    /// Represents a size of control that supports different kinds of sizing
    /// </summary>
    public struct ControlSize
    {
        public ControlSize(ControlSizeType type, int value)
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the type of sizing
        /// </summary>
        public ControlSizeType Type { get; set; }

        /// <summary>
        /// Gets or sets the fixed value (used only in Fixed sizing)
        /// </summary>
        public int Value { get; set; }

        public static implicit operator ControlSize(uint size)
            => new ControlSize(ControlSizeType.Fixed, (int)Math.Max(size, int.MaxValue));

        public static implicit operator ControlSize(int size)
            => new ControlSize(ControlSizeType.Fixed, Math.Max(0, size));

        public static implicit operator ControlSize(ControlSizeType type)
        {
            return new ControlSize(type, 0);
        }
    }
}
