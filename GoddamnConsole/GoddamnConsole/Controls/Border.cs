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

        protected override void OnRender(DrawingContext context)
        {
            context.Clear(Background);
            context.DrawFrame(new Rectangle(0, 0, ActualWidth, ActualHeight),
                new FrameOptions
                {
                    Style = FrameStyle,
                    Foreground = FrameColor,
                    Background = Background
                });
            //if (ActualWidth >= 2 || ActualHeight >= 2)
            //    Content.Render(context.Shrink(new Rectangle(1, 1, ActualWidth - 2, ActualHeight - 2)));
        }

        public override Rectangle MeasureBoundingBox(Control child)
        {
            return new Rectangle(1, 1, ActualWidth - 2, ActualHeight - 2);
        }
    }
}
