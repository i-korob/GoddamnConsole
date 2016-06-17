using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using GoddamnConsole;
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
                            new GridRowProperty {Row = 0, RowSpan = 2},
                            new GridColumnProperty {Column = 0},
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
                            new GridRowProperty {Row = 0},
                            new GridColumnProperty {Column = 1, ColumnSpan = 2},
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
                            new GridRowProperty {Row = 1},
                            new GridColumnProperty {Column = 1},
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
                            new GridRowProperty {Row = 1},
                            new GridColumnProperty {Column = 2},
                        },
                        Height = 10
                    }
                }
            };
            var tc = new TestClass();
            var tnc = new TestClass.TestNestedClass();
            var tnc2 = new TestClass.TestNestedClass.TestNestedClass2();
            tc.TestNestedProperty = tnc;
            tnc.TestNestedProperty2 = tnc2;
            tnc2.TestProperty = 20;
            ctl.DataContext = tc;
            ctl.Bind(nameof(ctl.Width), "TestNestedProperty.TestNestedProperty2.TestProperty");
            Debug.Assert(ctl.Width == 20);
            tnc2.TestProperty = 30;
            Debug.Assert(ctl.Width == 30);
            tnc.TestNestedProperty2 = new TestClass.TestNestedClass.TestNestedClass2
            {
                TestProperty = 40
            };
            Debug.Assert(ctl.Width == 40);
            tnc2.TestProperty = 50;
            tc.TestNestedProperty = new TestClass.TestNestedClass {TestNestedProperty2 = tnc2};
            Debug.Assert(ctl.Width == 50);
            Console.Focused = ctl;
            var popup = new Border
            {
                Width = 25,
                Height = 12,
                Content = new Grid
                {
                    RowDefinitions =
                    {
                        new GridSize(GridUnitType.Auto, 0),
                        new GridSize(GridUnitType.Fixed, 7),
                    },
                    ColumnDefinitions =
                    {
                        new GridSize(GridUnitType.Auto, 0)
                    },
                    Children =
                    {
                        new TextView
                        {
                            Text = "\n  Sample popup window  \n",
                            Background = CharColor.White,
                            Foreground = CharColor.Red,
                            AttachedProperties =
                            {
                                new GridRowProperty { Row = 0 },
                                new GridColumnProperty { Column = 0 }
                            },
                            Width = 23,
                            Height = 3
                        },
                        new Border
                        {
                            Content = new TextBox
                            {
                                Text = "Editable text\n\n\nVery very very long line ____________________\nOther line ____________\n\nAnother line\n\n\nYet another line"
                            },
                            AttachedProperties =
                            {
                                new GridRowProperty { Row = 1 },
                                new GridColumnProperty { Column = 0 }
                            }
                        }
                    }
                },
                FrameColor = CharColor.LightGreen
            };
            Console.IsPopupVisible = true;
            Console.Popup = popup;
            Console.Start(new WindowsNativeConsoleProvider(), ctl);
        }
    }

    class TestClass : INotifyPropertyChanged
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
