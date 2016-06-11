using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GoddamnConsole.Drawing
{
    internal sealed class RealDrawingContext : DrawingContext
    {
        public RealDrawingContext()
        {
            _width = Console.WindowWidth;
            _height = Console.WindowHeight;
            _x = _scrollX = _y = _scrollY = 0;
        }

        private int _scrollX;
        private int _scrollY; 
        private int _x;
        private int _y;
        private int _width;
        private int _height;

        public override DrawingContext Scroll(Point sourceOffset)
        {
            return new RealDrawingContext
            {
                _x = _x,
                _y = _y,
                _width = _width,
                _height = _height,
                _scrollX = _scrollX + sourceOffset.X,
                _scrollY = _scrollY + sourceOffset.Y
            };
        }

        public override DrawingContext Shrink(Point sourceOffset, Rectangle targetArea)
        {
            return Shrink(targetArea).Scroll(sourceOffset);
        }

        public override DrawingContext Shrink(Rectangle targetArea)
        {
            if (targetArea.X < 0 || targetArea.Y < 0) throw new ArgumentException(nameof(targetArea));
            var nx = _x + targetArea.X;
            var ny = _y + targetArea.Y;
            var nw = Math.Min(_width - targetArea.X, targetArea.Width);
            var nh = Math.Min(_height - targetArea.Y, targetArea.Height);
            if (nw <= 0 || nh <= 0) return new ImaginaryDrawingContext();
            return new RealDrawingContext
            {
                _x = nx,
                _y = ny,
                _width = nw,
                _height = nh
            };
        }

        public override void Clear()
        {
            for (var x = 0; x < _width; x++)
                for (var y = 0; y < _height; y++)
                {
                    Console.Provider.PutChar(new Character(' '), x + _x, y + _y);
                }
        }

        public override void PutChar(Point pt, char chr, CharColor foreground, CharColor background, CharAttribute attribute)
        {
            pt = pt.Offset(_scrollX, _scrollY);
            if (pt.X < 0 || pt.Y < 0 || pt.X >= _width || pt.Y >= _height) return;
            Console.Provider.PutChar(new Character(chr, foreground, background, attribute), 
                pt.X + _x, pt.Y + _y);
        }

        public override void DrawRectangle(Rectangle rect, char fill, RectangleOptions opts = null)
        {
            var clippedRect = rect.Clip(0, 0, _width, _height);
            if (clippedRect.Width == 0 || clippedRect.Height == 0) return;
            for (var x = clippedRect.X; x < clippedRect.X + clippedRect.Width; x++)
                for (var y = clippedRect.Y; y < clippedRect.Y + clippedRect.Height; y++)
                {
                    PutChar(new Point(x, y), fill, opts?.Foreground ?? CharColor.Gray,
                        opts?.Background ?? CharColor.Black, opts?.Attributes ?? CharAttribute.None);
                }
        }

        public override void DrawText(Point point, string line, TextOptions opts = null)
        {
            if (point.Y < 0 || point.Y >= _height) return;
            line = Regex.Replace(line, "[\r\n\t\f]", " ");
            for (int x = point.X, i = 0; x < Math.Min(line.Length, _width - point.X - _scrollX) + point.X; x++, i++)
            {
                PutChar(
                    new Point(x, point.Y), 
                    line[i], 
                    opts?.Foreground ?? CharColor.Gray, 
                    opts?.Background ?? CharColor.Black, 
                    opts?.Attributes ?? CharAttribute.None);
            }
        }

        public override void DrawText(Rectangle rect, string text, TextOptions opts = null)
        {
            var maxWid = Math.Min(rect.Width + rect.X, _width) - rect.X;
            IEnumerable<string> lines = text.Replace("\r\n", "\n").Split('\n');
            lines = (opts?.TextWrapping ?? TextWrapping.NoWrap) == TextWrapping.Wrap
                ? lines.SelectMany(x => x.Split(maxWid/* -_scrollX*/)) // wrapped text shouldn't be influenced by scrolling 
                : lines.Select(x => x.Length > maxWid - _scrollX ? x.Remove(maxWid - _scrollX) : x);
            if (rect.X < 0)
                lines = lines.Select(x => x.Length > -rect.X ? x.Substring(-rect.X) : "");
            var skip = rect.Y < 0 ? -rect.Y : 0;
            var xOfs = rect.X > 0 ? rect.X : 0;
            var yOfs = rect.Y > 0 ? rect.Y : 0;
            foreach (var line in lines.Skip(skip).Take(Math.Min(rect.Height - _scrollY, _height - rect.Y - _scrollY)))
            {
                DrawText(new Point(xOfs, yOfs++), line, opts);
            }
        }

        public override void DrawFrame(Rectangle rect, FrameOptions opts = null)
        {
            var frame = FrameOptions.Frames[(int) (opts?.Style ?? FrameStyle.Single)];
            var rectOpts = new RectangleOptions
            {
                Attributes = opts?.Attributes ?? CharAttribute.None,
                Background = opts?.Background ?? CharColor.Black,
                Foreground = opts?.Foreground ?? CharColor.Gray
            };
            DrawRectangle(new Rectangle(rect.X + 1, rect.Y, rect.Width - 2, 1), frame[0], rectOpts);
            DrawRectangle(new Rectangle(rect.X + 1, rect.Y + rect.Height - 1, rect.Width - 2, 1), frame[0], rectOpts);
            DrawRectangle(new Rectangle(rect.X, rect.Y + 1, 1, rect.Height - 2), frame[1], rectOpts);
            DrawRectangle(new Rectangle(rect.X + rect.Width - 1, rect.Y + 1, 1, rect.Height - 2), frame[1], rectOpts);
            PutChar(new Point(rect.X, rect.Y), frame[2], rectOpts.Foreground, rectOpts.Background, rectOpts.Attributes);
            PutChar(new Point(rect.X + rect.Width - 1, rect.Y), frame[3], rectOpts.Foreground,
                rectOpts.Background, rectOpts.Attributes);
            PutChar(new Point(rect.X, rect.Y + rect.Height - 1), frame[4], rectOpts.Foreground,
                rectOpts.Background, rectOpts.Attributes);
            PutChar(new Point(rect.X + rect.Width - 1, rect.Y + rect.Height - 1), frame[5],
                rectOpts.Foreground, rectOpts.Background, rectOpts.Attributes);
        }
    }

    internal static class Helpers
    {
        public static IEnumerable<string> Split(this string s, int len)
        {
            var ofs = 0;
            var slen = s.Length;
            while (slen - ofs >= len)
            {
                yield return s.Substring(ofs, len);
                ofs += len;
            }
            if (slen - ofs > 0) yield return s.Substring(ofs);
        }
    }
}
