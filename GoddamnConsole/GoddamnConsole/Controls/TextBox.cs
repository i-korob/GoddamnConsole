using System;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class TextBox : Control
    {
        public string Text { get; set; }

        protected override void OnKeyPress(ConsoleKeyInfo key)
        {
            base.OnKeyPress(key);
        }

        protected override void OnRender(DrawingContext context)
        {
            context.DrawText(new Rectangle(0, 0, 20, 10), Text, new TextOptions {TextWrapping = TextWrapping.Wrap});
        }
    }
}
