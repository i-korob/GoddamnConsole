using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    /// <summary>
    /// Represents a console content container
    /// </summary>
    public abstract class WindowBase : ParentControl
    {
        private string _title;
        private WindowAlignment _horizontalAlignment;
        private WindowAlignment _verticalAlignment;

        /// <summary>
        /// Gets or sets a vertical alignment style
        /// </summary>
        public WindowAlignment VerticalAlignment
        {
            get { return _verticalAlignment; }
            set { _verticalAlignment = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets a horizontal alignment style
        /// </summary>
        public WindowAlignment HorizontalAlignment
        {
            get { return _horizontalAlignment; }
            set { _horizontalAlignment = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets a window title
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged(); }
        }
    }

    /// <summary>
    /// Describes a kind of window alignment
    /// </summary>
    public enum WindowAlignment
    {
        /// <summary>
        /// WindowBase is aligned to begin of console area (Top/Left)
        /// </summary>
        Begin,
        /// <summary>
        /// WindowBase is aligned to end of console area (Bottom/Right)
        /// </summary>
        End,
        /// <summary>
        /// WindowBase is aligned to center of console area
        /// </summary>
        Center
    }
}
