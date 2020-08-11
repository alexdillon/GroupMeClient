using System;
using Avalonia.Data.Converters;

namespace GroupMeClient.AvaloniaUI.Converters
{
    /// <summary>
    /// <see cref="LikeStatusToBool"/> provides a converter to get a the correct
    /// a boolean corresponding to a Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions.
    /// The boolean value indicates whether the current user has Liked the message.
    /// </summary>
    public class LikeStatusToBool : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is GroupMeClient.Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions likeStatus)
            {
                switch (likeStatus)
                {
                    case GroupMeClient.Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions.NoLikers:
                    case GroupMeClient.Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions.OthersLiked:
                    case GroupMeClient.Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions.HiddenLikers:
                        return false;

                    case GroupMeClient.Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions.SelfLiked:
                        return true;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
