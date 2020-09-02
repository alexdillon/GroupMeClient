using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MicroCubeAvalonia.IconPack;

namespace GroupMeClient.AvaloniaUI.Extensions
{
    /// <summary>
    /// <see cref="MessageColorExtensions"/> provides extension methods for <see cref="Grid"/> for displaying correct background color.
    /// </summary>
    public class MessageColorExtensions
    {
        /// <summary>
        /// An Avalonia Attached property to represent the rendering color of the message background when the user is the sender.
        /// </summary>
        public static readonly AvaloniaProperty<IBrush> MessageISentColorProperty =
               AvaloniaProperty.RegisterAttached<Grid, IBrush>(
                "MessageISentColor",
                typeof(MessageColorExtensions),
                defaultValue: default(SolidColorBrush));

        /// <summary>
        /// An Avalonia Attached property to represent the rendering color of the message background when another user is the sender.
        /// </summary>
        public static readonly AvaloniaProperty<IBrush> MessageTheySentColorProperty =
               AvaloniaProperty.RegisterAttached<Grid, IBrush>(
                "MessageTheySentColor",
                typeof(MessageColorExtensions),
                defaultValue: default(SolidColorBrush));

        /// <summary>
        /// An Avalonia Attached property to represent if the sender was the user.
        /// </summary>
        public static readonly AvaloniaProperty<bool> MessageSenderProperty =
               AvaloniaProperty.RegisterAttached<Grid, bool>(
                "MessageSender",
                typeof(MessageColorExtensions),
                defaultValue: default(bool));

        /// <summary>
        /// Initializes static members of the <see cref="MessageColorExtensions"/> class.
        /// </summary>
        static MessageColorExtensions()
        {
            MessageISentColorProperty.Changed.Subscribe(PropertyChanged);
            MessageTheySentColorProperty.Changed.Subscribe(PropertyChanged);
            MessageSenderProperty.Changed.Subscribe(PropertyChanged);
        }

        /// <summary>
        /// Gets the color for when a message is sent by the user.
        /// </summary>
        /// <param name="element">The <see cref="Control"/ to assign the property value to.</param>
        /// <returns>Returns the <see cref="IBrush"/> used for user message sent.</returns>
        public static IBrush GetMessageISentColor(Control element)
        {
            return element.GetValue(MessageISentColorProperty);
        }

        /// <summary>
        /// Sets the color for when a message is sent by the user.
        /// </summary>
        /// <param name="element">The <see cref="Control"/ to assign the property value to.</param>
        /// <param name="brush">The <see cref="Brush"/> to set.</param>
        public static void SetMessageISentColor(Control element, IBrush brush)
        {
            element.SetValue(MessageISentColorProperty, brush);
            UpdateData(element);
        }

        /// <summary>
        /// Gets the color for when a message is sent by another user.
        /// </summary>
        /// <param name="element">The <see cref="Control"/ to assign the property value to.</param>
        /// <returns>Returns the <see cref="IBrush"/> used for other users message sent.</returns>
        public static IBrush GetMessageTheySentColor(Control element)
        {
            return element.GetValue(MessageTheySentColorProperty);
        }

        /// <summary>
        /// Sets the color for when a message is sent by another user.
        /// </summary>
        /// <param name="element">The <see cref="Control"/ to assign the property value to.</param>
        /// <param name="brush">The brush to change the color to.</param>
        public static void SetMessageTheySentColor(Control element, IBrush brush)
        {
            element.SetValue(MessageTheySentColorProperty, brush);
            UpdateData(element);
        }

        /// <summary>
        /// Gets the sender of this message.
        /// </summary>
        /// <param name="element">The <see cref="Control"/ to assign the property value to.</param>
        /// <returns>True if the sender is the current user.</returns>
        public static bool GetMessageSender(Control element)
        {
            return element.GetValue(MessageSenderProperty);
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
            if (element is Grid Grid)
            {
                if (GetMessageSender(element))
                {
                    Grid.Background = GetMessageISentColor(element);
                }
                else
                {
                    Grid.Background = GetMessageTheySentColor(element);
                }
            }
        }
    }
}
