using System;

namespace GoddamnConsole.Controls
{
    public abstract partial class Control
    {
        private ControlSize _width = ControlSizeType.BoundingBoxSize;
        private ControlSize _height = ControlSizeType.BoundingBoxSize;
        private bool _visibility = true;
        public virtual int MinWidth { get; } = 0;
        public virtual int MaxWidth { get; } = int.MaxValue;
        public virtual int MinHeight { get; } = 0;
        public virtual int MaxHeight { get; } = int.MaxValue;

        [AlsoNotifyFor(nameof(ActualWidth))]
        public ControlSize Width
        {
            get { return _width; }
            set { _width = value; OnPropertyChanged(); }
        }

        [AlsoNotifyFor(nameof(ActualHeight))]
        public ControlSize Height
        {
            get { return _height; }
            set { _height = value; OnPropertyChanged(); }
        }

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
                        return Parent?.MeasureBoundingBox(this)?.Width
                            ?? (this == Console.Root ? Console.WindowWidth : 0);
                    case ControlSizeType.MinByContent:
                        return MinWidth;
                    case ControlSizeType.MaxByContent:
                        return MaxWidth;
                    default:
                        return 0;
                }
            }
        }

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
                        return Parent?.MeasureBoundingBox(this)?.Height
                            ?? (this == Console.Root ? Console.WindowHeight : 0);
                    case ControlSizeType.MinByContent:
                        return MinHeight;
                    case ControlSizeType.MaxByContent:
                        return MaxHeight;
                    default:
                        return 0;
                }
            }
        }

        [AlsoNotifyFor(nameof(ActualVisibility))]
        public bool Visibility
        {
            get { return _visibility; }
            set { _visibility = value; OnPropertyChanged(); }
        }

        public bool ActualVisibility => Visibility && (Parent?.IsChildVisible(this) ?? true);
    }
    
    public enum ControlSizeType
    {
        Fixed,
        Infinite,
        BoundingBoxSize,
        MaxByContent,
        MinByContent
    }

    public struct ControlSize
    {
        public ControlSize(ControlSizeType type, int value)
        {
            Type = type;
            Value = value;
        }

        public ControlSizeType Type { get; set; }

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
