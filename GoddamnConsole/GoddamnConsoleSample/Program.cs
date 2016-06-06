using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoddamnConsole;
using GoddamnConsole.Controls;
using GoddamnConsole.Drawing;
using GoddamnConsole.NativeProviders;

namespace GoddamnConsoleSample
{
    class Program
    {
        static void Main()
        {
            Console.Start(new WindowsNativeConsoleProvider(), new SampleControl());
        }
    }

    class SampleControl : Control
    {
        protected override void OnRender(DrawingContext context)
        {
            context.DrawFrame(new Rectangle(20, 5, 10, 20));
            var shrunk = context.Shrink(new Rectangle(21, 6, 8, 20));
            var scrolled = shrunk.Scroll(new Point(0, 5));
            shrunk.DrawText(new Rectangle(0, 0, 8, 5), "Lorem ipsum dolor sit amet",
                new TextOptions { TextWrapping = TextWrapping.Wrap });
            scrolled.DrawText(new Rectangle(0, 0, 8, 5), "Lorem ipsum dolor sit amet",
                new TextOptions { TextWrapping = TextWrapping.Wrap });
        }
    }
}
