using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;
using Syscon = System.Console;

namespace GoddamnConsole.NativeProviders
{
    public sealed unsafe class WindowsNativeConsoleProvider : INativeConsoleProvider
    {
        // ReSharper disable once InconsistentNaming
        [StructLayout(LayoutKind.Sequential)]
        private struct COORD
        {
            public short X;
            public short Y;
        }

        // ReSharper disable once InconsistentNaming
        [StructLayout(LayoutKind.Sequential)]
        private struct SMALL_RECT
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        // ReSharper disable once InconsistentNaming
        [StructLayout(LayoutKind.Sequential)]
        private struct CHAR_INFO
        {
            public char Char;
            public short Attributes;

            public static int SizeOf { get; } = Marshal.SizeOf<CHAR_INFO>();
        }

        private readonly CancellationTokenSource _threadToken;
        private readonly ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
        private readonly CHAR_INFO* _buffer;
        private readonly IntPtr _bufferPtr;
        private readonly IntPtr _std;
        private const int BufferSize = 0x100;

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
            WindowWidth = Syscon.WindowWidth;
            WindowHeight = Syscon.WindowHeight;
            _buffer = (CHAR_INFO*) (_bufferPtr = Marshal.AllocHGlobal(BufferSize * BufferSize * CHAR_INFO.SizeOf)).ToPointer();
            _std = GetStdHandle(-11);
            new Thread(() => // window size monitor
            {
                while (!_threadToken.IsCancellationRequested)
                {
                    int nw = Syscon.WindowWidth,
                        nh = Syscon.WindowHeight;
                    if (nw == WindowWidth && nh == WindowHeight) continue;
                    var pw = WindowWidth;
                    var ph = WindowHeight;
                    WindowWidth = nw;
                    WindowHeight = nh;
                    try
                    {
                        Syscon.SetWindowPosition(0, 0);
                    } catch { }
                    try
                    {
                        SizeChanged?.Invoke(this, new SizeChangedEventArgs(new Size(pw, ph), new Size(nw, nh)));
                    }
                    catch { /**/ }
                    try
                    {
                        Refresh();
                    }
                    catch { /**/ }
                    Thread.Sleep(16);
                }
                _shutdownEvent.Set();
            }).Start();
            new Thread(() => // keyboard monitor
            {
                while (!_threadToken.IsCancellationRequested)
                {
                    var ci = Syscon.ReadKey(true);
                    KeyPressed?.Invoke(this, ci);
                }
            }).Start();
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WriteConsoleOutputW(
            IntPtr consoleHandle,
            IntPtr buffer,
            COORD bufferSize,
            COORD bufferCoord,
            ref SMALL_RECT writeRegion);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int handle);

        private static readonly Action<IntPtr, byte, int> Memset;

        public int WindowWidth { get; private set; }

        public int WindowHeight { get; private set; }

        public bool CursorVisible
        {
            get { return Syscon.CursorVisible; }
            set { Syscon.CursorVisible = value; }
        }

        public int CursorX
        {
            get { return Syscon.CursorTop; }
            set { Syscon.CursorTop = value; }
        }

        public int CursorY
        {
            get { return Syscon.CursorTop; }
            set { Syscon.CursorTop = value; }
        }

        public void PutChar(Character chr, int x, int y)
        {
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
            var rect = new SMALL_RECT
            {
                Left = 0,
                Top = 0,
                Right = BufferSize - 1, // (short)(wid - 1),
                Bottom = BufferSize - 1, // (short)(hei - 1)
            };
            WriteConsoleOutputW(_std, _bufferPtr,
                new COORD { X = BufferSize, Y = BufferSize },
                new COORD(), ref rect);
        }

        public void Clear()
        {
            Memset(_bufferPtr, 0x00, BufferSize*BufferSize*CHAR_INFO.SizeOf);
        }

        public void Start()
        {
            _shutdownEvent.WaitOne();
        }

        public event EventHandler<SizeChangedEventArgs> SizeChanged;
        public event EventHandler<ConsoleKeyInfo> KeyPressed;

        public void Dispose()
        {
            _threadToken.Cancel();
        }
    }
}
