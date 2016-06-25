using System;
using System.Runtime.InteropServices;

namespace GoddamnConsole.NativeProviders.Unix
{
    internal static unsafe class NativeMethods
    {
        [DllImport("libncursesw.so.5.9")]
        public static extern IntPtr initscr();

        [DllImport("libncursesw.so.5.9")]
        public static extern int endwin();

        [DllImport("libncursesw.so.5.9")]
        public static extern int start_color();

        [DllImport("libncursesw.so.5.9")]
        public static extern int refresh();

        [DllImport("libncursesw.so.5.9")]
        public static extern int add_wch(cchar_t* chr);

        [DllImport("libncursesw.so.5.9")]
        public static extern int init_pair(short color, short f, short b);

        [StructLayout(LayoutKind.Sequential)]
        // ReSharper disable once InconsistentNaming
        public struct cchar_t
        {
            public long attr;
            public fixed int chars[5];
        }

        [DllImport("libncursesw.so.5.9")]
        public static extern void move(int y, int x);

        [DllImport("libncursesw.so.5.9")]
        public static extern int resizeterm(int lines, int cols);
    }
}
