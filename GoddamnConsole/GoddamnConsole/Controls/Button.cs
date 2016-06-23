using System;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class Button : Control
    {
        public Button()
        {
            Focusable = true;
        }

        private string _text;

        public string Text
        {
            get { return _text; }
            set { _text = value; OnPropertyChanged(); }
        }

        public event EventHandler Clicked;

        protected override void OnKeyPressed(ConsoleKeyInfo info)
        {
            if (info.Key == ConsoleKey.Enter) Clicked?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawFrame(new Rectangle(0, 0, ActualWidth, ActualHeight), new FrameOptions
            {
                Style = Console.Focused == this ? FrameStyle.Double : FrameStyle.Single
            });
            var text = ActualWidth > Text.Length
                           ? Text
                           : ActualWidth > 4
                                 ? Text.Remove(ActualWidth - 3) + "..."
                                 : string.Empty;
            dc.DrawText(new Point((ActualWidth - text.Length) / 2, ActualHeight / 2), text);
        }
    }
}
