using System;
using System.Collections.Generic;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.NativeProviders
{
    public class SizeChangedEventArgs : EventArgs
    {
        public SizeChangedEventArgs(Size before, Size after)
        {
            Before = before;
            After = after;
        }

        public Size Before { get; }
        public Size After { get; }
    }

    public interface INativeConsoleProvider : IDisposable
    {
        int WindowWidth { get; }
        int WindowHeight { get; }
        bool CursorVisible { get; set; }
        int CursorX { get; set; }
        int CursorY { get; set; }

        void PutChar(Character chr, int x, int y);
        void CopyBlock(IEnumerable<Character> chars, int x, int y, int width, int height);
        void FillBlock(Character chr, int x, int y, int width, int height);
        void Refresh();
        void Start();
        void Clear();
        
        event EventHandler<SizeChangedEventArgs> SizeChanged;
        event EventHandler<ConsoleKeyInfo> KeyPressed;
    }
}
