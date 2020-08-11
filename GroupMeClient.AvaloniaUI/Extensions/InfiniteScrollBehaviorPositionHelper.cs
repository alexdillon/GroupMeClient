using Avalonia;
using Avalonia.Controls;

namespace GroupMeClient.AvaloniaUI.Extensions
{
    /// <summary>
    /// <see cref="InfiniteScrollBehaviorPositionHelper"/> provides support for easily binding to the scroll position
    /// of a <see cref="ListBox"/> when used in conjunction with the <see cref="InfiniteScrollBehavior"/> behavior.
    /// </summary>
    public class InfiniteScrollBehaviorPositionHelper
    {
        /// <summary>
        /// Gets an Avalonia Property indicating whether the list is not scrolled to the bottom.
        /// </summary>
        public static readonly AvaloniaProperty IsNotAtBottomProperty =
          AvaloniaProperty.RegisterAttached<ListBox, bool>(
              "IsNotAtBottom",
              typeof(InfiniteScrollBehavior),
              defaultValue: false);

        /// <summary>
        /// Gets a value indiciating whether the list is currently not scrolled to the bottom.
        /// </summary>
        /// <param name="obj">The dependency object to retreive the property from.</param>
        /// <returns>A boolean indicating whether the list is not at the bottom.</returns>
        public static bool GetIsNotAtBottom(AvaloniaObject obj)
        {
            return (bool)obj.GetValue(IsNotAtBottomProperty);
        }

        /// <summary>
        /// Sets a value indicating whether the list is currently not scrolled to the bottom.
        /// </summary>
        /// <param name="obj">The dependency object to retreive the property from.</param>
        /// <param name="value">Whether the list is not at the bottom.</param>
        public static void SetIsNotAtBottom(AvaloniaObject obj, bool value)
        {
            obj.SetValue(IsNotAtBottomProperty, value);
        }
    }
}
