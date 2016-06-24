using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    /// <summary>
    /// Draws a border around another element
    /// </summary>
    public class Border : ContentControl
    {
        private FrameStyle _frameStyle = FrameStyle.Single;
        
        /// <summary>
        /// Gets or sets the border frame style
        /// </summary>
        public FrameStyle FrameStyle
        {
            get { return _frameStyle; }
            set { _frameStyle = value;
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
                    Foreground = Foreground,
                    Background = Background
                });
        }

        public override Rectangle MeasureBoundingBox(Control child)
        {
            return new Rectangle(1, 1, ActualWidth - 2, ActualHeight - 2);
        }
    }
}
