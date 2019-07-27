using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GroupMeClient.ViewModels.Controls
{
    /// <summary>
    /// <see cref="MessageEffectsControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.MessageEffectsControl"/> control.
    /// </summary>
    public class MessageEffectsControlViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string typedMessageContents;
        private string selectedMessageContents;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageEffectsControlViewModel"/> class.
        /// </summary>
        public MessageEffectsControlViewModel()
        {
        }

        /// <summary>
        /// Gets a collection of generated messages produced by plugins.
        /// </summary>
        public ObservableCollection<SuggestedMessage> GeneratedMessages { get; } = new ObservableCollection<SuggestedMessage>();

        /// <summary>
        /// Gets or sets the command to be performed when the message is updated.
        /// </summary>
        public ICommand UpdateMessage { get; set; }

        /// <summary>
        /// Gets or sets the message the user has composed to send.
        /// </summary>
        public string TypedMessageContents
        {
            get
            {
                return this.typedMessageContents;
            }

            set
            {
                this.Set(() => this.TypedMessageContents, ref this.typedMessageContents, value);
                _ = this.GenerateResults();
            }
        }

        /// <summary>
        /// Gets or sets the message (with effects) the user has selected to send.
        /// </summary>
        public string SelectedMessageContents
        {
            get { return this.selectedMessageContents; }
            set { this.Set(() => this.SelectedMessageContents, ref this.selectedMessageContents, value); }
        }

        private async Task GenerateResults()
        {
            this.GeneratedMessages.Clear();

            foreach (var plugin in Plugins.PluginManager.Instance.MessageComposePlugins)
            {
                try
                {
                    var results = await plugin.ProvideOptions(this.TypedMessageContents);
                    foreach (var text in results.TextOptions)
                    {
                        this.GeneratedMessages.Add(new SuggestedMessage { Message = text, Plugin = plugin.EffectPluginName });
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// <see cref="SuggestedMessage"/> represents a generated result from a plugin
        /// for a message to suggest to the user.
        /// </summary>
        public class SuggestedMessage
        {
            /// <summary>
            /// Gets or sets the message contents.
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// Gets or sets the name of the plugin that generated this message.
            /// </summary>
            public string Plugin { get; set; }

            /// <inheritdoc />
            public override string ToString()
            {
                return this.Message;
            }
        }
    }
}
