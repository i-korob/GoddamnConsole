using System;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class TextBox : Control
    {
        public TextBox()
        {
            Focusable = true;
        }

        private int _scrollX;
        private int _scrollY;
        public string Text { get; set; }
        public TextWrapping TextWrapping { get; set; } = TextWrapping.Wrap;

        private int MeasureHeight() =>
            TextWrapping == TextWrapping.Wrap
                ? DrawingContext.MeasureWrappedText(Text, ActualWidth).Height
                : DrawingContext.MeasureText(Text).Height;

        private int MeasureWidth() =>
            TextWrapping == TextWrapping.Wrap
                ? DrawingContext.MeasureWrappedText(Text, ActualWidth).Width
                : DrawingContext.MeasureText(Text).Width;

        protected override void OnKeyPress(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.UpArrow && _scrollY < 0) _scrollY++;
            if (key.Key == ConsoleKey.DownArrow && 
                MeasureHeight() - ActualHeight >= -_scrollY) _scrollY--;
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
                var textHeight = MeasureHeight();
                if (textHeight < ActualHeight) _scrollY = 0;
                else if (textHeight - ActualHeight < -_scrollY)
                    _scrollY = -(textHeight - actHeight);
            }
            if (_scrollX > 0) _scrollX = 0;
            else
            {
                var textWidth = MeasureWidth();
                if (textWidth < ActualWidth) _scrollX = 0;
                else if (textWidth - ActualWidth < -_scrollX)
                    _scrollX = -(textWidth - actWidth);
            }
            var scrolled = context.Scroll(new Point(_scrollX, _scrollY));
            scrolled.DrawText(
                new Rectangle(0, 0, actWidth, actHeight), 
                Text,
                new TextOptions {TextWrapping = TextWrapping});
        }
    }
}
