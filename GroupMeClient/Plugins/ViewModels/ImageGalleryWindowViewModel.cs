using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClient.Plugins.Views;
using GroupMeClientApi;
using GroupMeClientApi.Models;
using GroupMeClientApi.Models.Attachments;
using GroupMeClientPlugin;
using GroupMeClientPlugin.GroupChat;

namespace GroupMeClient.Plugins.ViewModels
{
    /// <summary>
    /// <see cref="ImageGalleryWindowViewModel"/> provides a ViewModel for the <see cref="ImageGalleryWindow"/> Window.
    /// </summary>
    public class ImageGalleryWindowViewModel : ViewModelBase
    {
        private ViewModelBase dialog;

        private ImageGalleryWindowViewModel(IMessageContainer groupChat, CacheSession cacheSession, IPluginUIIntegration uiIntegration)
        {
            this.GroupChat = groupChat;
            this.CacheSession = cacheSession;
            this.UIIntegration = uiIntegration;

            this.GroupName = this.GroupChat.Name;

            this.ShowImageDetailsCommand = new RelayCommand<AttachmentImageItem>(this.ShowImageDetails);
            this.ClosePopup = new RelayCommand(this.ClosePopupHandler);
            this.EasyClosePopup = null; // EasyClose makes it too easy to accidently close the send dialog.

            this.MessagesWithAttachments =
               this.CacheSession.CacheForGroupOrChat
                   .AsEnumerable()
                   .Where(m => m.Attachments.Count > 0)
                   .OrderByDescending(m => m.CreatedAtTime);

            this.Images = new ObservableCollection<AttachmentImageItem>();
            this.LoadPage(0);
        }

        /// <summary>
        /// Gets a collection of the <see cref="AttachmentImageItem"/>s that should be displayed in this Gallery.
        /// </summary>
        public ObservableCollection<AttachmentImageItem> Images { get; }

        /// <summary>
        /// Gets the name of the <see cref="IMessageContainer"/> this Gallery is displayed for.
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// Gets the command to execute to show a detailed view for a particular attached image.
        /// </summary>
        public ICommand ShowImageDetailsCommand { get; }

        /// <summary>
        /// Gets the action to be be performed when a little popup has been closed.
        /// </summary>
        public ICommand ClosePopup { get; }

        /// <summary>
        /// Gets the action to be performed when the big popup has been closed indirectly.
        /// This typically is from the user clicking in the gray area around the popup to dismiss it.
        /// </summary>
        public ICommand EasyClosePopup { get; }

        /// <summary>
        /// Gets or sets the Dialog that should be displayed as a popup.
        /// Gets null if no dialog should be displayed.
        /// </summary>
        public ViewModelBase Dialog
        {
            get { return this.dialog; }
            set { this.Set(() => this.Dialog, ref this.dialog, value); }
        }

        private IMessageContainer GroupChat { get; }

        private CacheSession CacheSession { get; }

        private IPluginUIIntegration UIIntegration { get; }

        private IEnumerable<Message> MessagesWithAttachments { get; }

        private int ImagesPerPage { get; } = 100;

        /// <summary>
        /// <see cref="GetAttachmentContentUrls(IEnumerable{Attachment})"/> returns a listing of the
        /// URLs for all images, linked images, and video attachments in the provided <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="attachments">The list of <see cref="Attachment"/>s to scan through.</param>
        /// <returns>An array of content URLs.</returns>
        public static string[] GetAttachmentContentUrls(IEnumerable<Attachment> attachments)
        {
            var results = new List<string>();
            foreach (var attachment in attachments)
            {
                if (attachment is ImageAttachment imageAttachment)
                {
                    results.Add($"{imageAttachment.Url}");
                }
                else if (attachment is LinkedImageAttachment linkedImageAttachment)
                {
                    results.Add($"{linkedImageAttachment.Url}");
                }
                else if (attachment is VideoAttachment videoAttachment)
                {
                    results.Add(videoAttachment.PreviewUrl);
                }
            }

            return results.ToArray();
        }

        private void LoadPage(int friendlyPageNumber)
        {
            if (this.MessagesWithAttachments == null)
            {
                return;
            }

            var pageNumber = friendlyPageNumber - 1;

            var range = this.MessagesWithAttachments.Skip(pageNumber * this.ImagesPerPage).Take(this.ImagesPerPage);

            foreach (var msg in range)
            {
                var imageUrls = GetAttachmentContentUrls(msg.Attachments);
                for (int i = 0; i < imageUrls.Length; i++)
                {
                    if (!string.IsNullOrEmpty(imageUrls[i]))
                    {
                        var entry = new AttachmentImageItem($"{imageUrls[i]}.preview", msg.Id, i, this.GroupChat.Client.ImageDownloader);
                        this.Images.Add(entry);
                    }
                }
            }
        }

        private void ShowImageDetails(AttachmentImageItem item)
        {
            var message = this.MessagesWithAttachments.FirstOrDefault(m => m.Id == item.MessageId);

            if (message == null)
            {
                return;
            }

            var dialog = new ImageDetailsControlViewModel(message, item.ImageIndex, this.GroupChat.Client.ImageDownloader);
            this.Dialog = dialog;
        }

        private void ClosePopupHandler()
        {
            (this.Dialog as IDisposable)?.Dispose();
            this.Dialog = null;
        }

        /// <summary>
        /// <see cref="AttachmentImageItem"/> represents each image that will be shown in the gallery.
        /// </summary>
        public class AttachmentImageItem : ViewModelBase
        {
            private bool isLoading;
            private Stream imageData;

            /// <summary>
            /// Initializes a new instance of the <see cref="AttachmentImageItem"/> class.
            /// </summary>
            /// <param name="url">The URL of the image.</param>
            /// <param name="messageId">The ID of the <see cref="Message"/> this image was sent with.</param>
            /// <param name="imageIndex">The index of this image out of all the images attached to the same <see cref="Message"/>.</param>
            /// <param name="downloader">The <see cref="GroupMeClientApi.ImageDownloader"/> that should be used to download images.</param>
            public AttachmentImageItem(string url, string messageId, int imageIndex, ImageDownloader downloader)
            {
                this.MessageId = messageId;
                this.Url = url;
                this.ImageIndex = imageIndex;
                this.ImageDownloader = downloader;

                _ = this.LoadImage();
            }

            /// <summary>
            /// Gets a stream containing the image data to display.
            /// </summary>
            public Stream ImageData
            {
                get => this.imageData;
                private set => this.Set(() => this.ImageData, ref this.imageData, value);
            }

            /// <summary>
            /// Gets a value indicating whether the image is still loading.
            /// </summary>
            public bool IsLoading
            {
                get => this.isLoading;
                private set => this.Set(() => this.IsLoading, ref this.isLoading, value);
            }

            /// <summary>
            /// Gets the unique identifier of the <see cref="Message"/> this image was attached to.
            /// </summary>
            public string MessageId { get; }

            /// <summary>
            /// Gets the index number of this image out of the collection of all images attached to a given <see cref="Message"/>.
            /// </summary>
            public int ImageIndex { get; }

            private string Url { get; }

            private ImageDownloader ImageDownloader { get; }

            private async Task LoadImage()
            {
                this.IsLoading = true;

                var image = await this.ImageDownloader.DownloadPostImageAsync(this.Url);

                if (image == null)
                {
                    return;
                }

                this.ImageData = new MemoryStream(image);

                this.IsLoading = false;
            }
        }

        /// <summary>
        /// <see cref="ImageGalleryPlugin"/> defines a GroupMe Desktop Client Plugin that can be used
        /// to display an image gallery for a specific group or chat.
        /// </summary>
        public class ImageGalleryPlugin : PluginBase, IGroupChatPlugin
        {
            /// <inheritdoc/>
            public string PluginName => "Image Gallery New";

            /// <inheritdoc/>
            public override string PluginDisplayName => this.PluginName;

            /// <inheritdoc/>
            public override string PluginVersion => ThisAssembly.SimpleVersion;

            /// <inheritdoc/>
            public override Version ApiVersion => new Version(2, 0, 0);

            /// <inheritdoc/>
            public Task Activated(IMessageContainer groupOrChat, CacheSession cacheSession, IPluginUIIntegration integration, Action<CacheSession> cleanup)
            {
                var dataContext = new ImageGalleryWindowViewModel(groupOrChat, cacheSession, integration);
                var window = new ImageGalleryWindow();

                window.DataContext = dataContext;
                window.Closing += (s, e) =>
                {
                    cleanup(cacheSession);
                };

                Application.Current.Dispatcher.Invoke(() =>
                {
                    window.Show();
                });

                return Task.CompletedTask;
            }
        }
    }
}
