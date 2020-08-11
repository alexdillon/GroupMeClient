using System;
using Avalonia.Data.Converters;
using MicroCubeAvalonia.IconPack.Icons;

namespace GroupMeClient.AvaloniaUI.Converters
{
    /// <summary>
    /// <see cref="LikeStatusToIcon"/> provides a converter to get the correct
    /// icon shape from a <see cref="Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions"/>.
    /// </summary>
    public class LikeStatusToIcon : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is GroupMeClient.Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions likeStatus)
            {
                switch (likeStatus)
                {
                    case GroupMeClient.Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions.NoLikers:
                        return PackIconFontAwesomeKind.HeartRegular;

                    case GroupMeClient.Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions.OthersLiked:
                    case GroupMeClient.Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions.SelfLiked:
                        return PackIconFontAwesomeKind.HeartSolid;

                    case GroupMeClient.Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions.HiddenLikers:
                    default:
                        return PackIconFontAwesomeKind.None;
                }
            }

            return PackIconFontAwesomeKind.None;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
