using System;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class ScrollViewer : ContentControl
    {
        public ScrollViewer()
        {
            Focusable = true;
        }

        private int _scrollX;
        private int _scrollY;

        protected override void OnKeyPressed(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.UpArrow && _scrollY < 0) _scrollY++;
            if (key.Key == ConsoleKey.DownArrow && 
                Content.ActualHeight - ActualHeight >= -_scrollY) _scrollY--;
            if (key.Key == ConsoleKey.LeftArrow && _scrollX < 0)
            {
                _scrollX++;
            }
            if (key.Key == ConsoleKey.RightArrow &&
                Content.ActualWidth - ActualWidth >= -_scrollX)
            {
                _scrollX--;
            }
            Invalidate();
        }

        protected override void OnRender(DrawingContext context)
        {
            if (_scrollY > 0) _scrollY = 0;
            else
            {
                var contentHeight = Content.ActualHeight;
                if (contentHeight < ActualHeight) _scrollY = 0;
                else if (contentHeight - ActualHeight < -_scrollY)
                    _scrollY = -(contentHeight - ActualHeight);
            }
            if (_scrollX > 0) _scrollX = 0;
            else
            {
                var contentWidth = Content.ActualWidth;
                if (contentWidth < ActualWidth) _scrollX = 0;
                else if (contentWidth - ActualWidth < -_scrollX)
                    _scrollX = -(contentWidth - ActualWidth);
            }
            //var scrolled = context.Scroll(new Point(_scrollX, _scrollY));
            //Content.Render(scrolled);
        }

        public override Point GetScrollOffset(Control child) => new Point(_scrollX, _scrollY);
    }
}
