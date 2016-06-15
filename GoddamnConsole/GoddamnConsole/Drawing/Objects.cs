using System;

namespace GoddamnConsole.Drawing
{
    public class Size
    {
        public Size(int w, int h)
        {
            Width = w;
            Height = h;
        }

        public int Width { get; }
        public int Height { get; }
    }

    public class Point
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public Point Offset(int x, int y) => new Point(X + x, Y + y);

        public Rectangle CreateRectangle(Point second)
        {
            var minX = Math.Min(X, second.X);
            var minY = Math.Min(Y, second.Y);
            var maxX = Math.Max(X, second.X);
            var maxY = Math.Max(Y, second.Y);
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }
    }

    public class Rectangle
    {
        public Rectangle(int x, int y, int width, int height)
        {
            if (width < 0) width = 0; //throw new ArgumentException(nameof(width));
            if (height < 0) height = 0; //throw new ArgumentException(nameof(height));
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public Rectangle Offset(int x, int y)
            => new Rectangle(X + x, Y + y, Width, Height);

        public Rectangle Expand(int width, int height)
            => new Rectangle(X, Y, Width + width, Height + height);

        public Rectangle Clip(int x, int y, int width, int height)
        {
            var nx = X < x ? x : X;
            var ny = Y < y ? y : Y;
            var ocw = Width - (nx - X);
            var och = Height - (ny - Y);
            var nw = ocw > width ? width : ocw < 0 ? 0 : ocw;
            var nh = och > height ? height : och < 0 ? 0 : och;
            return new Rectangle(nx, ny, nw, nh);
        }
    }

    public class CommonOptions
    {
        public CharColor Foreground { get; set; } = CharColor.White;
        public CharColor Background { get; set; } = CharColor.Black;
        public CharAttribute Attributes { get; set; } = CharAttribute.None;
    }

    public class RectangleOptions : CommonOptions
    {

    }

    public class TextOptions : CommonOptions
    {
        public TextWrapping TextWrapping { get; set; } = TextWrapping.NoWrap;
    }

    public class FrameOptions : CommonOptions
    {
        internal static string[] Frames =
        {
            "─│┌┐└┘",
            "═║╔╗╚╝",
            "██████",
            "-|++++"
        };

        public FrameStyle Style { get; set; }
    }

    public enum TextWrapping
    {
        Wrap,
        NoWrap
    }

    public enum FrameStyle 
    {
        Single = 0,
        Double = 1,
        Fill = 2,
        Simple = 3
    }
}