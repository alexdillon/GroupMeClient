using System.IO;

namespace GroupMeClient.ViewModels.Controls
{
    /// <summary>
    /// <see cref="ViewImageControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.ViewImageControl"/> control.
    /// </summary>
    public class ViewImageControlViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewImageControlViewModel"/> class.
        /// </summary>
        /// <param name="stream">The image stream to display.</param>
        public ViewImageControlViewModel(Stream stream)
        {
            this.ImageStream = stream;
        }

        /// <summary>
        /// Gets or sets the image stream to preview the image.
        /// </summary>
        public Stream ImageStream { get; set; }
    }
}
