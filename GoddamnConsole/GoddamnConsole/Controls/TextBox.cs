using System;
using System.Collections.Generic;
using System.Linq;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    /// <summary>
    /// Represents a editable text area
    /// </summary>
    public class TextBox : Control
    {
        private class TextMeasurement
        {
            private readonly int _maxWidth;

            public TextMeasurement(string str, int maxWidth = int.MaxValue)
            {
                _maxWidth = maxWidth;
                Lines =
                    str.Replace("\r\n", "\n").Split('\n').Select(x => x.Split(maxWidth).ToArray()).ToArray();
                LineWidths = Lines.SelectMany(x => x.Select(y => y.Length)).ToArray();
            }
            
            private IReadOnlyList<IReadOnlyList<string>> Lines { get; } 
            private IReadOnlyList<int> LineWidths { get; }

            public Point CaretPosition(int pos)
            {
                int totalOfs = 0, yofs = 0, cx = 0, cy = 0, lofs = 0, lyofs = 0;
                foreach (var wid in LineWidths)
                {
                    var eol = Lines[lofs].Count == lyofs + 1;
                    if (pos >= totalOfs && pos <= totalOfs + wid)
                    {
                        cx = pos - totalOfs;
                        cy = yofs;
                        if (cx >= wid + (eol ? 1 : 0))
                        {
                            cx -= wid;
                            cy++;
                        }
                        break;
                    }
                    totalOfs += wid + (eol ? 1 : 0);
                    yofs++;
                    lyofs++;
                    if (lyofs == Lines[lofs].Count)
                    {
                        lofs++;
                        lyofs = 0;
                    }
                }
                return new Point(cx, cy);
            }

            public int MoveCaretUp(int pos)
            {
                var prevOffs = 0;
                var prevEol = false;
                for (int wln = 0, offs = 0, ln = 0, rln = 0; wln < LineWidths.Count; offs += LineWidths[wln], wln++, rln++)
                {
                    var eol = Lines[ln].Count == rln + 1;
                    if (pos >= offs && pos < offs + LineWidths[wln] + 1)
                    {
                        if (wln == 0) return pos;
                        var diff = pos - offs;
                        return prevOffs + (prevEol ? -1 : 0) +
                               Math.Min(
                                   LineWidths[wln - 1] +
                                   (prevEol && !eol && diff % _maxWidth == 0 ? 1 : 0),
                                   diff);
                    }
                    if (eol)
                    {
                        offs++;
                        rln = -1;
                        ln++;
                    }
                    prevEol = eol;
                    prevOffs = offs;
                }
                return pos;      
            }

            public int MoveCaretDown(int pos)
            {
                for (int wln = 0, offs = 0, ln = 0, rln = 0; wln < LineWidths.Count - 1; offs += LineWidths[wln], wln++, rln++)
                {
                    var eol = Lines[ln].Count == rln + 1;
                    if (pos >= offs && pos < offs + LineWidths[wln] + 1)
                    {
                        if (eol)
                        {
                            return offs + LineWidths[wln] + 1 + Math.Min(LineWidths[wln + 1], pos - offs);
                        }
                        var diff = pos - offs;
                        diff = Math.Min(diff, LineWidths[wln + 1] + (pos - offs == _maxWidth ? 1 : 0));
                        return offs + diff + LineWidths[wln];
                    }
                    if (eol)
                    {
                        offs++;
                        rln = -1;
                        ln++;
                    }
                }
                return pos;
            }
        }

        public TextBox()
        {
            Focusable = true;
        }

        /// <summary>
        /// Gets or sets the text of TextBox
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { _text = value;
                if (value != null)
                    _measurement = new TextMeasurement
                        (value,
                         _textWrapping == TextWrapping.Wrap ? ActualWidth : int.MaxValue);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a text wrapping option
        /// </summary>
        public TextWrapping TextWrapping
        {
            get { return _textWrapping; }
            set { _textWrapping = value;
                if (_text != null)
                    _measurement = new TextMeasurement
                        (_text,
                         value == TextWrapping.Wrap ? ActualWidth : int.MaxValue);
                OnPropertyChanged();
            }
        }

        private TextMeasurement _measurement;
        private int _caretPos;
        private long _scrollX;
        private long _scrollY;
        private string _text;
        private TextWrapping _textWrapping = TextWrapping.NoWrap;

        private void Remeasure()
        {
            if (_text != null)
                _measurement = new TextMeasurement
                    (_text,
                     _textWrapping == TextWrapping.Wrap ? ActualWidth : int.MaxValue);
        }

        protected override void OnSizeChanged()
        {
            Remeasure();
        }

        protected override void OnKeyPressed(ConsoleKeyInfo key)
        {
            if (key.Modifiers.HasFlag(ConsoleModifiers.Control) || key.Modifiers.HasFlag(ConsoleModifiers.Alt))
                return;
            if (_caretPos > Text.Length) _caretPos = Text.Length;
            if (_caretPos < 0) _caretPos = 0;
            var prevPos = _caretPos;
            switch (key.Key)
            {
                case ConsoleKey.Backspace:
                    if (_caretPos > 0)
                    {
                        _caretPos--;
                        Text = Text.Remove(_caretPos, 1);
                    }
                    break;
                case ConsoleKey.LeftArrow:
                    if (_caretPos > 0) _caretPos--;
                    break;
                case ConsoleKey.RightArrow:
                    if (_caretPos < Text.Length) _caretPos++;
                    break;
                case ConsoleKey.DownArrow:
                    _caretPos = _measurement.MoveCaretDown(_caretPos);
                    break;
                case ConsoleKey.UpArrow:
                    _caretPos = _measurement.MoveCaretUp(_caretPos);
                    break;
                case ConsoleKey.Delete:
                    if (_caretPos < Text.Length)
                    {
                        Text = Text.Remove(_caretPos, 1);
                        if (_caretPos > Text.Length) _caretPos = Text.Length;
                    }
                    break;
                case ConsoleKey.Enter:
                    Text = Text.Insert(_caretPos++, "\n");
                    break;
                default:
                    if (!char.IsControl(key.KeyChar))
                        Text = Text.Insert(_caretPos++, key.KeyChar.ToString());
                    break;
            }
            if (_caretPos != prevPos)
            {
                var point = _measurement.CaretPosition(_caretPos);
                var prevPoint = _measurement.CaretPosition(prevPos);
                var indent = Math.Min(2, Math.Max(0, prevPoint.X - point.X - ActualWidth + 3));
                if (ActualWidth - point.X > 0) indent = 0;
                if (point.X + _scrollX < 0) _scrollX = -point.X;
                if (point.X + _scrollX >= ActualWidth) _scrollX = ActualWidth - point.X - 1;
                _scrollX += indent;
                if (point.Y + _scrollY < 0) _scrollY = -point.Y;
                if (point.Y + _scrollY >= ActualHeight) _scrollY = ActualHeight - point.Y - 1;
            }
            Invalidate();
        }

        protected override void OnRender(DrawingContext context)
        {
            var so = new Point(TextWrapping == TextWrapping.Wrap ? 0 : (int) _scrollX, (int) _scrollY);
            var cpos =
                _measurement.CaretPosition(_caretPos);
            if (cpos.X >= ActualWidth)
            {
                cpos = new Point(0, cpos.Y + 1);
                if (cpos.Y + so.Y >= ActualHeight) so = so.Offset(0, -1);
            }
            CursorPosition = cpos.Offset(so.X, so.Y)
                                 .Offset(context.RenderOffsetX, context.RenderOffsetY);
            context = context.Scroll(so);
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
