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
            Console.Start(new WindowsNativeConsoleProvider(), new TextBox
            {
                Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, " +
                       "sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
            });
        }
    }
}
