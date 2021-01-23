using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using GroupMeClient.Core.ViewModels.Controls;
using MicroCubeAvalonia.IconPack;

namespace GroupMeClient.AvaloniaUI.Extensions
{
    /// <summary>
    /// <see cref="LikeIconExtensions"/> provides extension methods for <see cref="IconControl"/> to allow for displaying like statuses.
    /// </summary>
    public class LikeIconExtensions
    {
        /// <summary>
        /// An Avalonia Attached property to represent the rendering color of the icon when the current user likes the message.
        /// </summary>
        public static readonly AvaloniaProperty<IBrush> ColorWhenILikedProperty =
            AvaloniaProperty.RegisterAttached<IconControl, IBrush>(
                "ColorWhenILiked",
                typeof(LikeIconExtensions),
                defaultValue: default(SolidColorBrush));

        /// <summary>
        /// An Avalonia Attached property to represent the rendering color of the icon when the other users likes the message.
        /// </summary>
        public static readonly AvaloniaProperty<IBrush> ColorWhenTheyLikedProperty =
            AvaloniaProperty.RegisterAttached<IconControl, IBrush>(
                "ColorWhenTheyLiked",
                typeof(LikeIconExtensions),
                defaultValue: default(SolidColorBrush));

        /// <summary>
        /// An Avalonia Attached property to represent like status this control is displaying.
        /// </summary>
        public static readonly AvaloniaProperty<MessageControlViewModel.LikeStatusOptions> LikeStatusProperty =
           AvaloniaProperty.RegisterAttached<IconControl, MessageControlViewModel.LikeStatusOptions>(
               "LikeStatus",
               typeof(LikeIconExtensions),
               defaultValue: MessageControlViewModel.LikeStatusOptions.HiddenLikers);

        /// <summary>
        /// Initializes static members of the <see cref="LikeIconExtensions"/> class.
        /// </summary>
        static LikeIconExtensions()
        {
            ColorWhenILikedProperty.Changed.Subscribe(PropertyChanged);
            ColorWhenTheyLikedProperty.Changed.Subscribe(PropertyChanged);
            LikeStatusProperty.Changed.Subscribe(PropertyChanged);
        }

        /// <summary>
        /// Gets the brush color to render with when the current user likes the message.
        /// </summary>
        /// <param name="element">The <see cref="Control"/> to retreive the property from.</param>
        /// <returns>An <see cref="IBrush"/>.</returns>
        public static IBrush GetColorWhenILiked(Control element)
        {
            return element.GetValue(ColorWhenILikedProperty) as IBrush;
        }

        /// <summary>
        /// Sets the brush color to render with when the current user likes the message.
        /// </summary>
        /// <param name="element">The <see cref="Control"/> to assign the property value to.</param>
        /// <param name="value">The new value to assign.</param>
        public static void SetColorWhenILiked(Control element, IBrush value)
        {
            element.SetValue(ColorWhenILikedProperty, value);
            UpdateData(element);
        }

        /// <summary>
        /// Gets the brush color to render with when the other users likes the message.
        /// </summary>
        /// <param name="element">The <see cref="Control"/> to retreive the property from.</param>
        /// <returns>An <see cref="IBrush"/>.</returns>
        public static IBrush GetColorWhenTheyLiked(Control element)
        {
            return element.GetValue(ColorWhenTheyLikedProperty) as IBrush;
        }

        /// <summary>
        /// Sets the brush color to render with when other users like the message.
        /// </summary>
        /// <param name="element">The <see cref="Control"/> to assign the property value to.</param>
        /// <param name="value">The new value to assign.</param>
        public static void SetColorWhenTheyLiked(Control element, IBrush value)
        {
            element.SetValue(ColorWhenTheyLikedProperty, value);
            UpdateData(element);
        }

        /// <summary>
        /// Gets the like status that is being displayed.
        /// </summary>
        /// <param name="element">The <see cref="Control"/> to retreive the property from.</param>
        /// <returns>The associated like status.</returns>
        public static MessageControlViewModel.LikeStatusOptions GetLikeStatus(Control element)
        {
            return (MessageControlViewModel.LikeStatusOptions)element.GetValue(LikeStatusProperty);
        }

        /// <summary>
        /// Sets like status of this control.
        /// </summary>
        /// <param name="element">The <see cref="Control"/> to assign the property value to.</param>
        /// <param name="value">The new value to assign.</param>
        public static void SetLikeStatus(Control element, MessageControlViewModel.LikeStatusOptions value)
        {
            element.SetValue(LikeStatusProperty, value);
            UpdateData(element);
        }

        private static void PropertyChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is IconControl ic)
            {
                UpdateData(ic);
            }
        }

        private static void UpdateData(Control element)
        {
            if (element is IconControl iconControl)
            {
                switch (GetLikeStatus(element))
                {
                    case MessageControlViewModel.LikeStatusOptions.SelfLiked:
                        iconControl.BindableKind = MicroCubeAvalonia.IconPack.Icons.PackIconFontAwesomeKind.HeartSolid;
                        iconControl.Foreground = GetColorWhenILiked(element);
                        break;

                    case MessageControlViewModel.LikeStatusOptions.OthersLiked:
                        iconControl.BindableKind = MicroCubeAvalonia.IconPack.Icons.PackIconFontAwesomeKind.HeartSolid;
                        iconControl.Foreground = GetColorWhenTheyLiked(element);
                        break;

                    case MessageControlViewModel.LikeStatusOptions.HiddenLikers:
                        iconControl.BindableKind = MicroCubeAvalonia.IconPack.Icons.PackIconFontAwesomeKind.None;
                        break;

                    default:
                    case MessageControlViewModel.LikeStatusOptions.NoLikers:
                        iconControl.BindableKind = MicroCubeAvalonia.IconPack.Icons.PackIconFontAwesomeKind.HeartRegular;
                        iconControl.Foreground = GetColorWhenTheyLiked(element);
                        break;
                }
            }
        }
    }
}
