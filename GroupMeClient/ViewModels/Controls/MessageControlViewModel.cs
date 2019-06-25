using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClientApi.Models;

namespace GroupMeClient.ViewModels.Controls
{
    public class MessageControlViewModel : ViewModelBase
    {
        public MessageControlViewModel()
        {
        }

        public MessageControlViewModel(Message message)
        {
            this.message = message;
        }

        private Message message;

        public Message Message
        {
            get
            {
                return this.message;
            }

            set
            {
                if (this.message == value)
                {
                    return;
                }

                this.message = value;
                RaisePropertyChanged("Message");
            }
        }

        public string Id => this.Message.Id;

        public string Text => this.Message.Text;

        public string Sender => this.Message.Name;

        public System.Windows.Media.Color Color
        {
            get
            {
                if (this.Message.Group != null)
                {
                    // TODO decide who sent the message
                    return System.Windows.Media.Colors.White;
                }
                else if (this.Message.Chat != null)
                {
                    return System.Windows.Media.Colors.White;
                }
                else
                {
                    return System.Windows.Media.Colors.White;
                }
            }
        }
    }
}
