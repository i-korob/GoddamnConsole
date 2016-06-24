using System;

namespace GoddamnConsole
{
    /// <summary>
    /// Represents a structure that contains character and its attributes
    /// </summary>
    public struct Character
    {
        public Character(char chr)
        {
            Char = chr;
            Foreground = CharColor.Gray;
            Background = CharColor.Black;
            Attribute = CharAttribute.None;
        }

        public Character(char chr, CharColor foreground, CharColor background,
            CharAttribute attr)
        {
            Char = chr;
            Foreground = foreground;
            Background = background;
            Attribute = attr;
        }

        /// <summary>
        /// Returns the character
        /// </summary>
        public char Char { get; }
        /// <summary>
        /// Returns the character foreground
        /// </summary>
        public CharColor Foreground { get; }
        /// <summary>
        /// Returns the character background
        /// </summary>
        public CharColor Background { get; }
        /// <summary>
        /// Returns the character attributes
        /// </summary>
        public CharAttribute Attribute { get; }

        public static implicit operator Character(char x)
        {
            return new Character(x, CharColor.Gray, CharColor.Black, CharAttribute.None);
        }
    }

    /// <summary>
    /// Describes the character colors
    /// </summary>
    public enum CharColor : short
    {
        Black = 0,
        Blue = 1,
        Green = 2,
        Cyan = 3,
        Red = 4,
        Magenta = 5,
        Yellow = 6,
        Gray = 7,
        DarkGray = 8,
        LightBlue = 9,
        LightGreen = 0xa,
        LightCyan = 0xb,
        LightRed = 0xc,
        LightMagenta = 0xd,
        LightYellow = 0xe,
        White = 0xf
    }

    /// <summary>
    /// Returns the character attributes
    /// </summary>
    [Flags]
    public enum CharAttribute : short
    {
        None = 0,
        TopLine = 0x400,
        BottomLine = unchecked((short)0x8000),
        LeftLine = 0x800,
        RightLine = 0x1000
    }
}
