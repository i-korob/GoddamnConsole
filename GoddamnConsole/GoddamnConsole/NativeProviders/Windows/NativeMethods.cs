using System;
using System.Runtime.InteropServices;

namespace GoddamnConsole.NativeProviders.Windows
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteConsoleOutputW(
            IntPtr consoleHandle,
            IntPtr buffer,
            WindowsNativeConsoleProvider.COORD bufferSize,
            WindowsNativeConsoleProvider.COORD bufferCoord,
            ref WindowsNativeConsoleProvider.SMALL_RECT writeRegion);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadConsoleInputW(
            IntPtr consoleHandle,
            IntPtr buffer,
            uint length,
            ref uint readChars);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetConsoleCursorPosition(
            IntPtr consoleHandle,
            WindowsNativeConsoleProvider.COORD coord);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetConsoleScreenBufferInfo(
            IntPtr consoleHandle,
            ref WindowsNativeConsoleProvider.CONSOLE_SCREEN_BUFFER_INFO bufInfo);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetConsoleCursorInfo(
            IntPtr consoleHandle,
            ref WindowsNativeConsoleProvider.CONSOLE_CURSOR_INFO bufInfo);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetConsoleCursorInfo(
            IntPtr consoleHandle,
            ref WindowsNativeConsoleProvider.CONSOLE_CURSOR_INFO bufInfo);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetConsoleScreenBufferSize(
            IntPtr consoleHandle,
            WindowsNativeConsoleProvider.COORD size);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int handle);

    }
}
