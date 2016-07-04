using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    /// <summary>
    /// Represents a console content container
    /// </summary>
    public abstract class WindowBase : ParentControl
    {
        private string _title;
        private Alignment _horizontalAlignment;
        private Alignment _verticalAlignment;

        /// <summary>
        /// Gets or sets a vertical alignment style
        /// </summary>
        public Alignment VerticalAlignment
        {
            get { return _verticalAlignment; }
            set { _verticalAlignment = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets a horizontal alignment style
        /// </summary>
        public Alignment HorizontalAlignment
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
}
