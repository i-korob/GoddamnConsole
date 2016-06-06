namespace GoddamnConsole.Drawing
{
    internal sealed class ImaginaryDrawingContext : DrawingContext
    {
        //public override int Width { get; } = 0;
        //public override int Height { get; } = 0;

        public override DrawingContext Shrink(Point sourceOffset, Rectangle targetArea)
        {
            return this;
        }

        public override void Clear()
        {

        }

        public override void DrawRectangle(Rectangle rect, char fill, RectangleOptions opts = null)
        {

        }

        public override void DrawText(Point point, string line, TextOptions opts = null)
        {

        }

        public override void DrawText(Rectangle rect, string text, TextOptions opts = null)
        {

        }

        public override void DrawFrame(Rectangle rect, FrameOptions opts = null)
        {

        }
    }
}
