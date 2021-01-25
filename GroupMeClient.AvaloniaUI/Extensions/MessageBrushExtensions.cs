using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace GroupMeClient.AvaloniaUI.Extensions
{
    /// <summary>
    /// <see cref="MessageBrushExtensions"/> provides extension methods for <see cref="Grid"/> for displaying correct background Brush.
    /// </summary>
    public class MessageBrushExtensions
    {
        /// <summary>
        /// An Avalonia Attached property to represent the rendering Brush of the message background when the user is the sender.
        /// </summary>
        public static readonly AvaloniaProperty<IBrush> MessageISentBrushProperty =
               AvaloniaProperty.RegisterAttached<Grid, IBrush>(
                "MessageISentBrush",
                typeof(MessageBrushExtensions),
                defaultValue: default(SolidColorBrush));

        /// <summary>
        /// An Avalonia Attached property to represent the rendering Brush of the message background when another user is the sender.
        /// </summary>
        public static readonly AvaloniaProperty<IBrush> MessageTheySentBrushProperty =
               AvaloniaProperty.RegisterAttached<Grid, IBrush>(
                "MessageTheySentBrush",
                typeof(MessageBrushExtensions),
                defaultValue: default(SolidColorBrush));

        /// <summary>
        /// An Avalonia Attached property to represent if the sender was the user.
        /// </summary>
        public static readonly AvaloniaProperty<bool> MessageSenderProperty =
               AvaloniaProperty.RegisterAttached<Grid, bool>(
                "MessageSender",
                typeof(MessageBrushExtensions),
                defaultValue: default(bool));

        /// <summary>
        /// Initializes static members of the <see cref="MessageBrushExtensions"/> class.
        /// </summary>
        static MessageBrushExtensions()
        {
            MessageISentBrushProperty.Changed.Subscribe(PropertyChanged);
            MessageTheySentBrushProperty.Changed.Subscribe(PropertyChanged);
            MessageSenderProperty.Changed.Subscribe(PropertyChanged);
        }

        /// <summary>
        /// Gets the Brush for when a message is sent by the user.
        /// </summary>
        /// <param name="element">The <see cref="Control"/ to assign the property value to.</param>
        /// <returns>Returns the <see cref="IBrush"/> used for user message sent.</returns>
        public static IBrush GetMessageISentBrush(Control element)
        {
            return (IBrush)element.GetValue(MessageISentBrushProperty);
        }

        /// <summary>
        /// Sets the Brush for when a message is sent by the user.
        /// </summary>
        /// <param name="element">The <see cref="Control"/ to assign the property value to.</param>
        /// <param name="value">The <see cref="Brush"/> to set.</param>
        public static void SetMessageISentBrush(Control element, IBrush value)
        {
            element.SetValue(MessageISentBrushProperty, value);
            UpdateData(element);
        }

        /// <summary>
        /// Gets the Brush for when a message is sent by another user.
        /// </summary>
        /// <param name="element">The <see cref="Control"/ to assign the property value to.</param>
        /// <returns>Returns the <see cref="IBrush"/> used for other users message sent.</returns>
        public static IBrush GetMessageTheySentBrush(Control element)
        {
            return (IBrush)element.GetValue(MessageTheySentBrushProperty);
        }

        /// <summary>
        /// Sets the Brush for when a message is sent by another user.
        /// </summary>
        /// <param name="element">The <see cref="Control"/ to assign the property value to.</param>
        /// <param name="value">The brush to change the Brush to.</param>
        public static void SetMessageTheySentBrush(Control element, IBrush value)
        {
            element.SetValue(MessageTheySentBrushProperty, value);
            UpdateData(element);
        }

        /// <summary>
        /// Gets the sender of this message.
        /// </summary>
        /// <param name="element">The <see cref="Control"/ to assign the property value to.</param>
        /// <returns>True if the sender is the current user.</returns>
        public static bool GetMessageSender(Control element)
        {
            return (bool)element.GetValue(MessageSenderProperty);
        }

        /// <summary>
        /// Sets the message sender status.
        /// </summary>
        /// <param name="element">The <see cref="Control"/ to assign the property value to.</param>
        /// <param name="value">The sender type value.</param>
        public static void SetMessageSender(Control element, bool value)
        {
            element.SetValue(MessageSenderProperty, value);
            UpdateData(element);
        }

        private static void PropertyChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is Grid gd)
            {
                UpdateData(gd);
            }
        }

        private static void UpdateData(Control element)
        {
            if (element is Grid grid)
            {
                if (GetMessageSender(element))
                {
                    grid.Background = GetMessageISentBrush(element);
                }
                else
                {
                    grid.Background = GetMessageTheySentBrush(element);
                }
            }
        }
    }
}
