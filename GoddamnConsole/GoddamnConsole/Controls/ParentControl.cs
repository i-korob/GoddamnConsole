
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    /// <summary>
    /// Represents a control which can have children
    /// </summary>
    public abstract class ParentControl : Control, IParentControl
    {
        /// <summary>
        /// Measures a child position and size, which will be used in rendering
        /// </summary>
        public abstract Rectangle MeasureBoundingBox(Control child);
        /// <summary>
        /// Returns a current scroll position of child
        /// </summary>
        public abstract Point GetScrollOffset(Control child);
        /// <summary>
        /// Returns a value that indicates whether child is visible
        /// </summary>
        public abstract bool IsChildVisible(Control child);
    }
}
