using System;
using System.Windows.Data;
using MahApps.Metro.IconPacks;

namespace GroupMeClient.Wpf.Converters
{
    /// <summary>
    /// <see cref="LikeStatusToIcon"/> provides a converter to get the correct
    /// icon shape from a <see cref="Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions"/>.
    /// </summary>
    [ValueConversion(typeof(Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions), typeof(PackIconFontAwesomeKind))]
    public class LikeStatusToIcon : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions likeStatus)
            {
                switch (likeStatus)
                {
                    case Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions.NoLikers:
                        return PackIconFontAwesomeKind.HeartRegular;

                    case Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions.OthersLiked:
                    case Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions.SelfLiked:
                        return PackIconFontAwesomeKind.HeartSolid;

                    case Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions.HiddenLikers:
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
