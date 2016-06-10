using System.Linq;
using GoddamnConsole.Controls;
using GoddamnConsole.Drawing;
using GoddamnConsole.NativeProviders;
using Console = GoddamnConsole.Console;

namespace GoddamnConsoleSample
{
    internal class Program
    {
        private const string Lorem =
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit, " +
            "sed do eiusmod tempor incididunt ut labore et dolore magn" +
            "a aliqua. Ut enim ad minim veniam, quis nostrud exercitat" +
            "ion ullamco laboris nisi ut aliquip ex ea commodo consequ" +
            "at. Duis aute irure dolor in reprehenderit in voluptate v" +
            "elit esse cillum dolore eu fugiat nulla pariatur. Excepte" +
            "ur sint occaecat cupidatat non proident, sunt in culpa qu" +
            "i officia deserunt mollit anim id est laborum.";

        private static readonly string LongLorem = string.Join("\n", Enumerable.Repeat(Lorem, 3));

        private static void Main()
        {
            var ctl = new Grid
            {
                Children =
                {
                    new Border
                    {
                        Content =
                            new TextBox
                            {
                                Text = LongLorem,
                                TextWrapping = TextWrapping.Wrap,
                                Width = 200,
                                Height = 200
                            },
                        AttachedProperties =
                        {
                            new GridRowProperty {Row = 0, RowSpan = 2},
                            new GridColumnProperty {Column = 0},
                        }
                    },
                    new Border
                    {
                        Content =
                            new TextBox
                            {
                                Text = LongLorem,
                                TextWrapping = TextWrapping.Wrap,
                                Width = 200,
                                Height = 200
                            },
                        AttachedProperties =
                        {
                            new GridRowProperty {Row = 0},
                            new GridColumnProperty {Column = 1, ColumnSpan = 2},
                        }
                    },
                    new Border
                    {
                        Content =
                            new TextBox
                            {
                                Text = LongLorem,
                                TextWrapping = TextWrapping.Wrap,
                                Width = 200,
                                Height = 200
                            },
                        AttachedProperties =
                        {
                            new GridRowProperty {Row = 1},
                            new GridColumnProperty {Column = 1},
                        }
                    },
                    new Border
                    {
                        Content =
                            new TextBox
                            {
                                Text = LongLorem,
                                TextWrapping = TextWrapping.Wrap,
                                Width = 200,
                                Height = 200
                            },
                        AttachedProperties =
                        {
                            new GridRowProperty {Row = 1},
                            new GridColumnProperty {Column = 2},
                        }
                    },
                }
            };
            Console.Focused = ctl;
            Console.Start(new WindowsNativeConsoleProvider(), ctl);
        }
    }
}
