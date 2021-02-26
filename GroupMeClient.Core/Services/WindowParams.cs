using System;
using GalaSoft.MvvmLight;

namespace GroupMeClient.Core.Services
{
    /// <summary>
    /// Parameters that define a generic window or dialog displaying content.
    /// </summary>
    public class WindowParams
    {
        /// <summary>
        /// Placement locations for an on-screen window.
        /// </summary>
        public enum Location
        {
            /// <summary>
            /// Center of the screen.
            /// </summary>
            CenterScreen,

            /// <summary>
            /// The bottom left corner of the screen.
            /// </summary>
            BottomLeft,

            /// <summary>
            /// The bottom right corner of the screen.
            /// </summary>
            BottomRight,

            /// <summary>
            /// The top left corner of the screen.
            /// </summary>
            TopLeft,

            /// <summary>
            /// The top right corner of the screen.
            /// </summary>
            TopRight,

            /// <summary>
            /// The default location, as determined by the system.
            /// </summary>
            Default,

            /// <summary>
            /// A manually set X, Y position.
            /// </summary>
            Manual,
        }

        /// <summary>
        /// Gets or sets the contents to display in the window.
        /// </summary>
        public ViewModelBase Content { get; set; }

        /// <summary>
        /// Gets or sets the window's title.
        /// </summary>
        public string Title { get; set; } = "GroupMe Desktop Client";

        /// <summary>
        /// Gets or sets the width of the window.
        /// If <c>0</c>, the window will be automatically sized.
        /// </summary>
        public int Width { get; set; } = 0;

        /// <summary>
        /// Gets or sets the height of the window.
        /// If <c>0</c>, the window will be automatically sized.
        /// </summary>
        public int Height { get; set; } = 0;

        /// <summary>
        /// Gets or sets the callback that will be executed when the window is closed.
        /// </summary>
        public Action CloseCallback { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this window should be displayed above all others.
        /// </summary>
        public bool TopMost { get; set; } = false;

        /// <summary>
        /// Gets or sets an optional tag value.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Gets or sets the default window starting location.
        /// </summary>
        public Location StartingLocation { get; set; } = Location.Default;

        /// <summary>
        /// Gets or sets the starting X position of the window if <see cref="StartingLocation"/>
        /// is set to <see cref="Location.Manual"/>.
        /// </summary>
        public double StartingX { get; set; }

        /// <summary>
        /// Gets or sets the starting Y position of the window if <see cref="StartingLocation"/>
        /// is set to <see cref="Location.Manual"/>.
        /// </summary>
        public double StartingY { get; set; }
    }
}
