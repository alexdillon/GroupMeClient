using System;
using System.Collections.ObjectModel;
using System.Threading;
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

                if (this.GeneratorCancel != null)
                {
                    this.GeneratorCancel.Cancel();
                }

                this.GeneratedMessages.Clear();

                this.GeneratorCancel = new CancellationTokenSource();
                Task.Run(() => this.GenerateResults(this.GeneratorCancel.Token), this.GeneratorCancel.Token);
            }
        }

        /// <summary>
        /// Gets or sets the message (with effects) the user has selected to send.
        /// </summary>
        public string SelectedMessageContents
        {
            get => this.selectedMessageContents;
            set => this.Set(() => this.SelectedMessageContents, ref this.selectedMessageContents, value);
        }

        private CancellationTokenSource GeneratorCancel { get; set; }

        private void GenerateResults(CancellationToken cancellationToken)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                this.GeneratedMessages.Clear();
            });

            var parallelOptions = new ParallelOptions()
            {
                CancellationToken = cancellationToken,
            };

            // Run all generators in parallel in case one plugin hangs or runs very slowly
            Parallel.ForEach(Plugins.PluginManager.Instance.MessageComposePlugins, parallelOptions, async (plugin) =>
            {
                if (parallelOptions.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    var results = await plugin.ProvideOptions(this.TypedMessageContents);
                    foreach (var text in results.TextOptions)
                    {
                        if (parallelOptions.CancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        var textResults = new SuggestedMessage { Message = text, Plugin = plugin.EffectPluginName };

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            this.GeneratedMessages.Add(textResults);
                        });
                    }
                }
                catch (Exception)
                {
                }
            });
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
