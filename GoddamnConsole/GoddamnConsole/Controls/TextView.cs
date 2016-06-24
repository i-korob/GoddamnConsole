using System.Linq;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    /// <summary>
    /// Represents a read-only text area
    /// </summary>
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

        /// <summary>
        /// Gets or sets the text of TextView
        /// </summary>
        [AlsoNotifyFor(nameof(MaxWidth))]
        [AlsoNotifyFor(nameof(MaxHeight))]
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets a text wrapping option
        /// </summary>
        [AlsoNotifyFor(nameof(MaxWidth))]
        [AlsoNotifyFor(nameof(MaxHeight))]
        public TextWrapping TextWrapping
        {
            get { return _textWrapping; }
            set
            {
                _textWrapping = value;
                OnPropertyChanged();
            }
        }

        protected override void OnRender(DrawingContext context)
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
