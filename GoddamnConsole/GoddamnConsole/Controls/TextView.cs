using System.Linq;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class TextView : Control
    {
        private TextWrapping _textWrapping = TextWrapping.Wrap;
        private string _text;

        public override int MaxWidth
            => DrawingContext.MeasureText(_text ?? "").Max();

        public override int MaxHeight
            => _textWrapping == TextWrapping.Wrap
                   ? DrawingContext.MeasureWrappedText(_text ?? "", ActualWidth).Height
                   : DrawingContext.MeasureText(_text ?? "").Count();

        public string Text
        {
            get { return _text; }
            set { _text = value;
                OnPropertyChanged();
            }
        }

        public TextWrapping TextWrapping
        {
            get { return _textWrapping; }
            set { _textWrapping = value;
                OnPropertyChanged();
            }
        }

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
