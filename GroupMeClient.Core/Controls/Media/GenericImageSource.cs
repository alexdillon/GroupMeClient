using System;

namespace GroupMeClient.Core.Controls.Media
{
    /// <summary>
    /// Abstract container for raw image data.
    /// </summary>
    public class GenericImageSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericImageSource"/> class.
        /// </summary>
        /// <param name="rawImageData">The image data.</param>
        public GenericImageSource(byte[] rawImageData)
        {
            this.RawImageData = rawImageData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericImageSource"/> class.
        /// </summary>
        /// <param name="rawImageData">The raw image data.</param>
        /// <param name="renderWidth">The render width of the image.</param>
        /// <param name="renderHeight">The render height of the image.</param>
        public GenericImageSource(byte[] rawImageData, int renderWidth, int renderHeight)
        {
            this.RawImageData = rawImageData;
            this.RenderWidth = renderWidth;
            this.RenderHeight = renderHeight;
        }

        /// <summary>
        /// Gets the raw image data.
        /// </summary>
        public byte[] RawImageData { get; }

        /// <summary>
        /// Gets or sets the width the image is rendered at.
        /// </summary>
        public int RenderWidth { get; set; }

        /// <summary>
        /// Gets or sets the height the image is rendered at.
        /// </summary>
        public int RenderHeight { get; set; }
    }
}
