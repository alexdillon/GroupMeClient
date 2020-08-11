using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace GroupMeClient.AvaloniaUI.Extensions
{
    /// <summary>
    /// <see cref="TextBoxSendBehavior"/> provides support for typing MultiLine messages.
    /// Keyboard send triggers are supported.
    /// </summary>
    public class TextBoxSendBehavior : Behavior<TextBox>
    {
        /// <summary>
        /// Gets an Avalonia Property for the command to execute when sending is invoked.
        /// </summary>
        public static readonly DirectProperty<TextBoxSendBehavior, ICommand> SendCommandProperty =
            AvaloniaProperty.RegisterDirect<TextBoxSendBehavior, ICommand>(
                nameof(SendCommand),
                tsb => tsb.SendCommand,
                (tsb, command) => tsb.SendCommand = command);

        private ICommand sendCommand;

        /// <summary>
        /// Gets or sets the command to execute when send behavior is invoked.
        /// </summary>
        public ICommand SendCommand
        {
            get => this.sendCommand;
            set => this.SetAndRaise(SendCommandProperty, ref this.sendCommand, value);
        }

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.KeyDown += this.AssociatedObject_KeyDown;

            this.AssociatedObject.AddHandler(
             InputElement.KeyDownEvent,
             this.AssociatedObject_KeyDown,
             handledEventsToo: true);

            // If AcceptsReturn is true, the TextBox will automatically consume KeyDown events and mark them as handled.
            this.AssociatedObject.AcceptsReturn = true;
            this.AssociatedObject.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            base.OnDetaching();

            try
            {
                this.AssociatedObject.RemoveHandler(
                    InputElement.KeyDownEvent,
                    this.AssociatedObject_KeyDown);
            }
            catch (Exception)
            {
            }
        }

        private void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
        {
            var controlPressed = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            var shiftPressed = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

            if ((e.Key == Key.Enter || e.Key == Key.Return) &&
                (!controlPressed && !shiftPressed))
            {
                // Enter or Return has been pressed without any modifiers
                // Remove the newline character that Avalonia's textbox will automatically insert
                // Then, invoke send behavior
                var beforeCaret = this.AssociatedObject.Text.Substring(0, this.AssociatedObject.CaretIndex);
                var afterCaret = this.AssociatedObject.Text.Substring(this.AssociatedObject.CaretIndex);
                beforeCaret = beforeCaret.Substring(0, beforeCaret.LastIndexOf(this.AssociatedObject.NewLine));
                var newString = $"{beforeCaret}{afterCaret}";
                this.AssociatedObject.Text = newString;

                this.SendCommand?.Execute(null);
            }
            else if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                // No special behavior is required. Enter has been pressed with a modifier key.
                // Allow Avalonia's TextBox to automatically handle newline insertion.
            }
        }
    }
}
