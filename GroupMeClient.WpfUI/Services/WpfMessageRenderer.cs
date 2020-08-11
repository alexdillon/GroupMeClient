using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GroupMeClient.Core.Caching;
using GroupMeClient.Core.Services;
using GroupMeClient.Core.ViewModels.Controls;
using GroupMeClient.Core.ViewModels.Controls.Attachments;
using GroupMeClient.WpfUI.Utilities;
using GroupMeClient.WpfUI.Views.Controls;
using GroupMeClientApi.Models;

namespace GroupMeClient.WpfUI.Services
{
    /// <summary>
    /// <see cref="WpfMessageRenderer"/> provides an implementation of the <see cref="IMessageRendererService"/>
    /// for drawing messages to Bitmaps on the WPF platform.
    /// </summary>
    public class WpfMessageRenderer : IMessageRendererService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WpfMessageRenderer"/> class.
        /// </summary>
        /// <param name="cacheManager">The cache instance to use.</param>
        public WpfMessageRenderer(CacheManager cacheManager)
        {
            this.CacheManager = cacheManager;
        }

        private CacheManager CacheManager { get; }

        /// <inheritdoc/>
        public byte[] RenderMessageToPngImage(Message message, MessageControlViewModelBase displayedMessage)
        {
            var messageDataContext = new MessageControlViewModel(message, this.CacheManager, false, true, 1);

            // Copy the attachments from the version of the message that is already rendered and displayed.
            // These attachments already have previews downloaded and ready-to-render.
            messageDataContext.AttachedItems.Clear();
            this.FixImagesInReplyBitmaps(displayedMessage as MessageControlViewModel, messageDataContext);

            var messageControl = this.DuplicateMessage(messageDataContext);
            messageControl.Measure(new Size(500, double.PositiveInfinity));
            messageControl.ApplyTemplate();
            messageControl.UpdateLayout();
            var desiredSize = messageControl.DesiredSize;
            desiredSize.Width = Math.Max(300, desiredSize.Width);
            /*desiredSize.Height = Math.Min(250, desiredSize.Height);*/
            messageControl.Arrange(new Rect(new Point(0, 0), desiredSize));

            var bmp = new RenderTargetBitmap((int)messageControl.RenderSize.Width, (int)desiredSize.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(messageControl);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

            using (var quotedMessageRenderPng = new MemoryStream())
            {
                encoder.Save(quotedMessageRenderPng);
                return quotedMessageRenderPng.ToArray();
            }
        }

        private MessageControl DuplicateMessage(MessageControlViewModel viewModel)
        {
            return new MessageControl()
            {
                DataContext = viewModel,
                Background = (Brush)Application.Current.FindResource("MessageTheySentBackdropBrush"),
                Foreground = (Brush)Application.Current.FindResource("MahApps.Brushes.ThemeForeground"),
            };
        }

        private void FixImagesInReplyBitmaps(MessageControlViewModel displayedMessage, MessageControlViewModel renderTarget)
        {
            var newAttachments = new List<object>();

            foreach (var attachment in displayedMessage.AttachedItems)
            {
                // Images don't render correctly as-is due to the usage of the GIF attached property.
                if (attachment is GroupMeImageAttachmentControlViewModel gmImage)
                {
                    byte[] imageBytes = null;
                    using (var memoryStream = new MemoryStream())
                    {
                        gmImage.ImageAttachmentStream.Seek(0, SeekOrigin.Begin);
                        gmImage.ImageAttachmentStream.CopyTo(memoryStream);
                        imageBytes = memoryStream.ToArray();
                    }

                    newAttachments.Add(new System.Windows.Controls.Image()
                    {
                        Source = ImageUtils.BytesToImageSource(imageBytes),
                    });
                }
                else if (attachment is ImageLinkAttachmentControlViewModel linkedImage)
                {
                    // Linked Images aren't downloaded on the ViewModel side
                    // Just include the URL of the image
                    newAttachments.Add($"Image: {linkedImage.Url}");
                }
                else
                {
                    newAttachments.Add(attachment);
                }
            }

            renderTarget.AttachedItems.Clear();
            foreach (var newAttachment in newAttachments)
            {
                renderTarget.AttachedItems.Add(newAttachment);
            }

            if (displayedMessage.RepliedMessage != null)
            {
                var copyOfReplyMessage = new MessageControlViewModel(displayedMessage.RepliedMessage.Message);
                var copyOfReplyAttachment = new RepliedMessageControlViewModel(copyOfReplyMessage);
                this.FixImagesInReplyBitmaps(displayedMessage.RepliedMessage.Message, copyOfReplyMessage);
                renderTarget.RepliedMessage = copyOfReplyAttachment;
            }
        }
    }
}
