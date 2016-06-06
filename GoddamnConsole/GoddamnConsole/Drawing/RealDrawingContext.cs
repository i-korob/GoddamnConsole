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

        private readonly int _scrollX; // TODO
        private readonly int _scrollY; // TODO
        private readonly int _x;
        private readonly int _y;
        private readonly int _width;
        private readonly int _height;


        public override DrawingContext Shrink(Point sourceOffset, Rectangle targetArea)
        {
            throw new System.NotImplementedException();
        }

        public override void Clear()
        {
            throw new System.NotImplementedException();
        }

        public override void DrawRectangle(Rectangle rect, char fill, RectangleOptions opts = null)
        {
            var clippedRect = rect.Clip(0, 0, _width, _height);
            if (clippedRect.Width == 0 || clippedRect.Height == 0) return;
            for (var x = clippedRect.X; x < clippedRect.X + clippedRect.Width; x++)
                for (var y = clippedRect.Y; y < clippedRect.Y + clippedRect.Height; y++)
                {
                    Console.Provider.PutChar(new Character(fill), x + _x, y + _y);
                }
        }

        public override void DrawText(Point point, string line, TextOptions opts = null)
        {
            throw new System.NotImplementedException();
        }

        public override void DrawText(Rectangle rect, string text, TextOptions opts = null)
        {
            throw new System.NotImplementedException();
        }

        public override void DrawFrame(Rectangle rect, FrameOptions opts = null)
        {
            throw new System.NotImplementedException();
        }
    }
}
