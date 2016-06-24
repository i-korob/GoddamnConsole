using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Syscon = System.Console; // todo use ioctl

namespace GoddamnConsole.NativeProviders.Unix
{
    public class UnixNativeConsoleProvider : INativeConsoleProvider
    {
        [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int puts([MarshalAs(UnmanagedType.LPStr)] string str);

        public UnixNativeConsoleProvider()
        {
            new Thread(() => // keyboard monitor
            {
                while (true)
                {
                    var chr = Syscon.ReadKey(true);
                    KeyPressed?.Invoke(this, new KeyPressedEventArgs(chr));
                }
            }).Start();
        }

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
                {
                    var chr = _buffer[i * BufferSize + j];
                    var fg = (int)chr.Foreground;
                    var bg = (int)chr.Background;
                    str.Append("\x1b[3");
                    str.Append(((fg & 0x4) >> 2) | (fg & 0x2) | ((fg & 0x1) << 2));
                    if ((fg & 0x8) > 0) str.Append(";1");
                    str.Append(";4");
                    str.Append(((bg & 0x4) >> 2) | (bg & 0x2) | ((bg & 0x1) << 2));
                    if ((bg & 0x8) > 0) str.Append(";1");
                    str.Append("m");
                    str.Append(_buffer[i * BufferSize + j].Char);
                }
            str.Append("\x1b[H");
            puts(str.ToString());
            //Syscon.WriteLine(str.ToString());
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

        }

        public event EventHandler<SizeChangedEventArgs> SizeChanged;
        public event EventHandler<KeyPressedEventArgs> KeyPressed;
    }
}
