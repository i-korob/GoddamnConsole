using System.ComponentModel;
using System.Linq;
using GoddamnConsole;
using GoddamnConsole.Controls;
using GoddamnConsole.Drawing;
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

        private static readonly string LongLorem = string.Join("\n", Enumerable.Repeat(Lorem, 10));

        private static void Main()
        {
            var gridWindowTest = new GridWindow
            {
                Title = "GridWindow Test (Next: Shift+Tab)",
                DrawBorders = true,
                RowDefinitions =
                {
                    new GridSize(GridUnitType.Fixed, 10),
                    new GridSize(GridUnitType.Auto, 0),
                    new GridSize(GridUnitType.Auto, 0),
                },
                ColumnDefinitions =
                {
                    new GridSize(GridUnitType.Grow, 3),
                    new GridSize(GridUnitType.Grow, 2),
                    new GridSize(GridUnitType.Fixed, 15)
                },
                Children =
                {
                    new Border
                    {
                        FrameStyle = FrameStyle.Single,
                        Content = new TextView
                        {
                            Text = "1"
                        },
                        AttachedProperties =
                        {
                            new GridProperties
                            {
                                Row = 0,
                                Column = 0,
                                RowSpan = 3
                            }
                        }
                    },
                    new Border
                    {
                        FrameStyle = FrameStyle.Single,
                        Content = new TextView
                        {
                            Text = "2"
                        },
                        AttachedProperties =
                        {
                            new GridProperties
                            {
                                Row = 0,
                                Column = 1,
                                ColumnSpan = 2
                            }
                        }
                    },
                    new Border
                    {
                        FrameStyle = FrameStyle.Single,
                        Content = new TextView
                        {
                            Text = "3"
                        },
                        AttachedProperties =
                        {
                            new GridProperties
                            {
                                Row = 1,
                                Column = 1
                            }
                        }
                    },
                    new Border
                    {
                        FrameStyle = FrameStyle.Single,
                        Content = new TextView
                        {
                            Text = "4"
                        },
                        AttachedProperties =
                        {
                            new GridProperties
                            {
                                Row = 2,
                                Column = 2
                            }
                        }
                    }
                }
            };
            var btn = new Button
            {
                Text = "Press me!",
                Height = ControlSizeType.MaxByContent
            };
            var text = new TextView
            {
                Text = "Click count: 0",
                Height = ControlSizeType.MaxByContent
            };
            var clkCnt = 0;
            btn.Clicked += (o, e) => text.Text = $"Click count: {++clkCnt}";
            var tabControlTest = new ContentWindow
            {
                Title = "ContentWindow + TabControl Test (Prev: Shift+Tab)",
                Content = new TabControl
                {
                    Children =
                    {
                        new Tab
                        {
                            Name = "TextView",
                            Content = new TextView
                            {
                                Text = "Read-only text!\n" + LongLorem
                            }
                        },
                        new Tab
                        {
                            Name = "Vertical StackPanel",
                            Content = new StackPanel
                            {
                                Children =
                                {
                                    new Border
                                    {
                                        Content =
                                            new TextView
                                            {
                                                Text = "Item #1",
                                                Height = ControlSizeType.MaxByContent
                                            },
                                        Height = ControlSizeType.MaxByContent
                                    },
                                    new Border
                                    {
                                        Content =
                                            new TextView
                                            {
                                                Text = "Item #2",
                                                Height = ControlSizeType.MaxByContent
                                            },
                                        Height = ControlSizeType.MaxByContent
                                    },
                                    new Border
                                    {
                                        Content =
                                            new TextView
                                            {
                                                Text = "Item #3",
                                                Height = ControlSizeType.MaxByContent
                                            },
                                        Height = ControlSizeType.MaxByContent
                                    },
                                    new Border
                                    {
                                        Content =
                                            new TextView
                                            {
                                                Text = "Item #4",
                                                Height = ControlSizeType.MaxByContent
                                            },
                                        Height = ControlSizeType.MaxByContent
                                    }
                                }
                            }
                        },
                        new Tab
                        {
                            Name = "Horizontal StackPanel",
                            Content = new StackPanel
                            {
                                Orientation = StackPanelOrientation.Horizontal,
                                Children =
                                {
                                    new Border
                                    {
                                        Content =
                                            new TextView
                                            {
                                                Text = "Item #1",
                                                Width = ControlSizeType.MaxByContent
                                            },
                                        Width = ControlSizeType.MaxByContent
                                    },
                                    new Border
                                    {
                                        Content =
                                            new TextView
                                            {
                                                Text = "Item #2",
                                                Width = ControlSizeType.MaxByContent
                                            },
                                        Width = ControlSizeType.MaxByContent
                                    },
                                    new Border
                                    {
                                        Content =
                                            new TextView
                                            {
                                                Text = "Item #3",
                                                Width = ControlSizeType.MaxByContent
                                            },
                                        Width = ControlSizeType.MaxByContent
                                    },
                                    new Border
                                    {
                                        Content =
                                            new TextView
                                            {
                                                Text = "Item #4",
                                                Width = ControlSizeType.MaxByContent
                                            },
                                        Width = ControlSizeType.MaxByContent
                                    }
                                }
                            }
                        },
                        new Tab
                        {
                            Name = "ScrollViewer",
                            Content = new ScrollViewer
                            {
                                Content = new TextView
                                {
                                    Text = LongLorem,
                                    TextWrapping = TextWrapping.Wrap,
                                    Height = ControlSizeType.MaxByContent
                                }
                            }
                        },
                        new Tab
                        {
                            Name = "TextBox",
                            Content = new TextBox
                            {
                                Text = "Hello World!",
                                TextWrapping = TextWrapping.Wrap
                            }
                        },
                        new Tab
                        {
                            Name = "Button",
                            Content = new StackPanel
                            {
                                Children =
                                {
                                    new TextView
                                    {
                                        Text = "Press that",
                                        Height = ControlSizeType.MaxByContent
                                    },
                                    btn,
                                    text
                                }
                            }
                        }
                    },
                    SelectedIndex = 0
                }
            };
            Console.Windows.Add(gridWindowTest);
            Console.Windows.Add(tabControlTest);
            Console.Start();
        }
    }

    public class TestClass : INotifyPropertyChanged
    {
        public class TestNestedClass : INotifyPropertyChanged
        {
            public class TestNestedClass2 : INotifyPropertyChanged
            {
                private int _testProperty;
                public event PropertyChangedEventHandler PropertyChanged;

                public int TestProperty
                {
                    get { return _testProperty; }
                    set
                    {
                        _testProperty = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TestProperty)));
                    }
                }
            }

            private TestNestedClass2 _testProperty;
            public event PropertyChangedEventHandler PropertyChanged;

            public TestNestedClass2 TestNestedProperty2
            {
                get { return _testProperty; }
                set
                {
                    _testProperty = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TestNestedProperty2)));
                }
            }
        }

        private TestNestedClass _testProperty;
        public event PropertyChangedEventHandler PropertyChanged;

        public TestNestedClass TestNestedProperty
        {
            get { return _testProperty; }
            set
            {
                _testProperty = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TestNestedProperty)));
            }
        }
    }
}
