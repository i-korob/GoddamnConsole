using System;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class Border : ContentControl
    {
        public override void Render(DrawingContext context)
        {
            context.DrawFrame(new Rectangle(0, 0, ActualWidth, ActualHeight));
            Content.Render(context.Shrink(new Rectangle(1, 1, ActualWidth - 2, ActualHeight - 2)));
        }

        public override Size MeasureChild(Control child)
        {
            return new Size(Math.Min(child.AssumedWidth, ActualWidth - 2),
                Math.Min(child.AssumedHeight, ActualHeight - 2));
        }
    }
}
