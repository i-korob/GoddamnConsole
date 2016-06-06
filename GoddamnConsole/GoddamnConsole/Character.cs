using System;

namespace GoddamnConsole
{
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

        public char Char { get; }
        public CharColor Foreground { get; }
        public CharColor Background { get; }
        public CharAttribute Attribute { get; }

        public static implicit operator Character(char x)
        {
            return new Character(x, CharColor.Gray, CharColor.Black, CharAttribute.None);
        }
    }

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
