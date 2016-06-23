using System.ComponentModel;
using System.Linq;
using GoddamnConsole;
using GoddamnConsole.Controls;
using GoddamnConsole.Drawing;
using GoddamnConsole.NativeProviders.Windows;
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
            var ctl = new TabControl();
            var tab1 = new Tab
            {
                Name = "Grid",
                Content = new Grid
                {
                    ColumnDefinitions =
                    {
                        new GridSize(GridUnitType.Fixed, 10),
                        new GridSize(GridUnitType.Grow, 1),
                        new GridSize(GridUnitType.Grow, 2)
                    },
                    RowDefinitions =
                    {
                        new GridSize(GridUnitType.Fixed, 5),
                        new GridSize(GridUnitType.Auto, 0)
                    },
                    Children =
                    {
                        new Border
                        {
                            Content =
                                new TextView
                                {
                                    Text = LongLorem,
                                    TextWrapping = TextWrapping.Wrap
                                },
                            AttachedProperties =
                            {
                                new GridProperties
                                {
                                    RowSpan = 2
                                }
                            },
                            FrameStyle = FrameStyle.Double
                        },
                        new Border
                        {
                            Content =
                                new TextView
                                {
                                    Text = LongLorem,
                                    TextWrapping = TextWrapping.Wrap
                                },
                            AttachedProperties =
                            {
                                new GridProperties
                                {
                                    Column = 1,
                                    ColumnSpan = 2
                                }
                            },
                            FrameStyle = FrameStyle.Fill
                        },
                        new Border
                        {
                            Content =
                                new TextView
                                {
                                    Text = LongLorem,
                                    TextWrapping = TextWrapping.Wrap
                                },
                            AttachedProperties =
                            {
                                new GridProperties
                                {
                                    Row = 1,
                                    Column = 1
                                }
                            },
                            FrameStyle = FrameStyle.Simple
                        },
                        new Border
                        {
                            Content =
                                new TextView
                                {
                                    Text = LongLorem,
                                    TextWrapping = TextWrapping.Wrap
                                },
                            AttachedProperties =
                            {
                                new GridProperties
                                {
                                    Row = 1,
                                    Column = 2
                                }
                            },
                            Height = 10
                        }
                    }
                }
            };
            var tab2 = new Tab
            {
                Name = "TextBox",
                Content = new TextBox
                {
                    Text = "You can write here!"
                }
            };
            var tab3 = new Tab
            {
                Name = "ScrollViewer + TextView",
                Content = new ScrollViewer
                {
                    Content = new TextView
                    {
                        Text = LongLorem,
                        Background = CharColor.Blue,
                        Foreground = CharColor.LightYellow,
                        Height = ControlSizeType.MaxByContent
                    }
                }
            };
            ctl.Children.Add(tab1);
            ctl.Children.Add(tab2);
            ctl.Children.Add(tab3);
            ctl.SelectedTab = tab2;
            Console.Windows.Add(new Window
            {
                Content = ctl,
                Title = "First Window",
                Width = 80
            });
            var btn = new Button
            {
                Text = "Button!",
                AttachedProperties =
                {
                    new GridProperties
                    {
                        Row = 1
                    }
                }
            };
            var text =
                new TextView
                {
                    Text = "Second window!",
                    Width = ControlSizeType.MaxByContent,
                    Height = ControlSizeType.MaxByContent
                };
            var clkCnt = 0;
            btn.Clicked += (o, e) => text.Text = $"Click #{++clkCnt}";
            Console.Windows.Add(new Window
            {
                Content = new Grid
                {
                    Children =
                    {
                        text,
                        btn
                    },
                    RowDefinitions =
                    {
                        new GridSize(GridUnitType.Auto, 0),
                        new GridSize(GridUnitType.Auto, 0)
                    }
                },
                Title = "Second Window",
                Width = 20,
                Height = 8,
                HorizontalAlignment = WindowAlignment.End,
                VerticalAlignment = WindowAlignment.Center
            });
            Console.Start(new WindowsNativeConsoleProvider());
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
