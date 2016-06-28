using System;
using System.Runtime.InteropServices;

namespace GoddamnConsole.NativeProviders.Unix
{
    internal static unsafe class NativeMethods
    {
        private const string NcursesLib = "libncursesw.so.5";

        [DllImport(NcursesLib)]
        public static extern IntPtr initscr();

        [DllImport(NcursesLib)]
        public static extern int endwin();

        [DllImport(NcursesLib)]
        public static extern int start_color();

        [DllImport(NcursesLib)]
        public static extern int refresh();

        [DllImport(NcursesLib)]
        public static extern int curs_set(int visibility);

        [DllImport(NcursesLib)]
        public static extern int add_wch(cchar_t* chr);

        [DllImport(NcursesLib)]
        public static extern int init_pair(short color, short f, short b);

        [StructLayout(LayoutKind.Sequential)]
        // ReSharper disable once InconsistentNaming
        public struct cchar_t
        {
            public long attr;
            public fixed int chars[5];
        }

        [DllImport(NcursesLib)]
        public static extern void move(int y, int x);

        [DllImport(NcursesLib)]
        public static extern int resizeterm(int lines, int cols);
    }
}