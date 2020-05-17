using System.Windows.Controls.Primitives;
using WpfToolkit.Controls;

namespace GroupMeClient.Extensions
{
    /// <summary>
    /// <see cref="FastVirtualizingWrapPanel"/> provides a version of <see cref="VirtualizingWrapPanel"/> that provides
    /// much faster mouse-scrolling behavior.
    /// </summary>
    public class FastVirtualizingWrapPanel : VirtualizingWrapPanel, IScrollInfo
    {
        /// <summary>
        /// Gets or sets the mouse wheel delta for pixel based scrolling. The default value is 48 dp in WPF.
        /// </summary>
        public new double MouseWheelDelta { get; set; } = 112.0;

        /// <inheritdoc/>
        public new void MouseWheelUp()
        {
            if (this.MouseWheelScrollDirection == ScrollDirection.Vertical)
            {
                this.ScrollVertical(-this.MouseWheelDelta);
            }
            else
            {
                this.MouseWheelLeft();
            }
        }

        /// <inheritdoc/>
        public new void MouseWheelDown()
        {
            if (this.MouseWheelScrollDirection == ScrollDirection.Vertical)
            {
                this.ScrollVertical(this.MouseWheelDelta);
            }
            else
            {
                this.MouseWheelRight();
            }
        }
    }
}
