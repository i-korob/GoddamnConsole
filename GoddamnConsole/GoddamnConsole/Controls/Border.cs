using System;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class Border : ContentControl
    {
        private FrameStyle _frameStyle = FrameStyle.Single;
        private CharColor _frameColor = CharColor.White;

        public FrameStyle FrameStyle
        {
            get { return _frameStyle; }
            set { _frameStyle = value;
                OnPropertyChanged();
            }
        }

        public CharColor FrameColor
        {
            get { return _frameColor; }
            set { _frameColor = value;
                OnPropertyChanged();
            }
        }

        public override void Render(DrawingContext context)
        {
            var aw = ActualWidth;
            var ah = ActualHeight;
            context.Clear(Background);
            context.DrawFrame(new Rectangle(0, 0, aw, ah),
                new FrameOptions
                {
                    Style = FrameStyle,
                    Foreground = FrameColor,
                    Background = Background
                });
            if (aw >= 2 || ah >= 2)
                Content.Render(context.Shrink(new Rectangle(1, 1, aw - 2, ah - 2)));
        }

        public override Size MeasureChild(Control child)
        {
            return new Size(Math.Min(child.AssumedWidth, ActualWidth - 2),
                Math.Min(child.AssumedHeight, ActualHeight - 2));
        }
    }
}
