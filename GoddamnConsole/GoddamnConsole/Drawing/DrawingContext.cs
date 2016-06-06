namespace GoddamnConsole.Drawing
{
    public abstract class DrawingContext
    {
        //public abstract int Width { get; }
        //public abstract int Height { get; }

        public abstract DrawingContext Shrink(Point sourceOffset, Rectangle targetArea);

        public abstract void Clear();
        public abstract void DrawRectangle(Rectangle rect, char fill, RectangleOptions opts = null);
        public abstract void DrawText(Point point, string line, TextOptions opts = null);
        public abstract void DrawText(Rectangle rect, string text, TextOptions opts = null);
        public abstract void DrawFrame(Rectangle rect, FrameOptions opts = null);
    }
}
