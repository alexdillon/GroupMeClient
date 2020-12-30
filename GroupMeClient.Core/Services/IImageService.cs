namespace GroupMeClient.Core.Services
{
    /// <summary>
    /// <see cref="IImageService"/> provides a platform agnostic service for image generation.
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// Creates a transparent placeholder PNG image of a specific size.
        /// </summary>
        /// <param name="width">The width of the image to generate.</param>
        /// <param name="height">The height of the image to generate.</param>
        /// <returns>A byte array containing the a transparent image encoded in PNG format.</returns>
        byte[] CreateTransparentPng(int width, int height);
    }
}
