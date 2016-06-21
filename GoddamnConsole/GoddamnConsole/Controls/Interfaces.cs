using System;
using System.Collections.Generic;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public interface IParentControl
    {
        Rectangle MeasureBoundingBox(Control child);
        Point GetScrollOffset(Control child);
        bool IsChildVisible(Control child);
    }

    public interface IAttachedProperty
    {
    }
    
    public interface IContentControl : IParentControl
    {
        /// <summary>
        ///     Gets or sets child control
        /// </summary>
        Control Content { get; set; }

        /// <summary>
        ///     Fires after child control is changed
        /// </summary>
        event EventHandler<ChildRemovedEventArgs> ContentDetached;
    }

    public interface IChildrenControl : IParentControl
    {
        /// <summary>
        ///     Returns a collection that contains children
        /// </summary>
        IList<Control> Children { get; }

        /// <summary>
        ///     Returns a collection that contains children
        /// </summary>
        IList<Control> FocusableChildren { get; }

        /// <summary>
        ///     Fires after child control removed from children collection
        /// </summary>
        event EventHandler<ChildRemovedEventArgs> ChildRemoved;
    }
}