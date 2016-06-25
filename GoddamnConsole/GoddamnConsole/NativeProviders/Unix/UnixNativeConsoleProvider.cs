using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using GoddamnConsole.Drawing;
using Syscon = System.Console; // todo use ioctl
using static GoddamnConsole.NativeProviders.Unix.NativeMethods;

namespace GoddamnConsole.NativeProviders.Unix
{
    /// <summary>
    /// Represents a Unix native console provider (based on ncurses)
    /// </summary>
    public sealed unsafe class UnixNativeConsoleProvider : INativeConsoleProvider
    {
        public UnixNativeConsoleProvider()
        {
            initscr();
            start_color();
            for (short i = 0; i < 64; i++)
            {
                var fg = i >> 3;
                var bg = i & 7;
                fg = ((fg & 0x4) >> 2) | (fg & 0x2) | ((fg & 0x1) << 2);
                bg = ((bg & 0x4) >> 2) | (bg & 0x2) | ((bg & 0x1) << 2);
                init_pair((short)(i + 1), (short)fg, (short)bg);
            }
            new Thread(() => // window size monitor
            {
                var oldw = Syscon.WindowWidth;
                var oldh = Syscon.WindowHeight;
                while (!_cts.IsCancellationRequested)
                {
                    var neww = Syscon.WindowWidth;
                    var newh = Syscon.WindowHeight;
                    if (neww != oldw || newh != oldh)
                    {
                        WindowWidth = neww;
                        WindowHeight = newh;
                        resizeterm(newh, neww);
                        try
                        {
                            SizeChanged?.Invoke(this,
                                                new SizeChangedEventArgs(new Size(oldw, oldh), new Size(neww, newh)));
                        }
                        catch
                        {
                            //
                        }
                        oldw = neww;
                        oldh = newh;
                    }
                }
            }).Start();
            new Thread(() => // keyboard monitor
            {
                while (!_cts.IsCancellationRequested)
                {
                    var chr = Syscon.ReadKey(true);
                    KeyPressed?.Invoke(this, new KeyPressedEventArgs(chr));
                }
            }).Start();
        }

        private const int BufferSize = 0x100;
        private readonly Character[] _buffer = new Character[BufferSize * BufferSize];

        public int WindowWidth { get; private set; } = Syscon.WindowWidth;
        public int WindowHeight { get; private set; } = Syscon.WindowHeight;
        public bool CursorVisible { get; set; }
        public int CursorX { get; set; }
        public int CursorY { get; set; }
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

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
            move(0, 0);
            for (var i = 0; i < WindowHeight; i++)
                for (var j = 0; j < WindowWidth; j++)
                {
                    var chr = _buffer[i * BufferSize + j];
                    var fg = (int) chr.Foreground;
                    var bg = (int) chr.Background;
                    var bold = (fg & 0x8) > 0;
                    fg = fg & 0x7;
                    bg = bg == 8 ? 7 : (bg & 0x7);
                    var cchar = new cchar_t();
                    cchar.attr = ((1 + bg + (fg << 3)) << 8) + (bold ? 2097152 : 0);
                    cchar.chars[0] = chr.Char;
                    add_wch(&cchar);
                }
            refresh();
        }

        public void Start()
        {

        }

        public void Clear(CharColor background)
        {
            for (var i = 0; i < BufferSize * BufferSize; i++)
                _buffer[i] = new Character(' ', background, background, CharAttribute.None);
        }

        [SuppressMessage("Microsoft.Usage", "CA2216:DisposableTypesShouldDeclareFinalizer")]
        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            endwin();
        }

        public event EventHandler<SizeChangedEventArgs> SizeChanged;
        public event EventHandler<KeyPressedEventArgs> KeyPressed;
    }
}
