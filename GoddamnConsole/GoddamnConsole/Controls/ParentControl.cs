using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public abstract class ParentControl : Control, IParentControl
    {
        public abstract Rectangle MeasureBoundingBox(Control child);
        public abstract Point GetScrollOffset(Control child);
        public abstract bool IsChildVisible(Control child);
    }
}
