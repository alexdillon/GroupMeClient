using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GroupMeClient.Core.Caching;
using GroupMeClientApi.Models;

namespace GroupMeClient.Core.ViewModels.Controls.Attachments
{
    /// <summary>
    /// <see cref="RepliedMessageControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.Attachments.RepliedMessageControl"/> control.
    /// </summary>
    public class RepliedMessageControlViewModel : ViewModelBase
    {
        private MessageControlViewModel message;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepliedMessageControlViewModel"/> class.
        /// </summary>
        /// <param name="originalMessageId">The message id of the original message that is being replied to.</param>
        /// <param name="messageContainer">The message container in which the original message is contained.</param>
        /// <param name="nestLevel">The number of attachment deeply nested this <see cref="Message"/> is.</param>
        public RepliedMessageControlViewModel(string originalMessageId, IMessageContainer messageContainer, int nestLevel)
        {
            var cacheManager = SimpleIoc.Default.GetInstance<CacheManager>();
            using (var context = cacheManager.OpenNewContext())
            {
                var originalMessage = context.Messages.Find(originalMessageId);
                if (originalMessage == null)
                {
                    // problem
                }
                else
                {
                    if (messageContainer is Group g)
                    {
                        originalMessage.AssociateWithGroup(g);
                    }
                    else if (messageContainer is Chat c)
                    {
                        originalMessage.AssociateWithChat(c);
                    }

                    this.Message = new MessageControlViewModel(originalMessage, false, true, nestLevel + 1);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RepliedMessageControlViewModel"/> class.
        /// </summary>
        /// <param name="nestedMessage">An pre-configured viewmodel that can be used to initialize the <see cref="Message"/> property.</param>
        public RepliedMessageControlViewModel(MessageControlViewModel nestedMessage)
        {
            this.Message = nestedMessage;
        }

        /// <summary>
        /// Gets or sets the original <see cref="MessageControlViewModel"/> containing the <see cref="Message"/> that is being replied to.
        /// </summary>
        public MessageControlViewModel Message
        {
            get => this.message;
            set => this.Set(() => this.Message, ref this.message, value);
        }
    }
}
