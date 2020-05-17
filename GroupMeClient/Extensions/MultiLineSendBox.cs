using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GroupMeClient.Extensions
{
    /// <summary>
    /// <see cref="MultiLineSendBox"/> provides support for typing MultiLine messages.
    /// Keyboard send triggers are supported.
    /// </summary>
    public class MultiLineSendBox : TextBox
    {
        /// <summary>
        /// The maximum message size, in characters, that can be sent.
        /// </summary>
        public const int MaximumMessageLength = 1000;

        /// <summary>
        /// Gets a RoutedEvent for when Send is invoked on this <see cref="MultiLineSendBox"/>.
        /// </summary>
        public static readonly RoutedEvent SendEvent = EventManager.RegisterRoutedEvent(
            "Send", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MultiLineSendBox));

        /// <summary>
        /// Gets a dependency property for the <see cref="Brush"/> used to render the text.
        /// </summary>
        public static readonly DependencyProperty RegularTextBrushProperty =
            DependencyProperty.Register(
                "RegularTextBrush",
                typeof(Brush),
                typeof(MultiLineSendBox),
                new PropertyMetadata(default(Brush), new PropertyChangedCallback(OnBrushChanged)));

        /// <summary>
        /// Gets a dependency property for the <see cref="Brush"/> used to render the text when a validation error has occured.
        /// </summary>
        public static readonly DependencyProperty ErrorTextBrushProperty =
            DependencyProperty.Register(
                "ErrorTextBrush",
                typeof(Brush),
                typeof(MultiLineSendBox),
                new PropertyMetadata(default(Brush), new PropertyChangedCallback(OnBrushChanged)));

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLineSendBox"/> class.
        /// </summary>
        public MultiLineSendBox()
        {
            this.KeyDown += this.TextBoxKeyDown;
            this.TextChanged += this.MultiLineSendBox_TextChanged;
            this.PreviewKeyDown += this.TextBoxPreviewKeyDown;
            this.PreviewTextInput += this.MultiLineSendBox_PreviewTextInput;

            var gesture = new KeyGesture(Key.V, ModifierKeys.Shift | ModifierKeys.Control);
            var customPaste = new InputBinding(ApplicationCommands.Paste, gesture);
            this.InputBindings.Add(customPaste);
        }

        /// <summary>
        /// Adds or removes an event handler for when Send is triggered.
        /// </summary>
        public event RoutedEventHandler Send
        {
            add { this.AddHandler(SendEvent, value); }
            remove { this.RemoveHandler(SendEvent, value); }
        }

        /// <summary>
        /// Gets or sets which brush will be used to render the text when no validation errors occur.
        /// </summary>
        public Brush RegularTextBrush
        {
            get { return (Brush)this.GetValue(RegularTextBrushProperty); }
            set { this.SetValue(RegularTextBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the brush that will be used to render the text when validation errors occur.
        /// </summary>
        public Brush ErrorTextBrush
        {
            get { return (Brush)this.GetValue(ErrorTextBrushProperty); }
            set { this.SetValue(ErrorTextBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the contents that is "typed" into this <see cref="TextBox"/> while it is read-only and sending.
        /// </summary>
        private string ReadOnlyBuffer { get; set; }

        /// <inheritdoc/>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == IsReadOnlyProperty)
            {
                if ((bool)e.NewValue == false && (bool)e.OldValue == true)
                {
                    if (!string.IsNullOrEmpty(this.ReadOnlyBuffer))
                    {
                        TextCompositionManager.StartComposition(new TextComposition(InputManager.Current, this, this.ReadOnlyBuffer));
                        this.ReadOnlyBuffer = string.Empty;
                    }
                }
            }

            base.OnPropertyChanged(e);
        }

        private static void OnBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MultiLineSendBox mlsb)
            {
                mlsb.MultiLineSendBox_TextChanged(mlsb, null);
            }
        }

        private void MultiLineSendBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.Text.Length > MaximumMessageLength)
            {
                this.Foreground = this.ErrorTextBrush;
            }
            else
            {
                this.Foreground = this.RegularTextBrush;
            }
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            // This will never happen because the Enter Key is handled before
            // That means TextBoxKeyDown is not triggered for the Enter key
            if (e.Key == Key.Enter &&
                !(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                !(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.LeftShift)))
            {
                this.RaiseSendEvent();
            }
        }

        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Enter key is routed and the PreviewKeyDown is also fired with the
            // Enter key
            // You don't want to clear the box when CTRL and/or SHIFT is down
            if (e.Key == Key.Enter &&
                !(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                !(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                e.Handled = true;
                this.RaiseSendEvent();
            }
        }

        private void MultiLineSendBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (this.IsReadOnly)
            {
                this.ReadOnlyBuffer += e.Text;
            }
        }

        private void RaiseSendEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(SendEvent);
            this.RaiseEvent(newEventArgs);
        }
    }
}
