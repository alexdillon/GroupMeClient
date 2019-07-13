using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GroupMeClientApi.Models.Attachments
{
    /// <summary>
    /// Represents an attachment to a GroupMe <see cref="Message"/> containing an image.
    /// </summary>
    public class ImageAttachment : Attachment
    {
        /// <inheritdoc/>
        [JsonProperty("type")]
        public override string Type { get; } = "image";

        /// <summary>
        /// Gets the URL of the image attachment.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; internal set; }

        private static string GroupMeImageApiUrl => "https://image.groupme.com/pictures";

        /// <summary>
        /// Uploads an image to GroupMe and returns the created <see cref="ImageAttachment"/>.
        /// </summary>
        /// <param name="image">The image to upload.</param>
        /// <param name="group">The <see cref="Group"/> that the message is being sent to.</param>
        /// <returns>An <see cref="ImageAttachment"/> if uploaded successfully, null otherwise.</returns>
        public static async Task<ImageAttachment> CreateImageAttachment(byte[] image, Group group)
        {
            return await CreateImageAttachment(image, group.Client);
        }

        /// <summary>
        /// Uploads an image to GroupMe and returns the created <see cref="ImageAttachment"/>.
        /// </summary>
        /// <param name="image">The image to upload.</param>
        /// <param name="chat">The <see cref="Chat"/> that the message is being sent to.</param>
        /// <returns>An <see cref="ImageAttachment"/> if uploaded successfully, null otherwise.</returns>
        public static async Task<ImageAttachment> CreateImageAttachment(byte[] image, Chat chat)
        {
            return await CreateImageAttachment(image, chat.Client);
        }

        /// <summary>
        /// Uploads an image to GroupMe and returns the created <see cref="ImageAttachment"/>.
        /// </summary>
        /// <param name="image">The image to upload.</param>
        /// <param name="client">The <see cref="GroupMeClient"/> that should be used to perform the request.</param>
        /// <returns>An <see cref="ImageAttachment"/> if uploaded successfully, null otherwise.</returns>
        private static async Task<ImageAttachment> CreateImageAttachment(byte[] image, GroupMeClient client)
        {
            var request = client.CreateRestRequest(ImageAttachment.GroupMeImageApiUrl, RestSharp.Method.POST);
            request.AddParameter("image/jpeg", image, RestSharp.ParameterType.RequestBody);

            var cancellationTokenSource = new CancellationTokenSource();
            var restResponse = await client.ApiClient.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            if (restResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = JsonConvert.DeserializeObject<ImageUploadResponse>(restResponse.Content);

                return new ImageAttachment()
                {
                    Url = result.Payload.Url,
                };
            }
            else
            {
                return null;
            }
        }

        private class ImageUploadResponse
        {
            [JsonProperty("payload")]
            public UploadPayload Payload { get; internal set; }

            public class UploadPayload
            {
                [JsonProperty("url")]
                public string Url { get; internal set; }

                [JsonProperty("picture_url")]
                public string PictureUrl { get; internal set; }
            }
        }
    }
}