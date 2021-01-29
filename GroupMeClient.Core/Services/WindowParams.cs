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
    }
}
