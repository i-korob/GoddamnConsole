using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;
using GoddamnConsole.Drawing;
using static GoddamnConsole.NativeProviders.Windows.NativeMethods;

namespace GoddamnConsole.NativeProviders.Windows
{
    /// <summary>
    /// Represents a Windows native console provider
    /// </summary>
    public sealed unsafe class WindowsNativeConsoleProvider : INativeConsoleProvider
    {
        // ReSharper disable once InconsistentNaming
        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
        internal struct KEY_EVENT_RECORD
        {
            // ReSharper disable MemberCanBePrivate.Local
            // ReSharper disable FieldCanBeMadeReadOnly.Local
            [FieldOffset(0)]
            public bool KeyDown; // marshalas(bool) not working
            [FieldOffset(4)]
            public short RepeatCount;
            [FieldOffset(6)]
            public short KeyCode;
            [FieldOffset(8)]
            public short ScanCode;
            [FieldOffset(10)]
            public char Char;
            [FieldOffset(12)]
            public uint ControlKeyState;
            // ReSharper restore MemberCanBePrivate.Local
            // ReSharper restore FieldCanBeMadeReadOnly.Local
        }

        // ReSharper disable once InconsistentNaming
        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT_RECORD
        {
            // ReSharper disable once MemberCanBePrivate.Local
            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            public short EventType;
            public KEY_EVENT_RECORD KeyEvent;
        }

        // ReSharper disable once InconsistentNaming
        [StructLayout(LayoutKind.Sequential)]
        internal struct COORD
        {
            public short X;
            public short Y;
        }

        // ReSharper disable once InconsistentNaming
        [StructLayout(LayoutKind.Sequential)]
        internal struct SMALL_RECT
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        // ReSharper disable once InconsistentNaming
        [StructLayout(LayoutKind.Sequential)]
        internal struct CHAR_INFO
        {
            public char Char;
            public short Attributes;

            public static int SizeOf { get; } = Marshal.SizeOf(typeof(CHAR_INFO));
        }

        // ReSharper disable once InconsistentNaming
        [StructLayout(LayoutKind.Sequential)]
        internal struct CONSOLE_SCREEN_BUFFER_INFO
        {
            // ReSharper disable MemberCanBePrivate.Local
            // ReSharper disable FieldCanBeMadeReadOnly.Local
            public COORD Size;
            public COORD CursorPosition;
            public short Attributes;
            public SMALL_RECT Window;
            public COORD MaxWindowSize;
            // ReSharper restore FieldCanBeMadeReadOnly.Local
            // ReSharper restore MemberCanBePrivate.Local
        }

        // ReSharper disable once InconsistentNaming
        [StructLayout(LayoutKind.Sequential)]
        internal struct CONSOLE_CURSOR_INFO
        {
            // ReSharper disable MemberCanBePrivate.Local
            // ReSharper disable FieldCanBeMadeReadOnly.Local
            public uint Size;
            [MarshalAs(UnmanagedType.Bool)] public bool Visible;
            // ReSharper restore FieldCanBeMadeReadOnly.Local
            // ReSharper restore MemberCanBePrivate.Local
        }

        private readonly CancellationTokenSource _threadToken;
        private readonly ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
        private readonly CHAR_INFO* _buffer;
        private readonly IntPtr _bufferPtr;
        private readonly IntPtr _stdout;
        private readonly IntPtr _stdin;
        private const int BufferSize = 0x200;

        static WindowsNativeConsoleProvider()
        {
            var dynamicMethod = new DynamicMethod("Memset", MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
                null, new[] { typeof(IntPtr), typeof(byte), typeof(int) }, typeof(Console), true);

            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldarg_2);
            generator.Emit(OpCodes.Initblk);
            generator.Emit(OpCodes.Ret);

            Memset =
                (Action<IntPtr, byte, int>)dynamicMethod.CreateDelegate(typeof(Action<IntPtr, byte, int>));
        }

        public WindowsNativeConsoleProvider()
        {
            _threadToken = new CancellationTokenSource();
            _buffer = (CHAR_INFO*) (_bufferPtr = Marshal.AllocHGlobal(BufferSize * BufferSize * CHAR_INFO.SizeOf)).ToPointer();
            _stdin = GetStdHandle(-10);
            _stdout = GetStdHandle(-11);
            {
                var info = new CONSOLE_SCREEN_BUFFER_INFO();
                GetConsoleScreenBufferInfo(_stdout, ref info);
                WindowWidth = info.Window.Right - info.Window.Left + 1;
                WindowHeight = info.Window.Bottom - info.Window.Top + 1;
            }
            new Thread(() => // window size monitor
            {
                while (!_threadToken.IsCancellationRequested)
                {
                    var info = new CONSOLE_SCREEN_BUFFER_INFO();
                    GetConsoleScreenBufferInfo(_stdout, ref info);
                    int nw = info.Window.Right - info.Window.Left + 1,
                        nh = info.Window.Bottom - info.Window.Top + 1;
                    if (nw < 0) nw = WindowWidth;
                    if (nh < 0) nh = WindowHeight; // fukken winapi bugs
                    if (nw == WindowWidth && nh == WindowHeight) continue;
                    var pw = WindowWidth;
                    var ph = WindowHeight;
                    WindowWidth = nw;
                    WindowHeight = nh;
                    try
                    {
                        SizeChanged?.Invoke(this, new SizeChangedEventArgs(new Size(pw, ph), new Size(nw, nh)));
                    }
                    catch
                    {
                        /* Do not care if subscriber fucked up */
                    }
                    Refresh();
                    Thread.Sleep(16);
                }
            }) {Priority = ThreadPriority.Lowest}.Start();
            _keyboardThread = new Thread(() => // keyboard monitor
            {
                INPUT_RECORD* charBuf = stackalloc INPUT_RECORD[1];
                var ptr = new IntPtr(charBuf);
                var sizeOf = Marshal.SizeOf(typeof(INPUT_RECORD));
                while (!_threadToken.IsCancellationRequested)
                {
                    uint nchars = 0;
                    Memset(ptr, 0, sizeOf);
                    ReadConsoleInputW(_stdin, ptr, 1, ref nchars);
                    if (charBuf->EventType == 1 && nchars == 1 && charBuf->KeyEvent.KeyDown)
                    {
                        KeyPressed?.Invoke(
                            this,
                            new KeyPressedEventArgs(new ConsoleKeyInfo(
                                                        charBuf->KeyEvent.Char,
                                                        (ConsoleKey) charBuf->KeyEvent.KeyCode,
                                                        (charBuf->KeyEvent.ControlKeyState & 0x10) > 0,
                                                        (charBuf->KeyEvent.ControlKeyState & 0x03) > 0,
                                                        (charBuf->KeyEvent.ControlKeyState & 0x0c) > 0
                                                        )));
                    }
                }
            });
            _keyboardThread.Start();
        }

        private static readonly Action<IntPtr, byte, int> Memset;
        private readonly Thread _keyboardThread;

        public int WindowWidth { get; private set; }

        public int WindowHeight { get; private set; }

        public bool CursorVisible
        {
            get
            {
                var info = new CONSOLE_CURSOR_INFO();
                GetConsoleCursorInfo(_stdout, ref info);
                return info.Visible;
            }
            set
            {
                var cci = new CONSOLE_CURSOR_INFO
                {
                    Visible = value,
                    Size = 1
                };
                SetConsoleCursorInfo(_stdout, ref cci);
            }
        }

        public int CursorX
        {
            get
            {
                var info = new CONSOLE_SCREEN_BUFFER_INFO();
                GetConsoleScreenBufferInfo(_stdout, ref info);
                return info.CursorPosition.X;
            }
            set
            {
                CursorVisible = true;
                SetConsoleCursorPosition(_stdout, new COORD
                {
                    X = (short)value,
                    Y = (short)CursorY
                });
            }
        }

        public int CursorY
        {
            get
            {
                var cinfo = new CONSOLE_SCREEN_BUFFER_INFO();
                GetConsoleScreenBufferInfo(_stdout, ref cinfo);
                var info = new CONSOLE_SCREEN_BUFFER_INFO();
                GetConsoleScreenBufferInfo(_stdout, ref info);
                return info.CursorPosition.Y - cinfo.Window.Top;
            }
            set
            {
                CursorVisible = true;
                var cinfo = new CONSOLE_SCREEN_BUFFER_INFO();
                GetConsoleScreenBufferInfo(_stdout, ref cinfo);
                SetConsoleCursorPosition(_stdout, new COORD
                {
                    X = (short) CursorX,
                    Y = (short) (value + cinfo.Window.Top)
                });
            }
        }

        public void PutChar(Character chr, int x, int y)
        {
            if (x >= BufferSize || y >= BufferSize || x < 0 || y < 0) return;
            _buffer[y* BufferSize + x].Attributes =
                (short) ((short) chr.Attribute | (short) chr.Foreground | ((short) chr.Background << 4));
            _buffer[y* BufferSize + x].Char = chr.Char;
        }

        public void CopyBlock(IEnumerable<Character> chars, int x, int y, int width, int height)
        {
            var enumerator = chars.GetEnumerator();
            for (var i = x; i < x + width; i++)
                for (var j = y; j < y + height; j++)
                {
                    if (enumerator.MoveNext())
                    {
                        var chr = enumerator.Current;
                        _buffer[j* BufferSize + i].Attributes =
                            (short) ((short) chr.Attribute | (short) chr.Foreground | ((short) chr.Background << 4));
                        _buffer[j* BufferSize + i].Char = chr.Char;
                    }
                    else return;
                }
        }

        public void FillBlock(Character chr, int x, int y, int width, int height)
        {
            for (var i = x; i < x + width; i++)
                for (var j = y; j < y + height; j++)
                {
                    _buffer[j* BufferSize + i].Attributes =
                        (short) ((short) chr.Attribute | (short) chr.Foreground | ((short) chr.Background << 4));
                    _buffer[j* BufferSize + i].Char = chr.Char;
                }
        }

        public void Refresh()
        {
            var info = new CONSOLE_SCREEN_BUFFER_INFO();
            GetConsoleScreenBufferInfo(_stdout, ref info);
            var rect = new SMALL_RECT
            {
                Left = info.Window.Left,
                Top = info.Window.Top,
                Right = (short) (info.Window.Left + BufferSize - 1), 
                Bottom = (short) (info.Window.Top + BufferSize - 1),   
            };
            WriteConsoleOutputW(_stdout, _bufferPtr,
                new COORD { X = BufferSize, Y = BufferSize },
                new COORD(), ref rect);
        }

        public void Clear(CharColor background)
        {
            for (var i = 0; i < BufferSize * BufferSize; i++)
                _buffer[i].Attributes = (byte) ((int) background + ((int) background << 4));
        }

        public void Start()
        {
            _shutdownEvent.WaitOne();
        }

        public event EventHandler<SizeChangedEventArgs> SizeChanged;
        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        [SuppressMessage("Microsoft.Usage", "CA2216:DisposableTypesShouldDeclareFinalizer")]
        public void Dispose()
        {
            _shutdownEvent.Set();
            _shutdownEvent.Dispose();
            _threadToken.Cancel();
            _threadToken.Dispose();
            _keyboardThread.Abort();
        }
    }
}
