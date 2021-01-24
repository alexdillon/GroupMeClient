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
        /// An Avalonia Attached property to represent the rendering brush of the icon when the current user likes the message.
        /// </summary>
        public static readonly AvaloniaProperty<IBrush> BrushWhenILikedProperty =
            AvaloniaProperty.RegisterAttached<IconControl, IBrush>(
                "BrushWhenILiked",
                typeof(LikeIconExtensions),
                defaultValue: default(SolidColorBrush));

        /// <summary>
        /// An Avalonia Attached property to represent the rendering brush of the icon when the other users likes the message.
        /// </summary>
        public static readonly AvaloniaProperty<IBrush> BrushWhenTheyLikedProperty =
            AvaloniaProperty.RegisterAttached<IconControl, IBrush>(
                "BrushWhenTheyLiked",
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
            BrushWhenILikedProperty.Changed.Subscribe(PropertyChanged);
            BrushWhenTheyLikedProperty.Changed.Subscribe(PropertyChanged);
            LikeStatusProperty.Changed.Subscribe(PropertyChanged);
        }

        /// <summary>
        /// Gets the brush to render with when the current user likes the message.
        /// </summary>
        /// <param name="element">The <see cref="Control"/> to retreive the property from.</param>
        /// <returns>An <see cref="IBrush"/>.</returns>
        public static IBrush GetBrushWhenILiked(Control element)
        {
            return element.GetValue(BrushWhenILikedProperty) as IBrush;
        }

        /// <summary>
        /// Sets the brush to render with when the current user likes the message.
        /// </summary>
        /// <param name="element">The <see cref="Control"/> to assign the property value to.</param>
        /// <param name="value">The new value to assign.</param>
        public static void SetBrushWhenILiked(Control element, IBrush value)
        {
            element.SetValue(BrushWhenILikedProperty, value);
            UpdateData(element);
        }

        /// <summary>
        /// Gets the brush to render with when the other users likes the message.
        /// </summary>
        /// <param name="element">The <see cref="Control"/> to retreive the property from.</param>
        /// <returns>An <see cref="IBrush"/>.</returns>
        public static IBrush GetBrushWhenTheyLiked(Control element)
        {
            return element.GetValue(BrushWhenTheyLikedProperty) as IBrush;
           
        }

        /// <summary>
        /// Sets the brush to render with when other users like the message.
        /// </summary>
        /// <param name="element">The <see cref="Control"/> to assign the property value to.</param>
        /// <param name="value">The new value to assign.</param>
        public static void SetBrushWhenTheyLiked(Control element, IBrush value)
        {
            element.SetValue(BrushWhenTheyLikedProperty, value);
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
                        iconControl.Foreground = GetBrushWhenILiked(element);
                        break;

                    case MessageControlViewModel.LikeStatusOptions.OthersLiked:
                        iconControl.BindableKind = MicroCubeAvalonia.IconPack.Icons.PackIconFontAwesomeKind.HeartSolid;
                        iconControl.Foreground = GetBrushWhenTheyLiked(element);
                        break;

                    case MessageControlViewModel.LikeStatusOptions.HiddenLikers:
                        iconControl.BindableKind = MicroCubeAvalonia.IconPack.Icons.PackIconFontAwesomeKind.None;
                        break;

                    default:
                    case MessageControlViewModel.LikeStatusOptions.NoLikers:
                        iconControl.BindableKind = MicroCubeAvalonia.IconPack.Icons.PackIconFontAwesomeKind.HeartRegular;
                        iconControl.Foreground = GetBrushWhenTheyLiked(element);
                        break;
                }
            }
        }
    }
}
