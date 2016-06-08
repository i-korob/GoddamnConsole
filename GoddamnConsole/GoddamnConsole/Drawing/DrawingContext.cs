using System.Collections.Generic;
using System.Linq;

namespace GoddamnConsole.Drawing
{
    public abstract class DrawingContext
    {
        public abstract DrawingContext Scroll(Point sourceOffset);
        public abstract DrawingContext Shrink(Point sourceOffset, Rectangle targetArea);
        public abstract DrawingContext Shrink(Rectangle targetArea);

        public abstract void Clear();
        public abstract void PutChar(Point pt, char chr, CharColor foreground, CharColor background,
            CharAttribute attribute);
        public abstract void DrawRectangle(Rectangle rect, char fill, RectangleOptions opts = null);
        public abstract void DrawText(Point point, string line, TextOptions opts = null);
        public abstract void DrawText(Rectangle rect, string text, TextOptions opts = null);
        public abstract void DrawFrame(Rectangle rect, FrameOptions opts = null);

        public static Size MeasureText(string text)
        {
            if (text == null) return new Size(0, 0);
            var lines = text.Replace("\r\n", "\n").Split('\n');
            return new Size(lines.Max(x => x.Length), lines.Length);
        }

        public static Size MeasureWrappedText(string text, int maxWidth)
        {
            if (text == null) return new Size(0, 0);
            var lines =
                text.Replace("\r\n", "\n").Split('\n').SelectMany(x => x.Split(maxWidth)).ToArray();
            return new Size(lines.Max(x => x.Length), lines.Length);
        }
    }
}
