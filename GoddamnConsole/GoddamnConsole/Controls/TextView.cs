using System;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class TextView : Control
    {
        public string Text { get; set; }
        public TextWrapping TextWrapping { get; set; } = TextWrapping.Wrap;

        public override void Render(DrawingContext context)
        {
            context.Clear(Background);
            context.DrawText(
                new Rectangle(0, 0, ActualWidth, ActualHeight), 
                Text,
                new TextOptions
                {
                    TextWrapping = TextWrapping,
                    Foreground = Foreground,
                    Background = Background
                });
        }
    }
}
