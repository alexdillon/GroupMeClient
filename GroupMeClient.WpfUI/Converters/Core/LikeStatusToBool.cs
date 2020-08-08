using System;
using System.Windows.Data;

namespace GroupMeClient.WpfUI.Converters
{
    /// <summary>
    /// <see cref="LikeStatusToBool"/> provides a converter to get a the correct
    /// a boolean corresponding to a Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions.
    /// The boolean value indicates whether the current user has Liked the message.
    /// </summary>
    [ValueConversion(typeof(Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions), typeof(bool))]
    public class LikeStatusToBool : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions likeStatus)
            {
                switch (likeStatus)
                {
                    case Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions.NoLikers:
                    case Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions.OthersLiked:
                    case Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions.HiddenLikers:
                        return false;

                    case Core.ViewModels.Controls.MessageControlViewModel.LikeStatusOptions.SelfLiked:
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
