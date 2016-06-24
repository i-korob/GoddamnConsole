using System;
using System.Collections.Generic;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.NativeProviders
{
    /// <summary>
    /// Provides data for the SizeChanged event
    /// </summary>
    public class SizeChangedEventArgs : EventArgs
    {
        public SizeChangedEventArgs(Size before, Size after)
        {
            Before = before;
            After = after;
        }

        /// <summary>
        /// Returns a previous size
        /// </summary>
        public Size Before { get; }
        /// <summary>
        /// Returns a new size
        /// </summary>
        public Size After { get; }
    }

    /// <summary>
    /// Provides data for the KeyPressed event
    /// </summary>
    public class KeyPressedEventArgs : EventArgs
    {
        public KeyPressedEventArgs(ConsoleKeyInfo info)
        {
            Info = info;
        }

        /// <summary>
        /// Returns the keyboard button info
        /// </summary>
        public ConsoleKeyInfo Info { get; }
    }

    /// <summary>
    /// Represents an interface which abstracts console from low-level code
    /// </summary>
    public interface INativeConsoleProvider : IDisposable
    {
        /// <summary>
        /// Returns current window width
        /// </summary>
        int WindowWidth { get; }
        /// <summary>
        /// Returns current window height
        /// </summary>
        int WindowHeight { get; }
        /// <summary>
        /// Returns a value that indicates whether console cursor is visible
        /// </summary>
        bool CursorVisible { get; set; }
        /// <summary>
        /// Returns current cursor horizontal position
        /// </summary>
        int CursorX { get; set; }
        /// <summary>
        /// Returns current cursor vertical position
        /// </summary>
        int CursorY { get; set; }

        /// <summary>
        /// Draws character at specified position
        /// </summary>
        void PutChar(Character chr, int x, int y);
        /// <summary>
        /// Draws a block of chars at specified position with specified size
        /// </summary>
        void CopyBlock(IEnumerable<Character> chars, int x, int y, int width, int height);
        /// <summary>
        /// Fills a block with specified character
        /// </summary>
        void FillBlock(Character chr, int x, int y, int width, int height);
        /// <summary>
        /// Performs a console redraw
        /// </summary>
        void Refresh();
        /// <summary>
        /// Starts rendering cycle and waiting for shutdown
        /// </summary>
        void Start();
        /// <summary>
        /// Fills a console area with specified background color
        /// </summary>
        /// <param name="background"></param>
        void Clear(CharColor background);
        
        /// <summary>
        /// Occurs when console size has been changed
        /// </summary>
        event EventHandler<SizeChangedEventArgs> SizeChanged;
        /// <summary>
        /// Occurs when keyboard button has been pressed
        /// </summary>
        event EventHandler<KeyPressedEventArgs> KeyPressed;
    }
}
