using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AccordBoard
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern ushort GetKeyState(int nVirtKey);

        const string Layout = "VBUIOP";

        private static bool _shift = false;

        private const string Special = "(){}[]";
        
        static string Decode(int state)
        {
            if ((state & 0x20) > 0)
            {
                _shift = true;
                return " <SHIFT> ";
            }
            var val = state & 0x1f;
            var chr = val < 26 ?  (char) (val + 'a' - 1) : Special[val - 26];
            if (_shift)
            {
                chr = char.ToUpper(chr);
                _shift = false;
            }
            return chr.ToString();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("AccordBoard Simulator by Zawodskoj (layout VBUIOP)");
            var state = 0;
            while (true)
            {
                var nstate = 0;
                foreach (var btn
                    in Layout) nstate = (nstate << 1) + ((GetKeyState(btn) & 0x8000) > 0 ? 1 : 0);
                if (nstate == 0 && state != 0)
                {
                    Console.Write(Decode(state));
                    state = 0;
                }
                state |= nstate;
            }
        }
    }
}
