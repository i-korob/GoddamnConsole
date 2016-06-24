using System;
using System.Collections.Generic;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    /// <summary>
    /// Represents an interface of control which can have children
    /// </summary>
    public interface IParentControl
    {
        Rectangle MeasureBoundingBox(Control child);
        Point GetScrollOffset(Control child);
        bool IsChildVisible(Control child);
    }
    
    public interface IAttachedProperty
    {
    }

    /// <summary>
    /// Represents an interface of control which can have only one child
    /// </summary>
    public interface IContentControl : IParentControl
    {
        /// <summary>
        /// Gets or sets child control
        /// </summary>
        Control Content { get; set; }

        /// <summary>
        /// Fires after child control is changed
        /// </summary>
        event EventHandler<ChildRemovedEventArgs> ContentDetached;
    }

    /// <summary>
    /// Represents an interface of control which can have more than one child
    /// </summary>
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