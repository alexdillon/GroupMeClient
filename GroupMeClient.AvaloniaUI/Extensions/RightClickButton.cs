using System;
using Avalonia.Controls;
using Avalonia.Input;

namespace GroupMeClient.AvaloniaUI.Extensions
{
    /// <summary>
    /// Extension of <see cref="Button"/> that responds to being clicked by <see cref="MouseButton.Right"/>.
    /// Note: The <see cref="Button.IsPressed"/> property does NOT work in this implementation,
    /// and the only supported <see cref="Button.ClickMode"/> is <see cref="ClickMode.Press"/>.
    /// </summary>
    public class RightClickButton : Button
    {
        /// <inheritdoc/>
        protected override Type StyleKeyOverride => typeof(Button);

        /// <inheritdoc/>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                e.Handled = true;

                if (this.ClickMode == ClickMode.Press)
                {
                    this.OnClick();
                }
            }
        }
    }
}
