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
            context.DrawRectangle(new Rectangle(-10, -10, 20, 20), '#');
            context.DrawRectangle(new Rectangle(20, 5, 20, 20), '!');
            context.DrawRectangle(new Rectangle(50, 20, 200, 200), '@');
        }
    }
}
