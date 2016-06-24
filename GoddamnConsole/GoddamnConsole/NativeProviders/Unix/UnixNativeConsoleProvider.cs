using System;
using System.Collections.Generic;
using System.Text;
using Syscon = System.Console; // todo use ioctl

namespace GoddamnConsole.NativeProviders.Unix
{
    public class UnixNativeConsoleProvider : INativeConsoleProvider
    {
        private const int BufferSize = 0x100;
        private Character[] _buffer = new Character[BufferSize * BufferSize];

        public int WindowWidth { get; } = Syscon.WindowWidth;
        public int WindowHeight { get; } = Syscon.WindowHeight;
        public bool CursorVisible { get; set; }
        public int CursorX { get; set; }
        public int CursorY { get; set; }
        public void PutChar(Character chr, int x, int y)
        {
            if (x >= BufferSize || y >= BufferSize || x < 0 || y < 0) return;
            _buffer[y * BufferSize + x] = chr;
        }

        public void CopyBlock(IEnumerable<Character> chars, int x, int y, int width, int height)
        {
            var enumerator = chars.GetEnumerator();
            for (var i = x; i < x + width; i++)
                for (var j = y; j < y + height; j++)
                {
                    if (enumerator.MoveNext())
                    {
                        _buffer[j * BufferSize + i] = enumerator.Current;
                    }
                    else return;
                }
        }

        public void FillBlock(Character chr, int x, int y, int width, int height)
        {
            for (var i = x; i < x + width; i++)
                for (var j = y; j < y + height; j++)
                {
                    _buffer[j * BufferSize + i] = chr;
                }
        }

        public void Refresh()
        {
            var str = new StringBuilder("\x1b[2J\x1b[H");
            for (var i = 0; i < WindowHeight; i++)
                for (var j = 0; j < WindowWidth; j++)
                    str.Append(_buffer[i * WindowWidth + j].Char);
            Syscon.WriteLine(str.ToString());
        }

        public void Start()
        {
        }

        public void Clear(CharColor background)
        {
            for (var i = 0; i < BufferSize * BufferSize; i++)
                _buffer[i] = new Character(' ', background, background, CharAttribute.None);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public event EventHandler<SizeChangedEventArgs> SizeChanged;
        public event EventHandler<KeyPressedEventArgs> KeyPressed;
    }
}
