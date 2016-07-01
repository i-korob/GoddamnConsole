using System;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    /// <summary>
    /// Represents a standart button control, which reacts to the Button.Clicked event by pressing Enter
    /// </summary>
    public class Button : Control
    {
        public Button()
        {
            Focusable = true;
        }

        private string _text;

        public override int MinHeight => 3;

        /// <summary>
        /// Gets or sets the title of button
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { _text = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Occurs when a button is clicked
        /// </summary>
        public event EventHandler Clicked;

        protected override void OnKeyPressed(ConsoleKeyInfo info)
        {
            if (info.Key == ConsoleKey.Enter) Clicked?.Invoke(this, EventArgs.Empty);
        }

        public override int MaxHeight => 3;

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawFrame(new Rectangle(0, 0, ActualWidth, ActualHeight), new FrameOptions
            {
                Style = Console.Focused == this ? FrameStyle.Double : FrameStyle.Single,
                Background = Background,
                Foreground = Foreground
            });
            var text = ActualWidth > Text.Length
                           ? Text
                           : ActualWidth > 4
                                 ? Text.Remove(ActualWidth - 3) + "..."
                                 : string.Empty;
            dc.DrawText(new Point((ActualWidth - text.Length) / 2, ActualHeight / 2), text, new TextOptions
            {
                Background = Background,
                Foreground = Foreground
            });
        }
    }
}
