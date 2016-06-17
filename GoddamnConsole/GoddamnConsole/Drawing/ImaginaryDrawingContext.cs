namespace GoddamnConsole.Drawing
{
    internal sealed class ImaginaryDrawingContext : DrawingContext
    {
        public override int RenderOffsetX => 0;
        public override int RenderOffsetY => 0;

        public override DrawingContext Scroll(Point sourceOffset)
        {
            return this;
        }

        public override DrawingContext Shrink(Point sourceOffset, Rectangle targetArea)
        {
            return this;
        }

        public override DrawingContext Shrink(Rectangle targetArea)
        {
            return this;
        }

        public override void Clear(CharColor background)
        {

        }

        public override void PutChar(Point pt, char chr, CharColor foreground, CharColor background, CharAttribute attribute)
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
