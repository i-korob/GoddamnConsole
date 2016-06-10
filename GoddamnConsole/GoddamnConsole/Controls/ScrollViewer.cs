using System;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class ScrollViewer : Control, IContentControl
    {
        private int _scrollX;
        private int _scrollY;

        protected override void OnKeyPress(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.UpArrow && _scrollY < 0) _scrollY++;
            if (key.Key == ConsoleKey.DownArrow && 
                MeasureChild(Content).Height - ActualHeight >= -_scrollY) _scrollY--;
            if (key.Key == ConsoleKey.LeftArrow && _scrollX < 0)
            {
                _scrollX++;
            }
            if (key.Key == ConsoleKey.RightArrow)
            {
                _scrollX--;
            }
            Invalidate();
        }

        public override void Render(DrawingContext context)
        {
            var actHeight = ActualHeight;
            var actWidth = ActualWidth;
            if (_scrollY > 0) _scrollY = 0;
            else
            {
                var textHeight = MeasureChild(Content).Height;
                if (textHeight < ActualHeight) _scrollY = 0;
                else if (textHeight - ActualHeight < -_scrollY)
                    _scrollY = -(textHeight - actHeight);
            }
            if (_scrollX > 0) _scrollX = 0;
            else
            {
                var textWidth = MeasureChild(Content).Width;
                if (textWidth < ActualWidth) _scrollX = 0;
                else if (textWidth - ActualWidth < -_scrollX)
                    _scrollX = -(textWidth - actWidth);
            }
            var scrolled = context.Scroll(new Point(_scrollX, _scrollY));
            Content.Render(scrolled);
        }

        public Size MeasureChild(Control child)
        {
            return new Size(Math.Max(0, child.Width), Math.Max(0, child.Height));
        }

        public Control Content { get; set; }
        public event EventHandler<Control> ContentDetached;
    }
}
